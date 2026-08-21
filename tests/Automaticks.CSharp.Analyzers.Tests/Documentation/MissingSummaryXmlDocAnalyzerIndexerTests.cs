using Automaticks.CSharp.Documentation;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Documentation;

/// <summary>
///     Tests MissingSummaryXmlDocAnalyzer against members that carry no identifier.
/// </summary>
public class MissingSummaryXmlDocAnalyzerIndexerTests
{
    /// <summary>
    ///     Tests that a documented indexer is left alone.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_DocumentedIndexer_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///     A shape.
                                  /// </summary>
                                  public class Shape {
                                      /// <summary>
                                      ///     A side.
                                      /// </summary>
                                      public int this[int index] { get { return index; } }
                                  }
                              }
                              """;

        var analyzer = new MissingSummaryXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.MissingSummaryXmlDoc)).IsFalse();
    }

    /// <summary>
    ///     Tests that an indexer is reported even though it has no name to quote.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_UndocumentedIndexer_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///     A shape.
                                  /// </summary>
                                  public class Shape {
                                      public int this[int index] { get { return index; } }
                                  }
                              }
                              """;

        var analyzer = new MissingSummaryXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.MissingSummaryXmlDoc)).IsTrue();
    }
}
