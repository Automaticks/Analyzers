using Automaticks.Testing.Analyzers.Tests.Stubs;
using Automaticks.Testing.Coverage;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Testing.Analyzers.Tests.Coverage;

/// <summary>
///     Tests for FileBranchCoverageAnalyzer.
/// </summary>
public class FileBranchCoverageAnalyzerTests
{
    private const string Source = """
                                  namespace MyApp {
                                      public class Foo {
                                          public int Branchy(int value) { return value > 0 ? 1 : 2; }
                                      }
                                  }
                                  """;

    /// <summary>
    ///     Tests that a lowered threshold accepts coverage the default would reject.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConfiguredThresholdLowered_SuppressesDefaultViolation(CancellationToken cancellationToken)
    {
        const string report = """
                              <coverage version="1.9"><packages><package name="MyApp"><classes>
                                <class name="MyApp.Foo" filename="MyApp/Foo.cs"><lines>
                                  <line number="3" hits="1" condition-coverage="50% (1/2)" />
                                </lines></class>
                              </classes></package></packages></coverage>
                              """;

        var diagnostics = await AnalyzeWithThresholdAsync(report, "40", cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST017")).IsFalse();
    }

    /// <summary>
    ///     Tests that a threshold outside the percentage range falls back to the default.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConfiguredThresholdOutOfRange_UsesDefault(CancellationToken cancellationToken)
    {
        const string report = """
                              <coverage version="1.9"><packages><package name="MyApp"><classes>
                                <class name="MyApp.Foo" filename="MyApp/Foo.cs"><lines>
                                  <line number="3" hits="1" condition-coverage="50% (1/2)" />
                                </lines></class>
                              </classes></package></packages></coverage>
                              """;

        var diagnostics = await AnalyzeWithThresholdAsync(report, "150", cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST017")).IsTrue();
    }

    /// <summary>
    ///     Tests that a threshold that is not a number falls back to the default.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConfiguredThresholdUnparsable_UsesDefault(CancellationToken cancellationToken)
    {
        const string report = """
                              <coverage version="1.9"><packages><package name="MyApp"><classes>
                                <class name="MyApp.Foo" filename="MyApp/Foo.cs"><lines>
                                  <line number="3" hits="1" condition-coverage="50% (1/2)" />
                                </lines></class>
                              </classes></package></packages></coverage>
                              """;

        var diagnostics = await AnalyzeWithThresholdAsync(report, "most of them", cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST017")).IsTrue();
    }

    /// <summary>
    ///     Tests that coverage at or above the minimum is accepted.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CoverageAboveMinimum_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string report = """
                              <coverage version="1.9"><packages><package name="MyApp"><classes>
                                <class name="MyApp.Foo" filename="MyApp/Foo.cs"><lines>
                                  <line number="3" hits="1" condition-coverage="100% (2/2)" />
                                </lines></class>
                              </classes></package></packages></coverage>
                              """;

        var diagnostics = await AnalyzeWithReportAsync(report, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST017")).IsFalse();
    }

    /// <summary>
    ///     Tests that coverage below the minimum is reported.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CoverageBelowMinimum_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string report = """
                              <coverage version="1.9"><packages><package name="MyApp"><classes>
                                <class name="MyApp.Foo" filename="MyApp/Foo.cs"><lines>
                                  <line number="3" hits="1" condition-coverage="50% (1/2)" />
                                </lines></class>
                              </classes></package></packages></coverage>
                              """;

        var diagnostics = await AnalyzeWithReportAsync(report, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST017")).IsTrue();
    }

    /// <summary>
    ///     Tests that a file absent from the report is left alone.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FileMissingFromReport_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string report = """
                              <coverage version="1.9"><packages><package name="MyApp"><classes>
                                <class name="MyApp.Other" filename="MyApp/Other.cs"><lines>
                                  <line number="3" hits="1" condition-coverage="50% (1/2)" />
                                </lines></class>
                              </classes></package></packages></coverage>
                              """;

        var diagnostics = await AnalyzeWithReportAsync(report, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST017")).IsFalse();
    }

    /// <summary>
    ///     Tests that a file reporting no branches at all is left alone.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FileWithoutBranches_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string report = """
                              <coverage version="1.9"><packages><package name="MyApp"><classes>
                                <class name="MyApp.Foo" filename="MyApp/Foo.cs"><lines>
                                  <line number="3" hits="1" />
                                </lines></class>
                              </classes></package></packages></coverage>
                              """;

        var diagnostics = await AnalyzeWithReportAsync(report, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST017")).IsFalse();
    }

    /// <summary>
    ///     Tests that the rule stays silent when no report is supplied.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NoReportSupplied_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        var analyzer = new FileBranchCoverageAnalyzer();
        var options = new AnalysisOptions
        {
            FilePath = "C:/repo/MyApp/Foo.cs",
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, Source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST017")).IsFalse();
    }

    /// <summary>
    ///     Tests that the same file appearing in two reports is merged rather than double counted.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SameFileInTwoReports_MergesWithoutDoubleCounting(CancellationToken cancellationToken)
    {
        const string report = """
                              <coverage version="1.9"><packages><package name="MyApp"><classes>
                                <class name="MyApp.Foo" filename="MyApp/Foo.cs"><lines>
                                  <line number="3" hits="1" condition-coverage="100% (2/2)" />
                                </lines></class>
                              </classes></package></packages></coverage>
                              """;

        var analyzer = new FileBranchCoverageAnalyzer();
        var firstText = new TestAdditionalText("C:/repo/artifacts/coverage0.cobertura.xml", report);
        var secondText = new TestAdditionalText("C:/repo/artifacts/coverage1.cobertura.xml", report);
        var additionalFiles = new List<AdditionalText>
        {
            firstText,
            secondText,
        };
        var options = new AnalysisOptions
        {
            FilePath = "C:/repo/MyApp/Foo.cs",
            AdditionalFiles = additionalFiles,
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, Source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST017")).IsFalse();
    }

    private async Task<ImmutableArray<Diagnostic>> AnalyzeWithReportAsync(
        string reportXml,
        CancellationToken cancellationToken)
    {
        var analyzer = new FileBranchCoverageAnalyzer();
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

    private async Task<ImmutableArray<Diagnostic>> AnalyzeWithThresholdAsync(
        string reportXml,
        string minimumBranchCoverage,
        CancellationToken cancellationToken)
    {
        var analyzer = new FileBranchCoverageAnalyzer();
        var additionalText = new TestAdditionalText("C:/repo/artifacts/coverage.cobertura.xml", reportXml);
        var additionalFiles = new List<AdditionalText>
        {
            additionalText,
        };
        var configOptions = new Dictionary<string, string>
        {
            ["automaticks.minimum_branch_coverage"] = minimumBranchCoverage,
        };
        var options = new AnalysisOptions
        {
            FilePath = "C:/repo/MyApp/Foo.cs",
            AdditionalFiles = additionalFiles,
            ConfigOptions = configOptions,
        };
        return await AnalyzerTestRunner.AnalyzeAsync(analyzer, Source, options, cancellationToken);
    }
}
