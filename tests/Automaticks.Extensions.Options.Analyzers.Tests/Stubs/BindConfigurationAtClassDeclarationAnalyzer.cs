using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.Extensions.Options.Analyzers.Tests.Stubs;

/// <summary>Reports ATXEO049 on a class identifier, to test the code fix's "no enclosing invocation" guard.</summary>
public sealed class BindConfigurationAtClassDeclarationAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule;

    static BindConfigurationAtClassDeclarationAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.Options.BindConfiguration,
            "Test-only ATXEO049 on a class identifier",
            "Test-only ATXEO049 on a class identifier",
            "Testing",
            DiagnosticSeverity.Error,
            true);
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.ClassDeclaration);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        var classDeclaration = (context.Node as ClassDeclarationSyntax)!;
        context.ReportDiagnostic(Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation()));
    }
}
