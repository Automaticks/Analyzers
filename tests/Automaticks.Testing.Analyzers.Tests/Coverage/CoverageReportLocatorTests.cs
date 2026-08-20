using Automaticks.Testing.Analyzers.Tests.Stubs;
using Automaticks.Testing.Coverage;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Testing.Analyzers.Tests.Coverage;

/// <summary>
///     Tests for CoverageReportLocator.
/// </summary>
public class CoverageReportLocatorTests
{
    private const string UsableReportXml = """
                                           <coverage version="1.9"><packages><package name="MyApp"><classes>
                                             <class name="MyApp.Foo" filename="MyApp/Foo.cs"><lines><line number="1" hits="1" /></lines></class>
                                           </classes></package></packages></coverage>
                                           """;

    /// <summary>
    ///     Tests that Find_AdditionalFileNotMarkedAsReport_SkipsFile.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Find_AdditionalFileNotMarkedAsReport_SkipsFile(CancellationToken cancellationToken)
    {
        var unrelatedFile = new TestAdditionalText("C:/repo/notes.txt", "not a coverage report");
        var reportFile = new TestAdditionalText("C:/repo/coverage.cobertura.xml", UsableReportXml);
        var additionalFiles = ImmutableArray.Create<AdditionalText>(unrelatedFile, reportFile);
        var optionsProvider = new TestAnalyzerConfigOptionsHolder(false, false, null);
        var options = new AnalyzerOptions(additionalFiles, optionsProvider);

        var report = CoverageReportLocator.Find(options, cancellationToken);

        await Assert.That(report).IsNotNull();
        await Assert.That(report!.FindFile("C:/repo/MyApp/Foo.cs")).IsNotNull();
    }

    /// <summary>
    ///     Tests that Find_AdditionalFileTextIsNull_SkipsFile.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Find_AdditionalFileTextIsNull_SkipsFile(CancellationToken cancellationToken)
    {
        var unreadableFile = new TestAdditionalText("C:/repo/coverage.cobertura.xml", null);
        var additionalFiles = ImmutableArray.Create<AdditionalText>(unreadableFile);
        var optionsProvider = new TestAnalyzerConfigOptionsHolder(false, false, null);
        var options = new AnalyzerOptions(additionalFiles, optionsProvider);

        var report = CoverageReportLocator.Find(options, cancellationToken);

        await Assert.That(report).IsNull();
    }

    /// <summary>
    ///     Tests that Find_MarkerMetadataFalse_FallsBackToSuffixCheck.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Find_MarkerMetadataFalse_FallsBackToSuffixCheck(CancellationToken cancellationToken)
    {
        var nonSuffixedFile = new TestAdditionalText("C:/repo/report.notxml", UsableReportXml);
        var additionalFiles = ImmutableArray.Create<AdditionalText>(nonSuffixedFile);
        var configOptions = new Dictionary<string, string>
        {
            ["build_metadata.AdditionalFiles.IsCoverageReport"] = "false",
        };
        var optionsProvider = new TestAnalyzerConfigOptionsHolder(false, false, configOptions);
        var options = new AnalyzerOptions(additionalFiles, optionsProvider);

        var report = CoverageReportLocator.Find(options, cancellationToken);

        await Assert.That(report).IsNull();
    }

    /// <summary>
    ///     Tests that Find_MarkerMetadataTrue_TreatsNonSuffixedFileAsReport.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Find_MarkerMetadataTrue_TreatsNonSuffixedFileAsReport(CancellationToken cancellationToken)
    {
        var nonSuffixedFile = new TestAdditionalText("C:/repo/report.notxml", UsableReportXml);
        var additionalFiles = ImmutableArray.Create<AdditionalText>(nonSuffixedFile);
        var configOptions = new Dictionary<string, string>
        {
            ["build_metadata.AdditionalFiles.IsCoverageReport"] = "true",
        };
        var optionsProvider = new TestAnalyzerConfigOptionsHolder(false, false, configOptions);
        var options = new AnalyzerOptions(additionalFiles, optionsProvider);

        var report = CoverageReportLocator.Find(options, cancellationToken);

        await Assert.That(report).IsNotNull();
    }

    /// <summary>
    ///     Tests that FindUnusableReportPaths_AdditionalFileNotMarkedAsReport_SkipsFile.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindUnusableReportPaths_AdditionalFileNotMarkedAsReport_SkipsFile(CancellationToken cancellationToken)
    {
        var unrelatedFile = new TestAdditionalText("C:/repo/notes.txt", "not a coverage report");
        var additionalFiles = ImmutableArray.Create<AdditionalText>(unrelatedFile);
        var optionsProvider = new TestAnalyzerConfigOptionsHolder(false, false, null);
        var options = new AnalyzerOptions(additionalFiles, optionsProvider);

        var unusablePaths = CoverageReportLocator.FindUnusableReportPaths(options, cancellationToken);

        await Assert.That(unusablePaths).IsEmpty();
    }

    /// <summary>
    ///     Tests that FindUnusableReportPaths_AdditionalFileTextIsNull_TreatsAsUnusable.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindUnusableReportPaths_AdditionalFileTextIsNull_TreatsAsUnusable(CancellationToken cancellationToken)
    {
        var unreadableFile = new TestAdditionalText("C:/repo/coverage.cobertura.xml", null);
        var additionalFiles = ImmutableArray.Create<AdditionalText>(unreadableFile);
        var optionsProvider = new TestAnalyzerConfigOptionsHolder(false, false, null);
        var options = new AnalyzerOptions(additionalFiles, optionsProvider);

        var unusablePaths = CoverageReportLocator.FindUnusableReportPaths(options, cancellationToken);

        await Assert.That(unusablePaths).Contains("C:/repo/coverage.cobertura.xml");
    }
}
