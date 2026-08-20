using Automaticks.CSharp.CodeFixes.Documentation;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes.Documentation;

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
    ///     Tests that a comment starting a line with no indentation is still removed.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_CommentAfterLineStartWithNoIndent_RemovesCommentLine(CancellationToken cancellationToken)
    {
        const string source = "namespace MyApp {\n    public class Foo {\n// no indent\n        public void Bar() { }\n    }\n}\n";

        var analyzer = new PlainCommentAnalyzer();
        var provider = new PlainCommentCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).DoesNotContain("no indent");
        await Assert.That(fixedSource).Contains("public void Bar() { }");
    }

    /// <summary>
    ///     Tests that a comment with nothing following it in the file is removed.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_CommentAtAbsoluteEndOfFile_RemovesComment(CancellationToken cancellationToken)
    {
        const string source = "namespace MyApp {\n    public class Foo {\n        public void Bar() { }\n    }\n}\n// trailing comment";

        var analyzer = new PlainCommentAnalyzer();
        var provider = new PlainCommentCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).DoesNotContain("trailing comment");
    }

    /// <summary>
    ///     Tests that a block comment followed by more code on the same line is removed.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_CommentFollowedByCodeOnSameLine_RemovesComment(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() /* note */ { }
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

        await Assert.That(fixedSource).DoesNotContain("note");
        await Assert.That(fixedSource).Contains("public void Bar()");
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
    ///     Tests that a span pointing inside a comment body offers no fix.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountActionsForSpan_PositionInsideCommentBody_ReportsZero(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      // a note
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
        var commentStart = source.IndexOf("// a note", System.StringComparison.Ordinal);
        var span = new TextSpan(commentStart + 3, 1);
        var count = await CodeFixTestRunner.CountActionsForSpanAsync(
            request,
            PlainCommentAnalyzer.Rule,
            span,
            cancellationToken);

        await Assert.That(count).IsEqualTo(0);
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

    /// <summary>
    ///     Tests that the provider always exposes the batch Fix All provider.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetFixAllProvider_Always_ReturnsBatchFixer(CancellationToken cancellationToken)
    {
        var provider = new PlainCommentCodeFixProvider();
        var fixAllProvider = provider.GetFixAllProvider();

        await Assert.That(fixAllProvider).IsEqualTo(WellKnownFixAllProviders.BatchFixer);
    }
}
