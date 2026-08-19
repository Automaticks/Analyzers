using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace Automaticks.Testing.Coverage;

/// <summary>
///     Reports source files whose line coverage in the supplied report falls below the configured minimum, read from .editorconfig key automaticks.minimum...
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class FileLineCoverageAnalyzer : DiagnosticAnalyzer
{
    private const int DefaultMinimumPercentage = 80;
    private const string MinimumPercentageKey = "automaticks.minimum_line_coverage";
    private static readonly DiagnosticDescriptor Rule;

    static FileLineCoverageAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.Testing.FileLineCoverage,
            "File line coverage must meet the configured minimum",
            "File '{0}' has {1}% line coverage, which is below the required minimum of {2}%. Add tests for the unexecuted lines.",
            "Testing",
            DiagnosticSeverity.Warning,
            true,
            "The coverage report shows this file has fewer executed lines than the configured minimum. Set the threshold with `automaticks.minimum_line_coverage` in `.editorconfig`. The rule stays silent when no coverage report is supplied, so a clean clone still builds before any test run has happened.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(RegisterCompilationStart);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context, CoverageReport report)
    {
        var file = report.FindFile(context.Tree.FilePath);
        if (file is null || file.LinePercentage < 0)
        {
            return;
        }

        var minimum = ReadMinimumPercentage(context);
        if (file.LinePercentage >= minimum)
        {
            return;
        }

        var span = new TextSpan(0, 0);
        var location = Location.Create(context.Tree, span);
        context.ReportDiagnostic(Diagnostic.Create(
            Rule,
            location,
            file.ReportedPath,
            file.LinePercentage,
            minimum));
    }

    private int ReadMinimumPercentage(SyntaxTreeAnalysisContext context)
    {
        var treeOptions = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Tree);
        if (treeOptions.TryGetValue(MinimumPercentageKey, out var raw)
            && int.TryParse(raw, out var parsed)
            && parsed >= 0
            && parsed <= 100)
        {
            return parsed;
        }

        return DefaultMinimumPercentage;
    }

    private void RegisterCompilationStart(CompilationStartAnalysisContext context)
    {
        var report = CoverageReportLocator.Find(context.Options, context.CancellationToken);
        if (report is null)
        {
            return;
        }

        context.RegisterSyntaxTreeAction(treeContext => AnalyzeSyntaxTree(treeContext, report));
    }
}
