using Automaticks.CSharp.Complexity;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Complexity;

/// <summary>
///     Tests NamespaceFileCountAnalyzer when the compilation carries no file paths.
/// </summary>
public class NamespaceFileCountAnalyzerMissingPathTests
{
    /// <summary>
    ///     Tests that trees with no path are skipped instead of counted together.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TreesWithoutFilePath_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        var sourceFiles = new List<SourceFile>(21);
        for (var index = 0; index < 21; index++)
        {
            var sourceFile = new SourceFile
            {
                Source = $"namespace MyApp;\n\npublic class Foo{index}\n{{\n}}\n",
                FilePath = string.Empty
            };
            sourceFiles.Add(sourceFile);
        }

        var analyzer = new NamespaceFileCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, sourceFiles, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.NamespaceFileCount)).IsFalse();
    }
}
