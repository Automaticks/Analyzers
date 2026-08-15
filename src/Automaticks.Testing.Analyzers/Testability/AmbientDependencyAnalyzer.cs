using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.Testing.Testability;

/// <summary>
///     Flags direct use of ambient process state that has no injectable seam. Without a seam the
///     surrounding code cannot be driven deterministically and its failure paths cannot be
///     exercised by a test.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AmbientDependencyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule;

    static AmbientDependencyAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.Testing.AmbientDependency,
            "Ambient dependency must be reached through an injectable seam",
            "'{0}' reads ambient state directly, so this code cannot be driven deterministically or fault-injected by a test. Instead, {1}.",
            "Testing",
            DiagnosticSeverity.Warning,
            true,
            "Code that reaches straight for the clock, the file system, the network, or the random source has no seam a test can substitute, so its error-handling paths stay unexercised. Route the dependency through an injected abstraction that a test can replace with one that fails on demand, the way a pluggable virtual file system and allocator let a database engine test every I/O and out-of-memory path.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
        context.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        var symbol = context.SemanticModel.GetSymbolInfo(memberAccess).Symbol;
        if (symbol is null)
        {
            return;
        }

        var seam = DescribeSeam(symbol);
        if (seam is null)
        {
            return;
        }

        var name = $"{symbol.ContainingType.Name}.{symbol.Name}";
        context.ReportDiagnostic(Diagnostic.Create(Rule, memberAccess.GetLocation(), name, seam));
    }

    private void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ObjectCreationExpressionSyntax creation)
        {
            return;
        }

        if (context.SemanticModel.GetSymbolInfo(creation).Symbol is not IMethodSymbol constructor)
        {
            return;
        }

        var seam = DescribeConstructedSeam(constructor);
        if (seam is null)
        {
            return;
        }

        var name = $"new {constructor.ContainingType.Name}()";
        context.ReportDiagnostic(Diagnostic.Create(Rule, creation.GetLocation(), name, seam));
    }

    private string? DescribeConstructedSeam(IMethodSymbol constructor)
    {
        var typeName = constructor.ContainingType.ToDisplayString();
        if (typeName == "System.Random" && constructor.Parameters.Length == 0)
        {
            return "inject the instance, or pass an explicit seed so runs are reproducible";
        }

        if (typeName == "System.Net.Http.HttpClient")
        {
            return "inject the client, or resolve it from IHttpClientFactory";
        }

        return null;
    }

    private string? DescribeSeam(ISymbol symbol)
    {
        if (symbol.ContainingType is null)
        {
            return null;
        }

        switch (symbol.ContainingType.ToDisplayString())
        {
            case "System.DateTime":
            case "System.DateTimeOffset":
                return HasClockMemberName(symbol.Name) ? "inject System.TimeProvider" : null;
            case "System.Guid":
                return symbol.Name == "NewGuid" ? "inject a factory so identifiers are reproducible" : null;
            case "System.Threading.Thread":
                return symbol.Name == "Sleep" ? "await a synchronisation primitive, or inject System.TimeProvider" : null;
            case "System.IO.File":
            case "System.IO.Directory":
                return "inject a file-system abstraction so I/O failures can be simulated";
            case "System.Environment":
                return symbol.Name == "GetEnvironmentVariable" ? "inject configuration instead of reading process state" : null;
            default:
                return null;
        }
    }

    private bool HasClockMemberName(string name)
    {
        return name == "Now" || name == "UtcNow" || name == "Today";
    }
}
