using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.EntityFrameworkCore.Analyzers.Tests.Stubs;

/// <summary>Reports a Warning-severity <c>ATXLQ002</c> on every using directive so suppressors can be exercised.</summary>
public sealed class SuppressibleLinqUsageAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule;

    static SuppressibleLinqUsageAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            "ATXLQ002",
            "LINQ is not allowed (test stub)",
            "LINQ is not allowed in this codebase.",
            "Linq",
            DiagnosticSeverity.Warning,
            true);
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeUsing, SyntaxKind.UsingDirective);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeUsing(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not UsingDirectiveSyntax usingDirective)
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, usingDirective.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }
}
