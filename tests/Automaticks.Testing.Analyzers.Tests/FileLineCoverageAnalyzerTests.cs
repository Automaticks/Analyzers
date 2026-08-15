using Automaticks.Testing.Analyzers.Tests.Stubs;
using Automaticks.Testing.Coverage;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Testing.Analyzers.Tests;

/// <summary>
///     Tests for FileLineCoverageAnalyzer.
/// </summary>
public class FileLineCoverageAnalyzerTests
{
    private const string Source = """
                                  namespace MyApp {
                                      public class Foo {
                                          public int Covered() { return 1; }
                                          public int Uncovered() { return 2; }
                                      }
                                  }
                                  """;

    /// <summary>
    ///     Tests that Analyze_CoverageAboveMinimum_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CoverageAboveMinimum_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string report = """
                              <coverage version="1.9"><packages><package name="MyApp"><classes>
                                <class name="MyApp.Foo" filename="MyApp/Foo.cs"><lines>
                                  <line number="3" hits="1" /><line number="4" hits="1" />
                                  <line number="5" hits="1" /><line number="6" hits="1" />
                                  <line number="7" hits="1" /><line number="8" hits="0" />
                                </lines></class>
                              </classes></package></packages></coverage>
                              """;

        var diagnostics = await AnalyzeWithReportAsync(report, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST013")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_CoverageBelowMinimum_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CoverageBelowMinimum_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string report = """
                              <coverage version="1.9"><packages><package name="MyApp"><classes>
                                <class name="MyApp.Foo" filename="MyApp/Foo.cs"><lines>
                                  <line number="3" hits="1" /><line number="4" hits="0" />
                                  <line number="5" hits="0" /><line number="6" hits="0" />
                                </lines></class>
                              </classes></package></packages></coverage>
                              """;

        var diagnostics = await AnalyzeWithReportAsync(report, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(
            diagnostics,
            "ATXTST013",
            "25% line coverage")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_WithoutReport_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_WithoutReport_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        var analyzer = new FileLineCoverageAnalyzer();
        var options = new AnalysisOptions
        {
            FilePath = "C:/repo/MyApp/Foo.cs",
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, Source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST013")).IsFalse();
    }

    private async Task<ImmutableArray<Diagnostic>> AnalyzeWithReportAsync(
        string reportXml,
        CancellationToken cancellationToken)
    {
        var analyzer = new FileLineCoverageAnalyzer();
        var additionalText = new TestAdditionalText("C:/repo/artifacts/coverage.cobertura.xml", reportXml);
        var additionalFiles = new List<AdditionalText>
        {
            additionalText,
        };
        var options = new AnalysisOptions
        {
            FilePath = "C:/repo/MyApp/Foo.cs",
            AdditionalFiles = additionalFiles,
        };
        return await AnalyzerTestRunner.AnalyzeAsync(analyzer, Source, options, cancellationToken);
    }
}
