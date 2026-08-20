using Automaticks.Testing.Analyzers.Tests.Stubs;
using Automaticks.Testing.Coverage;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Testing.Analyzers.Tests.Coverage;

/// <summary>
///     Tests for MethodBranchCoverageAnalyzer.
/// </summary>
public class MethodBranchCoverageAnalyzerTests
{
    private const string Source = """
                                  namespace MyApp {
                                      public class Foo {
                                          public int Bar(bool flag) { if (flag) { return 1; } return 0; }
                                      }
                                  }
                                  """;

    /// <summary>
    ///     Tests that Analyze_BranchCoverageAboveMinimum_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BranchCoverageAboveMinimum_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string report = """
                              <coverage version="1.9"><packages><package name="MyApp"><classes>
                                <class name="MyApp.Foo" filename="MyApp/Foo.cs"><methods>
                                  <method name="Bar" signature="(System.Boolean)"><lines>
                                    <line number="3" hits="1" branch="true" condition-coverage="100% (4/4)" />
                                  </lines></method>
                                </methods></class>
                              </classes></package></packages></coverage>
                              """;

        var diagnostics = await AnalyzeWithReportAsync(report, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST015")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_BranchCoverageBelowMinimum_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BranchCoverageBelowMinimum_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string report = """
                              <coverage version="1.9"><packages><package name="MyApp"><classes>
                                <class name="MyApp.Foo" filename="MyApp/Foo.cs"><methods>
                                  <method name="Bar" signature="(System.Boolean)"><lines>
                                    <line number="3" hits="1" branch="true" condition-coverage="25% (1/4)" />
                                  </lines></method>
                                </methods></class>
                              </classes></package></packages></coverage>
                              """;

        var diagnostics = await AnalyzeWithReportAsync(report, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(
            diagnostics,
            "ATXTST015",
            "'Bar' has 25% branch coverage")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ConfiguredThresholdLowered_SuppressesDefaultViolation.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConfiguredThresholdLowered_SuppressesDefaultViolation(CancellationToken cancellationToken)
    {
        const string report = """
                              <coverage version="1.9"><packages><package name="MyApp"><classes>
                                <class name="MyApp.Foo" filename="MyApp/Foo.cs"><methods>
                                  <method name="Bar" signature="(System.Boolean)"><lines>
                                    <line number="3" hits="1" branch="true" condition-coverage="75% (3/4)" />
                                  </lines></method>
                                </methods></class>
                              </classes></package></packages></coverage>
                              """;

        var diagnostics = await AnalyzeWithConfiguredThresholdAsync(report, "50", cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST015")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_MethodMissingFromReport_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodMissingFromReport_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string report = """
                              <coverage version="1.9"><packages><package name="MyApp"><classes>
                                <class name="MyApp.Foo" filename="MyApp/Foo.cs"><methods>
                                  <method name="NotInSource" signature="()"><lines>
                                    <line number="3" hits="1" branch="true" condition-coverage="25% (1/4)" />
                                  </lines></method>
                                </methods></class>
                              </classes></package></packages></coverage>
                              """;

        var diagnostics = await AnalyzeWithReportAsync(report, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST015")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_WithoutReport_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_WithoutReport_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        var analyzer = new MethodBranchCoverageAnalyzer();
        var options = new AnalysisOptions
        {
            FilePath = "C:/repo/MyApp/Foo.cs",
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, Source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST015")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ZeroTotalBranches_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ZeroTotalBranches_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string report = """
                              <coverage version="1.9"><packages><package name="MyApp"><classes>
                                <class name="MyApp.Foo" filename="MyApp/Foo.cs"><methods>
                                  <method name="Bar" signature="(System.Boolean)"><lines>
                                    <line number="3" hits="0" />
                                  </lines></method>
                                </methods></class>
                              </classes></package></packages></coverage>
                              """;

        var diagnostics = await AnalyzeWithReportAsync(report, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST015")).IsFalse();
    }

    private async Task<ImmutableArray<Diagnostic>> AnalyzeWithConfiguredThresholdAsync(
        string reportXml,
        string minimumBranchCoverage,
        CancellationToken cancellationToken)
    {
        var analyzer = new MethodBranchCoverageAnalyzer();
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

    private async Task<ImmutableArray<Diagnostic>> AnalyzeWithReportAsync(
        string reportXml,
        CancellationToken cancellationToken)
    {
        var analyzer = new MethodBranchCoverageAnalyzer();
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
