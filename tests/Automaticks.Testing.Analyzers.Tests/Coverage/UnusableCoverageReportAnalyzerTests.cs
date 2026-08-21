using Automaticks.Testing.Analyzers.Tests.Stubs;
using Automaticks.Testing.Coverage;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Testing.Analyzers.Tests.Coverage;

/// <summary>
///     Tests for UnusableCoverageReportAnalyzer.
/// </summary>
public class UnusableCoverageReportAnalyzerTests
{
    private const string Source = """
                                  namespace MyApp {
                                      public class Foo {
                                          public int Bar() { return 1; }
                                      }
                                  }
                                  """;

    /// <summary>
    ///     Tests that Analyze_EmptyReport_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_EmptyReport_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        var diagnostics = await AnalyzeWithReportAsync(string.Empty, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(
            diagnostics,
            "ATXTST016",
            "coverage.cobertura.xml")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_MalformedXml_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MalformedXml_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string report = "<coverage version=\"1.9\"><packages><package name=\"MyApp\">";

        var diagnostics = await AnalyzeWithReportAsync(report, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST016")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ReportWithZeroFileEntries_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ReportWithZeroFileEntries_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string report = """
                              <?xml version="1.0" encoding="utf-8"?>
                              <coverage version="1.9"><packages></packages></coverage>
                              """;

        var diagnostics = await AnalyzeWithReportAsync(report, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST016")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_UsableReport_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_UsableReport_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string report = """
                              <coverage version="1.9"><packages><package name="MyApp"><classes>
                                <class name="MyApp.Foo" filename="MyApp/Foo.cs"><lines>
                                  <line number="3" hits="1" />
                                </lines></class>
                              </classes></package></packages></coverage>
                              """;

        var diagnostics = await AnalyzeWithReportAsync(report, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST016")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_WithoutReport_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_WithoutReport_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        var analyzer = new UnusableCoverageReportAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, Source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST016")).IsFalse();
    }

    private async Task<ImmutableArray<Diagnostic>> AnalyzeWithReportAsync(
        string reportXml,
        CancellationToken cancellationToken)
    {
        var analyzer = new UnusableCoverageReportAnalyzer();
        var additionalText = new TestAdditionalText("C:/repo/artifacts/coverage.cobertura.xml", reportXml);
        var additionalFiles = new List<AdditionalText>
        {
            additionalText,
        };
        var options = new AnalysisOptions
        {
            AdditionalFiles = additionalFiles,
        };
        return await AnalyzerTestRunner.AnalyzeAsync(analyzer, Source, options, cancellationToken);
    }
}
