using Automaticks.CSharp.CodeFixes.Naming;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes.Naming;

/// <summary>
///     Tests for FileNameMismatchCodeFixProvider.
/// </summary>
public class FileNameMismatchCodeFixProviderTests
{
    /// <summary>
    ///     Tests that a compound extension is preserved when renaming.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_CompoundExtension_KeepsEveryExtension(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class DialogView { }
                              }
                              """;

        var analyzer = new FileNameMismatchAnalyzer();
        var provider = new FileNameMismatchCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source,
            FilePath = "Other.axaml.cs"
        };
        var documentName = await CodeFixTestRunner.GetFixedDocumentNameAsync(request, cancellationToken);

        await Assert.That(documentName).IsEqualTo("DialogView.axaml.cs");
    }

    /// <summary>
    ///     Tests that the source text itself is left untouched by the rename.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_MismatchedFile_KeepsSourceUnchanged(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Widget { }
                              }
                              """;

        var analyzer = new FileNameMismatchAnalyzer();
        var provider = new FileNameMismatchCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source,
            FilePath = "Gadget.cs"
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("public class Widget { }");
    }

    /// <summary>
    ///     Tests that the document is renamed to match the type.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_MismatchedFile_RenamesDocumentToTypeName(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Widget { }
                              }
                              """;

        var analyzer = new FileNameMismatchAnalyzer();
        var provider = new FileNameMismatchCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source,
            FilePath = "Gadget.cs"
        };
        var documentName = await CodeFixTestRunner.GetFixedDocumentNameAsync(request, cancellationToken);

        await Assert.That(documentName).IsEqualTo("Widget.cs");
    }

    /// <summary>
    ///     Tests that a matching file name is never offered a fix.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountFixable_MatchingFileName_ReportsZero(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Widget { }
                              }
                              """;

        var analyzer = new FileNameMismatchAnalyzer();
        var provider = new FileNameMismatchCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source,
            FilePath = "Widget.cs"
        };
        var count = await CodeFixTestRunner.CountFixableAsync(request, cancellationToken);

        await Assert.That(count).IsEqualTo(0);
    }
}
