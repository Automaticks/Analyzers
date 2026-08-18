using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Complexity;

/// <summary>
///     Tests for NamespaceFileCountAnalyzer.
/// </summary>
public class NamespaceFileCountAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_BlockScopedNestedNamespace_CountsOnlyInnermostNamespace.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BlockScopedNestedNamespace_CountsOnlyInnermostNamespace(CancellationToken cancellationToken)
    {
        var sourceFiles = BuildFileScopedSourceFiles(20, "Outer.Inner");
        var nestedFile = new SourceFile
        {
            Source = "namespace Outer\n{\n    namespace Inner\n    {\n        public class FooNested\n        {\n        }\n    }\n}\n",
            FilePath = "FooNested.cs"
        };
        sourceFiles.Add(nestedFile);

        var analyzer = new NamespaceFileCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, sourceFiles, cancellationToken);

        var diagnostic = DiagnosticCollectionAssertions.FindById(diagnostics, DiagnosticIds.CSharp.NamespaceFileCount);
        await Assert.That(diagnostic).IsNotNull();
        await Assert.That(diagnostic!.GetMessage(CultureInfo.InvariantCulture)).Contains("Outer.Inner");
        await Assert.That(diagnostic.GetMessage(CultureInfo.InvariantCulture)).DoesNotContain("'Outer'");
    }

    /// <summary>
    ///     Tests that Analyze_ChildNamespaceFiles_NotCountedTowardParentNamespace.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ChildNamespaceFiles_NotCountedTowardParentNamespace(CancellationToken cancellationToken)
    {
        var sourceFiles = BuildFileScopedSourceFiles(15, "MyApp");
        sourceFiles.AddRange(BuildFileScopedSourceFiles(10, "MyApp.Services", "Service"));

        var analyzer = new NamespaceFileCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, sourceFiles, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.NamespaceFileCount)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_DiagnosticMessage_ContainsFileCount.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_DiagnosticMessage_ContainsFileCount(CancellationToken cancellationToken)
    {
        var sourceFiles = BuildFileScopedSourceFiles(21, "MyApp");

        var analyzer = new NamespaceFileCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, sourceFiles, cancellationToken);

        var diagnostic = DiagnosticCollectionAssertions.FindById(diagnostics, DiagnosticIds.CSharp.NamespaceFileCount);
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
        var sourceFiles = BuildFileScopedSourceFiles(21, "MyApp");

        var analyzer = new NamespaceFileCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, sourceFiles, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.NamespaceFileCount)).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ExceedingLimit_ReportsSingleDiagnosticForNamespace.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExceedingLimit_ReportsSingleDiagnosticForNamespace(CancellationToken cancellationToken)
    {
        var sourceFiles = BuildFileScopedSourceFiles(25, "MyApp");

        var analyzer = new NamespaceFileCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, sourceFiles, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.CountId(diagnostics, DiagnosticIds.CSharp.NamespaceFileCount)).IsEqualTo(1);
    }

    /// <summary>
    ///     Tests that Analyze_FileReopeningSameNamespaceTwice_CountsFileOnce.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FileReopeningSameNamespaceTwice_CountsFileOnce(CancellationToken cancellationToken)
    {
        var sourceFiles = BuildFileScopedSourceFiles(20, "MyApp");
        var reopenedFile = new SourceFile
        {
            Source = "namespace MyApp\n{\n    public class FooReopenedA\n    {\n    }\n}\n\nnamespace MyApp\n{\n    public class FooReopenedB\n    {\n    }\n}\n",
            FilePath = "FooReopened.cs"
        };
        sourceFiles.Add(reopenedFile);

        var analyzer = new NamespaceFileCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, sourceFiles, cancellationToken);

        var diagnostic = DiagnosticCollectionAssertions.FindById(diagnostics, DiagnosticIds.CSharp.NamespaceFileCount);
        await Assert.That(diagnostic).IsNotNull();
        await Assert.That(diagnostic!.GetMessage(CultureInfo.InvariantCulture)).Contains("21");
    }

    /// <summary>
    ///     Tests that Analyze_FileWithNoNamespace_IsSkipped.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FileWithNoNamespace_IsSkipped(CancellationToken cancellationToken)
    {
        var sourceFiles = BuildFileScopedSourceFiles(21, "MyApp");
        var topLevelFile = new SourceFile
        {
            Source = "System.Console.WriteLine(\"top level\");\n",
            FilePath = "Program.cs"
        };
        sourceFiles.Add(topLevelFile);

        var analyzer = new NamespaceFileCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, sourceFiles, cancellationToken);

        var diagnostic = DiagnosticCollectionAssertions.FindById(diagnostics, DiagnosticIds.CSharp.NamespaceFileCount);
        await Assert.That(diagnostic).IsNotNull();
        await Assert.That(diagnostic!.GetMessage(CultureInfo.InvariantCulture)).Contains("21");
    }

    /// <summary>
    ///     Tests that Analyze_FileWithTwoSiblingNamespaces_CountsTowardBothNamespaces.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FileWithTwoSiblingNamespaces_CountsTowardBothNamespaces(CancellationToken cancellationToken)
    {
        var sourceFiles = new List<SourceFile>(21);
        for (var index = 0; index < 21; index++)
        {
            var sourceFile = new SourceFile
            {
                Source = $"namespace SiblingA\n{{\n    public class FooA{index}\n    {{\n    }}\n}}\n\nnamespace SiblingB\n{{\n    public class FooB{index}\n    {{\n    }}\n}}\n",
                FilePath = $"Sibling{index}.cs"
            };
            sourceFiles.Add(sourceFile);
        }

        var analyzer = new NamespaceFileCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, sourceFiles, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.CountId(diagnostics, DiagnosticIds.CSharp.NamespaceFileCount)).IsEqualTo(2);
    }

    /// <summary>
    ///     Tests that Analyze_GeneratedCodeFilesExcluded_StaysUnderLimit.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_GeneratedCodeFilesExcluded_StaysUnderLimit(CancellationToken cancellationToken)
    {
        var sourceFiles = BuildFileScopedSourceFiles(19, "MyApp");
        var generatedFile1 = new SourceFile
        {
            Source = "// <auto-generated>\nnamespace MyApp;\n\npublic class FooGenerated1\n{\n}\n",
            FilePath = "FooGenerated1.cs"
        };
        sourceFiles.Add(generatedFile1);
        var generatedFile2 = new SourceFile
        {
            Source = "// <auto-generated>\nnamespace MyApp;\n\npublic class FooGenerated2\n{\n}\n",
            FilePath = "FooGenerated2.cs"
        };
        sourceFiles.Add(generatedFile2);

        var analyzer = new NamespaceFileCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, sourceFiles, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.NamespaceFileCount)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_NamespaceAtExactLimit_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NamespaceAtExactLimit_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        var sourceFiles = BuildFileScopedSourceFiles(20, "MyApp");

        var analyzer = new NamespaceFileCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, sourceFiles, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.NamespaceFileCount)).IsFalse();
    }

    private List<SourceFile> BuildFileScopedSourceFiles(int count, string namespaceName)
    {
        return BuildFileScopedSourceFiles(count, namespaceName, "Foo");
    }

    private List<SourceFile> BuildFileScopedSourceFiles(int count, string namespaceName, string namePrefix)
    {
        var sourceFiles = new List<SourceFile>(count);
        for (var index = 0; index < count; index++)
        {
            var sourceFile = new SourceFile
            {
                Source = $"namespace {namespaceName};\n\npublic class {namePrefix}{index}\n{{\n}}\n",
                FilePath = $"{namespaceName}_{namePrefix}{index}.cs"
            };
            sourceFiles.Add(sourceFile);
        }

        return sourceFiles;
    }
}
