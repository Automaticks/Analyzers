using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.Testing.Coverage;

/// <summary>
///     Reports public members that the supplied coverage report shows were never executed.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UncoveredPublicMemberAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule;

    static UncoveredPublicMemberAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.Testing.UncoveredPublicMember,
            "Public member must be covered by a test",
            "Public member '{0}' was never executed in the supplied coverage report. Add a test that exercises it, or remove it if it is unreachable.",
            "Testing",
            DiagnosticSeverity.Warning,
            true,
            "The coverage report records no executed line for this member, so nothing verifies that it behaves as documented. Because the report is produced by a previous test run, this rule stays silent when no report is supplied and skips members the report does not mention at all, reporting only members it positively shows as unexecuted.");
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
            var path = reference.SyntaxTree.FilePath;
            var file = report.FindFile(path);
            var recorded = file?.FindMethod(method.Name);
            if (recorded is null || recorded.IsCovered)
            {
                continue;
            }

            var location = Location.Create(reference.SyntaxTree, reference.Span);
            context.ReportDiagnostic(Diagnostic.Create(Rule, location, method.Name));
        }
    }

    private bool HasReportableMethod(IMethodSymbol method)
    {
        if (method.DeclaredAccessibility != Accessibility.Public)
        {
            return false;
        }

        if (method.IsImplicitlyDeclared || method.IsAbstract || method.IsExtern)
        {
            return false;
        }

        return method.MethodKind == MethodKind.Ordinary && method.DeclaringSyntaxReferences.Length > 0;
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
