using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.Testing.Coverage;

/// <summary>
///     Reports a coverage report that was supplied but yields no usable entries.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UnusableCoverageReportAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule;

    static UnusableCoverageReportAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.Testing.UnusableCoverageReport,
            "Supplied coverage report must be usable",
            "The coverage report '{0}' was supplied but is empty, not valid XML, or contains no file entries, so the coverage gate is silently checking nothing. Regenerate the report, or remove it from AdditionalFiles.",
            "Testing",
            DiagnosticSeverity.Warning,
            true,
            "A coverage-gated build must fail loudly when the report it was told to check is unusable, because otherwise every coverage rule silently passes as if the codebase were fully covered. This differs from the no-report case, which is a legitimate clean clone that has simply not run tests yet, and where the coverage rules stay silent instead.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationAction(AnalyzeCompilation);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [Rule];
        }
    }

    private void AnalyzeCompilation(CompilationAnalysisContext context)
    {
        var unusablePaths = CoverageReportLocator.FindUnusableReportPaths(context.Options, context.CancellationToken);
        foreach (var path in unusablePaths)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, Location.None, path));
        }
    }
}
