using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace Automaticks.Testing.Coverage;

/// <summary>
///     Reports source files whose branch coverage in the supplied report falls below the configured minimum.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class FileBranchCoverageAnalyzer : DiagnosticAnalyzer
{
    private const int DefaultMinimumPercentage = 80;
    private const string MinimumPercentageKey = "automaticks.minimum_branch_coverage";
    private static readonly DiagnosticDescriptor Rule;

    static FileBranchCoverageAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.Testing.FileBranchCoverage,
            "File branch coverage must meet the configured minimum",
            "File '{0}' has {1}% branch coverage, which is below the required minimum of {2}%. Add tests that exercise the missing branches.",
            "Testing",
            DiagnosticSeverity.Warning,
            true,
            "The coverage report shows this file has fewer taken branches than the configured minimum. Set the threshold with `automaticks.minimum_branch_coverage` in `.editorconfig`. This counts every branch in the file, including those the compiler moves into generated state machines for `async` and iterator methods, which a per-method rule cannot attribute. The rule stays silent when no coverage report is supplied.");
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
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [Rule];
        }
    }

    private void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context, CoverageReport report)
    {
        var file = report.FindFile(context.Tree.FilePath);
        if (file is null || file.BranchPercentage < 0)
        {
            return;
        }

        var minimum = ReadMinimumPercentage(context);
        if (file.BranchPercentage >= minimum)
        {
            return;
        }

        var span = new TextSpan(0, 0);
        var location = Location.Create(context.Tree, span);
        context.ReportDiagnostic(Diagnostic.Create(
            Rule,
            location,
            file.ReportedPath,
            file.BranchPercentage,
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
