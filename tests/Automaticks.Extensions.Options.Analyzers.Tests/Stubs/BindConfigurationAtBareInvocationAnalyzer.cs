using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.Extensions.Options.Analyzers.Tests.Stubs;

/// <summary>Reports ATXEO049 on a bare invocation with no member access, to test the code fix's defensive shape guard.</summary>
public sealed class BindConfigurationAtBareInvocationAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule;

    static BindConfigurationAtBareInvocationAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.Options.BindConfiguration,
            "Test-only ATXEO049 on a bare invocation",
            "Test-only ATXEO049 on a bare invocation",
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
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.InvocationExpression);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [Rule];
        }
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        var invocation = (context.Node as InvocationExpressionSyntax)!;
        if (invocation.Expression is IdentifierNameSyntax)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation()));
        }
    }
}
