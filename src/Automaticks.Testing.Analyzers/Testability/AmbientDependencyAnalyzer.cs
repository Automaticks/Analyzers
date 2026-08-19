using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Automaticks.Testing.Testability;

/// <summary>
///     Flags direct use of ambient process state that has no injectable seam.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AmbientDependencyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule;
    private readonly ImmutableHashSet<string> AmbientTypeNames;

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

    /// <summary>
    ///     Initializes the lookup tables used during analysis.
    /// </summary>
    public AmbientDependencyAnalyzer()
    {
        var ambientTypeNames = ImmutableHashSet.CreateBuilder<string>(StringComparer.Ordinal);
        ambientTypeNames.Add("DateTime");
        ambientTypeNames.Add("DateTimeOffset");
        ambientTypeNames.Add("Directory");
        ambientTypeNames.Add("Environment");
        ambientTypeNames.Add("File");
        ambientTypeNames.Add("Guid");
        ambientTypeNames.Add("Thread");
        AmbientTypeNames = ambientTypeNames.ToImmutable();
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(RegisterPerCompilation);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context, ConcurrentDictionary<SyntaxTree, bool> aliasTrees)
    {
        var memberAccess = (context.Node as MemberAccessExpressionSyntax)!;

        if (!AmbientTypeNames.Contains(GetRightmostName(memberAccess.Expression))
            && !HasUsingAlias(context.Node.SyntaxTree, aliasTrees))
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
        var creation = (context.Node as ObjectCreationExpressionSyntax)!;

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

        if (!AmbientTypeNames.Contains(symbol.ContainingType.Name))
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

    private string GetRightmostName(ExpressionSyntax expression)
    {
        if (expression is MemberAccessExpressionSyntax memberAccess)
        {
            return memberAccess.Name.Identifier.ValueText;
        }

        if (expression is IdentifierNameSyntax identifier)
        {
            return identifier.Identifier.ValueText;
        }

        return string.Empty;
    }

    private bool HasAliasInTree(SyntaxTree tree)
    {
        var root = tree.GetRoot();

        foreach (var node in root.DescendantNodes(candidate => candidate is CompilationUnitSyntax or BaseNamespaceDeclarationSyntax))
        {
            if (node is UsingDirectiveSyntax { Alias: not null })
            {
                return true;
            }
        }

        return false;
    }

    private bool HasClockMemberName(string name)
    {
        return name == "Now" || name == "UtcNow" || name == "Today";
    }

    private bool HasUsingAlias(SyntaxTree tree, ConcurrentDictionary<SyntaxTree, bool> aliasTrees)
    {
        if (aliasTrees.TryGetValue(tree, out var hasAlias))
        {
            return hasAlias;
        }

        hasAlias = HasAliasInTree(tree);
        aliasTrees[tree] = hasAlias;
        return hasAlias;
    }

    private void RegisterPerCompilation(CompilationStartAnalysisContext compilationContext)
    {
        var aliasTrees = new ConcurrentDictionary<SyntaxTree, bool>();
        compilationContext.RegisterSyntaxNodeAction(
            context => AnalyzeMemberAccess(context, aliasTrees),
            SyntaxKind.SimpleMemberAccessExpression);
        compilationContext.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression);
    }
}
