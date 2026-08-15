using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Threading;

namespace Automaticks.Testing.Coverage;

/// <summary>
///     Locates and parses the coverage report supplied to a compilation through AdditionalFiles.
///     Analyzers must not touch the file system directly, so the report can only arrive this way.
/// </summary>
public static class CoverageReportLocator
{
    private const string MetadataKey = "build_metadata.AdditionalFiles.IsCoverageReport";
    private const string ReportSuffix = ".cobertura.xml";

    /// <summary>
    ///     Finds the first usable coverage report among the additional files.
    /// </summary>
    /// <param name="options">The analyzer options carrying the additional files.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The parsed report, or <see langword="null" /> when none was supplied.</returns>
    public static CoverageReport? Find(AnalyzerOptions options, CancellationToken cancellationToken)
    {
        foreach (var additionalFile in options.AdditionalFiles)
        {
            if (!HasCoverageReportMarker(options, additionalFile))
            {
                continue;
            }

            var text = additionalFile.GetText(cancellationToken);
            if (text is null)
            {
                continue;
            }

            var report = new CoverageReport(text.ToString());
            if (report.IsPopulated)
            {
                return report;
            }
        }

        return null;
    }

    private static bool HasCoverageReportMarker(AnalyzerOptions options, AdditionalText additionalFile)
    {
        var fileOptions = options.AnalyzerConfigOptionsProvider.GetOptions(additionalFile);
        if (fileOptions.TryGetValue(MetadataKey, out var flag)
            && string.Equals(flag, "true", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return additionalFile.Path.EndsWith(ReportSuffix, StringComparison.OrdinalIgnoreCase);
    }
}
