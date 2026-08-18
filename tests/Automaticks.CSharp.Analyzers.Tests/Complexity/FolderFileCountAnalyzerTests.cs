using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Complexity;

/// <summary>
///     Tests for FolderFileCountAnalyzer.
/// </summary>
public class FolderFileCountAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_DiagnosticMessage_ContainsFileCount.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_DiagnosticMessage_ContainsFileCount(CancellationToken cancellationToken)
    {
        var sourceFiles = BuildSourceFiles(21, "FolderA");

        var analyzer = new FolderFileCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, sourceFiles, cancellationToken);

        var diagnostic = DiagnosticCollectionAssertions.FindById(diagnostics, DiagnosticIds.CSharp.FolderFileCount);
        await Assert.That(diagnostic).IsNotNull();
        await Assert.That(diagnostic!.GetMessage(CultureInfo.InvariantCulture)).Contains("21");
    }

    /// <summary>
    ///     Tests that Analyze_ExceedingLimit_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExceedingLimit_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        var sourceFiles = BuildSourceFiles(21, "FolderA");

        var analyzer = new FolderFileCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, sourceFiles, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.FolderFileCount)).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ExceedingLimit_ReportsSingleDiagnosticForFolder.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExceedingLimit_ReportsSingleDiagnosticForFolder(CancellationToken cancellationToken)
    {
        var sourceFiles = BuildSourceFiles(25, "FolderA");

        var analyzer = new FolderFileCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, sourceFiles, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.CountId(diagnostics, DiagnosticIds.CSharp.FolderFileCount)).IsEqualTo(1);
    }

    /// <summary>
    ///     Tests that Analyze_FileAtRootWithNoDirectory_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FileAtRootWithNoDirectory_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        var sourceFile = new SourceFile
        {
            Source = "namespace MyApp;\n\npublic class Foo\n{\n}\n",
            FilePath = "Foo.cs"
        };
        List<SourceFile> sourceFiles = [sourceFile];

        var analyzer = new FolderFileCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, sourceFiles, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.FolderFileCount)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_FilesInSubfolder_NotCountedTowardParentFolder.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FilesInSubfolder_NotCountedTowardParentFolder(CancellationToken cancellationToken)
    {
        var sourceFiles = BuildSourceFiles(15, "Root");
        sourceFiles.AddRange(BuildSourceFiles(10, Path.Combine("Root", "Sub"), "Sub"));

        var analyzer = new FolderFileCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, sourceFiles, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.FolderFileCount)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_FilesSplitAcrossDifferentFolders_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FilesSplitAcrossDifferentFolders_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        var sourceFiles = BuildSourceFiles(11, "FolderA");
        sourceFiles.AddRange(BuildSourceFiles(10, "FolderB", "B"));

        var analyzer = new FolderFileCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, sourceFiles, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.FolderFileCount)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_FileWithNoPath_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FileWithNoPath_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = "namespace MyApp;\n\npublic class Foo\n{\n}\n";

        var analyzer = new FolderFileCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.FolderFileCount)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_FolderAtExactLimit_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FolderAtExactLimit_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        var sourceFiles = BuildSourceFiles(20, "FolderA");

        var analyzer = new FolderFileCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, sourceFiles, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.FolderFileCount)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_GeneratedCodeFilesExcluded_StaysUnderLimit.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_GeneratedCodeFilesExcluded_StaysUnderLimit(CancellationToken cancellationToken)
    {
        var sourceFiles = BuildSourceFiles(19, "FolderA");
        var generatedFile1 = new SourceFile
        {
            Source = "// <auto-generated>\nnamespace MyApp;\n\npublic class FooGenerated1\n{\n}\n",
            FilePath = Path.Combine("FolderA", "FooGenerated1.cs")
        };
        sourceFiles.Add(generatedFile1);
        var generatedFile2 = new SourceFile
        {
            Source = "// <auto-generated>\nnamespace MyApp;\n\npublic class FooGenerated2\n{\n}\n",
            FilePath = Path.Combine("FolderA", "FooGenerated2.cs")
        };
        sourceFiles.Add(generatedFile2);

        var analyzer = new FolderFileCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, sourceFiles, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.FolderFileCount)).IsFalse();
    }

    private List<SourceFile> BuildSourceFiles(int count, string directory)
    {
        return BuildSourceFiles(count, directory, "Foo");
    }

    private List<SourceFile> BuildSourceFiles(int count, string directory, string namePrefix)
    {
        var sourceFiles = new List<SourceFile>(count);
        for (var index = 0; index < count; index++)
        {
            var sourceFile = new SourceFile
            {
                Source = $"namespace MyApp;\n\npublic class {namePrefix}{index}\n{{\n}}\n",
                FilePath = Path.Combine(directory, $"{namePrefix}{index}.cs")
            };
            sourceFiles.Add(sourceFile);
        }

        return sourceFiles;
    }
}
