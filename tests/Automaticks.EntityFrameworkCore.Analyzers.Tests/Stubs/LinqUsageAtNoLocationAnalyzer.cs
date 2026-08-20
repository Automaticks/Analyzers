using Automaticks.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.EntityFrameworkCore.Analyzers.Tests.Stubs;

/// <summary>Reports ATXLQ002 at Location.None so the suppressor's null-tree branch can be tested.</summary>
public sealed class LinqUsageAtNoLocationAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule;

    static LinqUsageAtNoLocationAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.Linq.LinqUsage,
            "Test-only ATXLQ002 with no location",
            "Test-only ATXLQ002 with no location",
            "Testing",
            DiagnosticSeverity.Warning,
            true);
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationAction(Analyze);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void Analyze(CompilationAnalysisContext context)
    {
        context.ReportDiagnostic(Diagnostic.Create(Rule, Location.None));
    }
}
