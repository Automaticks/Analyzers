using Automaticks.Testing.Coverage;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Testing.Analyzers.Tests.Coverage;

/// <summary>
///     Tests for CoverageReport.
/// </summary>
public class CoverageReportTests
{
    /// <summary>
    ///     Tests that Constructor_ClassElementMissingFilename_LeavesReportUnpopulated.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_ClassElementMissingFilename_LeavesReportUnpopulated(CancellationToken cancellationToken)
    {
        const string reportXml = """
                                 <coverage version="1.9"><packages><package name="MyApp"><classes>
                                   <class name="MyApp.Foo"><methods>
                                     <method name="Bar"><lines><line number="3" hits="1" /></lines></method>
                                   </methods><lines><line number="3" hits="1" /></lines></class>
                                 </classes></package></packages></coverage>
                                 """;

        var report = new CoverageReport(reportXml);

        await Assert.That(report.IsPopulated).IsFalse();
    }

    /// <summary>
    ///     Tests that Constructor_LineElementWithNonNumericAttributes_DefaultsCountersToZero.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_LineElementWithNonNumericAttributes_DefaultsCountersToZero(CancellationToken cancellationToken)
    {
        const string reportXml = """
                                 <coverage version="1.9"><packages><package name="MyApp"><classes>
                                   <class name="MyApp.Foo" filename="MyApp/Foo.cs"><lines>
                                     <line number="abc" hits="xyz" />
                                   </lines></class>
                                 </classes></package></packages></coverage>
                                 """;

        var report = new CoverageReport(reportXml);
        var file = report.FindFile("C:/repo/MyApp/Foo.cs");

        await Assert.That(file).IsNotNull();
        await Assert.That(file!.TotalLines).IsEqualTo(1);
        await Assert.That(file.CoveredLines).IsEqualTo(0);
    }

    /// <summary>
    ///     Tests that Constructor_MethodElementMissingName_SkipsMethodTracking.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_MethodElementMissingName_SkipsMethodTracking(CancellationToken cancellationToken)
    {
        const string reportXml = """
                                 <coverage version="1.9"><packages><package name="MyApp"><classes>
                                   <class name="MyApp.Foo" filename="MyApp/Foo.cs"><methods>
                                     <method><lines><line number="3" hits="1" /></lines></method>
                                   </methods></class>
                                 </classes></package></packages></coverage>
                                 """;

        var report = new CoverageReport(reportXml);
        var file = report.FindFile("C:/repo/MyApp/Foo.cs");

        await Assert.That(file).IsNotNull();
        await Assert.That(file!.TotalLines).IsEqualTo(1);
        await Assert.That(file.FindMethod("Bar")).IsNull();
    }

    /// <summary>
    ///     Tests that FindFile_EmptyDocumentPath_ReturnsNull.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindFile_EmptyDocumentPath_ReturnsNull(CancellationToken cancellationToken)
    {
        const string reportXml = """
                                 <coverage version="1.9"><packages><package name="MyApp"><classes>
                                   <class name="MyApp.Foo" filename="MyApp/Foo.cs"><lines>
                                     <line number="3" hits="1" />
                                   </lines></class>
                                 </classes></package></packages></coverage>
                                 """;

        var report = new CoverageReport(reportXml);

        await Assert.That(report.FindFile(string.Empty)).IsNull();
    }

    /// <summary>
    ///     Tests that FindFile_MultipleCandidatesWithMatchingSuffix_ReturnsMatchingCandidate.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindFile_MultipleCandidatesWithMatchingSuffix_ReturnsMatchingCandidate(CancellationToken cancellationToken)
    {
        const string reportXml = """
                                 <coverage version="1.9"><packages><package name="MyApp"><classes>
                                   <class name="MyApp.ProjectA.Shared" filename="ProjectA/Shared.cs"><lines><line number="1" hits="1" /></lines></class>
                                   <class name="MyApp.ProjectB.Shared" filename="ProjectB/Shared.cs"><lines><line number="1" hits="1" /></lines></class>
                                 </classes></package></packages></coverage>
                                 """;

        var report = new CoverageReport(reportXml);
        var file = report.FindFile("C:/repo/ProjectB/Shared.cs");

        await Assert.That(file).IsNotNull();
        await Assert.That(file!.ReportedPath).IsEqualTo("ProjectB/Shared.cs");
    }

    /// <summary>
    ///     Tests that FindFile_MultipleCandidatesWithNoMatchingSuffix_ReturnsNull.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindFile_MultipleCandidatesWithNoMatchingSuffix_ReturnsNull(CancellationToken cancellationToken)
    {
        const string reportXml = """
                                 <coverage version="1.9"><packages><package name="MyApp"><classes>
                                   <class name="MyApp.ProjectA.Shared" filename="ProjectA/Shared.cs"><lines><line number="1" hits="1" /></lines></class>
                                   <class name="MyApp.ProjectB.Shared" filename="ProjectB/Shared.cs"><lines><line number="1" hits="1" /></lines></class>
                                 </classes></package></packages></coverage>
                                 """;

        var report = new CoverageReport(reportXml);

        await Assert.That(report.FindFile("C:/repo/ProjectC/Shared.cs")).IsNull();
    }

    /// <summary>
    ///     Tests that FindFile_NoDirectorySeparatorInPath_MatchesByBareFileName.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindFile_NoDirectorySeparatorInPath_MatchesByBareFileName(CancellationToken cancellationToken)
    {
        const string reportXml = """
                                 <coverage version="1.9"><packages><package name="MyApp"><classes>
                                   <class name="MyApp.Foo" filename="Foo.cs"><lines><line number="1" hits="1" /></lines></class>
                                 </classes></package></packages></coverage>
                                 """;

        var report = new CoverageReport(reportXml);

        await Assert.That(report.FindFile("Foo.cs")).IsNotNull();
    }

    /// <summary>
    ///     Tests that Include_SecondReportWithSameFilePath_MergesIntoExistingFileEntry.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Include_SecondReportWithSameFilePath_MergesIntoExistingFileEntry(CancellationToken cancellationToken)
    {
        const string firstReportXml = """
                                      <coverage version="1.9"><packages><package name="MyApp"><classes>
                                        <class name="MyApp.Foo" filename="MyApp/Foo.cs"><lines><line number="3" hits="0" /></lines></class>
                                      </classes></package></packages></coverage>
                                      """;
        const string secondReportXml = """
                                       <coverage version="1.9"><packages><package name="MyApp"><classes>
                                         <class name="MyApp.Foo" filename="MyApp/Foo.cs"><lines><line number="3" hits="1" /></lines></class>
                                       </classes></package></packages></coverage>
                                       """;

        var report = new CoverageReport(firstReportXml);
        report.Include(secondReportXml);
        var file = report.FindFile("C:/repo/MyApp/Foo.cs");

        await Assert.That(file).IsNotNull();
        await Assert.That(file!.TotalLines).IsEqualTo(1);
        await Assert.That(file.CoveredLines).IsEqualTo(1);
    }
}
