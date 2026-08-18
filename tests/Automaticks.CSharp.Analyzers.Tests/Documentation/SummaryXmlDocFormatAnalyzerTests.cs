using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Documentation;

/// <summary>
///     Tests for SummaryXmlDocFormatAnalyzer.
/// </summary>
public class SummaryXmlDocFormatAnalyzerTests
{

    /// <summary>
    ///     Tests that Analyze_ContentOnNewLineWithFourSpaceIndentation_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ContentOnNewLineWithFourSpaceIndentation_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///     The Android entry point for the Inferno client.
                                  /// </summary>
                                  public class Foo {}
                              }
                              """;

        var analyzer = new SummaryXmlDocFormatAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS050")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ContentOnNewLineWithNoIndentation_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ContentOnNewLineWithNoIndentation_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  /// No indentation here.
                                  /// </summary>
                                  public class Foo {}
                              }
                              """;

        var analyzer = new SummaryXmlDocFormatAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS050")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ContentOnNewLineWithThreeSpaceIndentation_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ContentOnNewLineWithThreeSpaceIndentation_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///    Three spaces only.
                                  /// </summary>
                                  public class Foo {}
                              }
                              """;

        var analyzer = new SummaryXmlDocFormatAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS050")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ContentOnNewLineWithTwoSpaceIndentation_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ContentOnNewLineWithTwoSpaceIndentation_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///   Two spaces only.
                                  /// </summary>
                                  public class Foo {}
                              }
                              """;

        var analyzer = new SummaryXmlDocFormatAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS050")).IsTrue();
    }
    /// <summary>
    ///     Tests that Analyze_InlineSummaryOnClass_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_InlineSummaryOnClass_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>The Android entry point for the Inferno client.</summary>
                                  public class Foo {}
                              }
                              """;

        var analyzer = new SummaryXmlDocFormatAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS050")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_InlineSummaryOnMethod_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_InlineSummaryOnMethod_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      /// <summary>Does something.</summary>
                                      public void Bar() {}
                                  }
                              }
                              """;

        var analyzer = new SummaryXmlDocFormatAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS050")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_InlineSummaryWithSeeRef_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_InlineSummaryWithSeeRef_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>Works with <see cref="System.String" /> values.</summary>
                                  public class Foo {}
                              }
                              """;

        var analyzer = new SummaryXmlDocFormatAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS050")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_MultiLineContentAllLinesProperlyIndented_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MultiLineContentAllLinesProperlyIndented_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///     First line of the summary.
                                  ///     Second line of the summary.
                                  /// </summary>
                                  public class Foo {}
                              }
                              """;

        var analyzer = new SummaryXmlDocFormatAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS050")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_MultiLineContentOneLineMissingIndentation_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MultiLineContentOneLineMissingIndentation_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///     First line properly indented.
                                  /// Missing indentation on this line.
                                  /// </summary>
                                  public class Foo {}
                              }
                              """;

        var analyzer = new SummaryXmlDocFormatAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS050")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_NoDocComment_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NoDocComment_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {}
                              }
                              """;

        var analyzer = new SummaryXmlDocFormatAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS050")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ProperlyFormattedSummaryOnMethod_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ProperlyFormattedSummaryOnMethod_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      /// <summary>
                                      ///     Does something useful.
                                      /// </summary>
                                      public void Bar() {}
                                  }
                              }
                              """;

        var analyzer = new SummaryXmlDocFormatAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS050")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_SummaryWithSeeRefProperlyFormatted_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SummaryWithSeeRefProperlyFormatted_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///     Works with <see cref="System.String" /> values.
                                  /// </summary>
                                  public class Foo {}
                              }
                              """;

        var analyzer = new SummaryXmlDocFormatAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS050")).IsFalse();
    }
}
