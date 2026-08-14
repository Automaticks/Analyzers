using Automaticks.CSharp.CodeFixes.Documentation;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes;

/// <summary>
///     Tests for PlainCommentCodeFixProvider.
/// </summary>
public class PlainCommentCodeFixProviderTests
{
    /// <summary>
    ///     Tests that repeated application clears every plain comment.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyAllFixes_SeveralComments_RemovesEveryOne(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      // first note
                                      public void Bar() { }

                                      /* second note */
                                      public void Baz() { }
                                  }
                              }
                              """;

        var analyzer = new PlainCommentAnalyzer();
        var provider = new PlainCommentCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyAllFixesAsync(request, cancellationToken);

        await Assert.That(fixedSource).DoesNotContain("first note");
        await Assert.That(fixedSource).DoesNotContain("second note");
    }

    /// <summary>
    ///     Tests that a block comment is removed.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_BlockComment_RemovesComment(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      /* a note */
                                      public void Bar() { }
                                  }
                              }
                              """;

        var analyzer = new PlainCommentAnalyzer();
        var provider = new PlainCommentCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).DoesNotContain("a note");
        await Assert.That(fixedSource).Contains("public void Bar() { }");
    }

    /// <summary>
    ///     Tests that a trailing comment on a code line is removed.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_TrailingComment_RemovesComment(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() { } // a note
                                  }
                              }
                              """;

        var analyzer = new PlainCommentAnalyzer();
        var provider = new PlainCommentCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).DoesNotContain("a note");
        await Assert.That(fixedSource).Contains("public void Bar() { }");
    }

    /// <summary>
    ///     Tests that an XML documentation comment is never offered a fix.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountFixable_DocumentationComment_ReportsZero(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///     Does a thing.
                                  /// </summary>
                                  public class Foo { }
                              }
                              """;

        var analyzer = new PlainCommentAnalyzer();
        var provider = new PlainCommentCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var count = await CodeFixTestRunner.CountFixableAsync(request, cancellationToken);

        await Assert.That(count).IsEqualTo(0);
    }
}
