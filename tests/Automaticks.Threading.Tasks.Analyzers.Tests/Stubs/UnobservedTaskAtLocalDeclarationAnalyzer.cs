using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.Threading.Tasks.Analyzers.Tests.Stubs;

/// <summary>Reports ATXTA010 on any invocation, including one with no enclosing expression statement.</summary>
public sealed class UnobservedTaskAtLocalDeclarationAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule;

    static UnobservedTaskAtLocalDeclarationAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.ThreadingTasks.UnobservedTask,
            "Test-only ATXTA010 on any invocation",
            "Test-only ATXTA010 on any invocation",
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
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        var invocation = (context.Node as InvocationExpressionSyntax)!;
        context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation()));
    }
}
