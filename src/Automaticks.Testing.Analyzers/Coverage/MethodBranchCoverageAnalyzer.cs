using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.Testing.Coverage;

/// <summary>
///     Reports methods whose branch coverage in the supplied report falls below the configured minimum.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MethodBranchCoverageAnalyzer : DiagnosticAnalyzer
{
    private const int DefaultMinimumPercentage = 80;
    private const string MinimumPercentageKey = "automaticks.minimum_branch_coverage";
    private static readonly DiagnosticDescriptor Rule;

    static MethodBranchCoverageAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.Testing.MethodBranchCoverage,
            "Method branch coverage must meet the configured minimum",
            "Method '{0}' has {1}% branch coverage, which is below the required minimum of {2}%. Add tests that exercise the missing branches.",
            "Testing",
            DiagnosticSeverity.Warning,
            true,
            "The coverage report shows this method takes fewer of its branches than the configured minimum. Set the threshold with `automaticks.minimum_branch_coverage` in `.editorconfig`. The rule stays silent when no coverage report is supplied, so a clean clone still builds before any test run has happened.");
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

    private void AnalyzeSymbol(SymbolAnalysisContext context, CoverageReport report)
    {
        var method = (context.Symbol as IMethodSymbol)!;
        if (!HasReportableMethod(method))
        {
            return;
        }

        foreach (var reference in method.DeclaringSyntaxReferences)
        {
            var file = report.FindFile(reference.SyntaxTree.FilePath);
            var recorded = file?.FindMethod(method.Name);
            if (recorded is null || recorded.TotalBranches == 0)
            {
                continue;
            }

            var percentage = recorded.CoveredBranches * 100 / recorded.TotalBranches;
            var minimum = ReadMinimumPercentage(context, reference.SyntaxTree);
            if (percentage >= minimum)
            {
                continue;
            }

            var location = Location.Create(reference.SyntaxTree, reference.Span);
            context.ReportDiagnostic(Diagnostic.Create(Rule, location, method.Name, percentage, minimum));
        }
    }

    private bool HasReportableMethod(IMethodSymbol method)
    {
        if (method.IsImplicitlyDeclared || method.IsAbstract || method.IsExtern)
        {
            return false;
        }

        return method.MethodKind == MethodKind.Ordinary && method.DeclaringSyntaxReferences.Length > 0;
    }

    private int ReadMinimumPercentage(SymbolAnalysisContext context, SyntaxTree tree)
    {
        var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(tree);
        if (options.TryGetValue(MinimumPercentageKey, out var raw)
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

        context.RegisterSymbolAction(symbolContext => AnalyzeSymbol(symbolContext, report), SymbolKind.Method);
    }
}
