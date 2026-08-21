using Automaticks.CSharp.Formatting;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Formatting;

/// <summary>
///     Tests DuplicateUsingDirectiveAnalyzer against aliases that name no namespace.
/// </summary>
public class DuplicateUsingDirectiveAnalyzerTypeAliasTests
{
    /// <summary>
    ///     Tests that the same array alias written twice is reported.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_RepeatedArrayAlias_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using Numbers = int[];
                              using Numbers = int[];
                              namespace MyApp { public class Shape { } }
                              """;

        var analyzer = new DuplicateUsingDirectiveAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.DuplicateUsingDirective)).IsTrue();
    }

    /// <summary>
    ///     Tests that two array aliases with different names are left alone.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SeparateArrayAliases_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using Numbers = int[];
                              using Sizes = long[];
                              namespace MyApp { public class Shape { } }
                              """;

        var analyzer = new DuplicateUsingDirectiveAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.DuplicateUsingDirective)).IsFalse();
    }
}
