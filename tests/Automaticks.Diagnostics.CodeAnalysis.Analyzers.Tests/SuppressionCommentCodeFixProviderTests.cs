using Automaticks.Diagnostics.CodeAnalysis.Analyzers.Tests.Stubs;
using Automaticks.Diagnostics.CodeAnalysis.CodeFixes;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Diagnostics.CodeAnalysis.Analyzers.Tests;

/// <summary>Tests for SuppressionCommentCodeFixProvider.</summary>
public class SuppressionCommentCodeFixProviderTests
{
    /// <summary>Verifies repeated application clears both suppression forms in one document.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyAllFixes_BothSuppressionForms_RemovesEveryOccurrence(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                              #pragma warning disable ATXCS011
                                      // ReSharper disable once UnusedMember.Local
                                      private static void Helper() { }
                                  }
                              }
                              """;

        var analyzer = new SuppressionCommentAnalyzer();
        var provider = new SuppressionCommentCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyAllFixesAsync(request, cancellationToken);

        await Assert.That(fixedSource).DoesNotContain("#pragma warning disable");
        await Assert.That(fixedSource).DoesNotContain("ReSharper disable");
    }

    /// <summary>Verifies a pragma disable directive is deleted.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_PragmaDisable_RemovesDirective(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                              #pragma warning disable ATXCS011
                                      private static void Helper() { }
                                  }
                              }
                              """;

        var analyzer = new SuppressionCommentAnalyzer();
        var provider = new SuppressionCommentCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).DoesNotContain("#pragma warning disable");
        await Assert.That(fixedSource).Contains("private static void Helper() { }");
    }

    /// <summary>Verifies the removed comment leaves no dangling blank line behind.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_ReSharperDisable_LeavesNoBlankLine(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      // ReSharper disable once UnusedMember.Local
                                      private static void Helper() { }
                                  }
                              }
                              """;

        var analyzer = new SuppressionCommentAnalyzer();
        var provider = new SuppressionCommentCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).DoesNotContain("\n\n");
    }

    /// <summary>Verifies a ReSharper disable comment is deleted.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_ReSharperDisable_RemovesComment(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      // ReSharper disable once UnusedMember.Local
                                      private static void Helper() { }
                                  }
                              }
                              """;

        var analyzer = new SuppressionCommentAnalyzer();
        var provider = new SuppressionCommentCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).DoesNotContain("ReSharper disable");
        await Assert.That(fixedSource).Contains("private static void Helper() { }");
    }

    /// <summary>Verifies a ReSharper disable comment at the very end of the file, with no trailing newline, is still removed.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_ReSharperDisableAtEndOfFile_RemovesComment(CancellationToken cancellationToken)
    {
        const string source = "namespace MyApp {\n"
            + "    public class Foo {\n"
            + "        private static void Helper() { }\n"
            + "    }\n"
            + "}\n"
            + "// ReSharper disable once UnusedMember.Local";

        var analyzer = new SuppressionCommentAnalyzer();
        var provider = new SuppressionCommentCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).DoesNotContain("ReSharper disable");
    }

    /// <summary>Verifies a ReSharper disable comment at the very start of the file, with no leading trivia before it, is still removed.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_ReSharperDisableAtStartOfFile_RemovesComment(CancellationToken cancellationToken)
    {
        const string source = """
                              // ReSharper disable once UnusedMember.Local
                              namespace MyApp {
                                  public class Foo {
                                      private static void Helper() { }
                                  }
                              }
                              """;

        var analyzer = new SuppressionCommentAnalyzer();
        var provider = new SuppressionCommentCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).DoesNotContain("ReSharper disable");
    }

    /// <summary>Verifies a ReSharper disable comment preceded by a blank line is still removed.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_ReSharperDisablePrecededByBlankLine_RemovesComment(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {

                                      // ReSharper disable once UnusedMember.Local
                                      private static void Helper() { }
                                  }
                              }
                              """;

        var analyzer = new SuppressionCommentAnalyzer();
        var provider = new SuppressionCommentCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).DoesNotContain("ReSharper disable");
    }

    /// <summary>Verifies a ReSharper disable comment attached as trailing trivia is still removed.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_ReSharperDisableTrailingComment_RemovesComment(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private static void Helper() { } // ReSharper disable once UnusedMember.Local
                                  }
                              }
                              """;

        var analyzer = new SuppressionCommentAnalyzer();
        var provider = new SuppressionCommentCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).DoesNotContain("ReSharper disable");
    }

    /// <summary>Verifies a pragma restore directive is never offered a fix.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountFixable_PragmaRestoreOnly_ReportsZero(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                              #pragma warning restore ATXCS011
                                      private static void Helper() { }
                                  }
                              }
                              """;

        var analyzer = new SuppressionCommentAnalyzer();
        var provider = new SuppressionCommentCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var count = await CodeFixTestRunner.CountFixableAsync(request, cancellationToken);

        await Assert.That(count).IsEqualTo(0);
    }

    /// <summary>Verifies GetFixAllProvider returns a non-null batch fixer.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetFixAllProvider_Called_ReturnsBatchFixer(CancellationToken cancellationToken)
    {
        var provider = new SuppressionCommentCodeFixProvider();
        var fixAllProvider = provider.GetFixAllProvider();

        await Assert.That(fixAllProvider).IsNotNull();
    }

    /// <summary>Verifies a diagnostic reported outside any trivia registers no fix.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RegisterCodeFixes_DiagnosticOutsideTrivia_RegistersNoFix(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                  }
                              }
                              """;

        var analyzer = new SuppressionPragmaAtNonTriviaLocationAnalyzer();
        var provider = new SuppressionCommentCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var count = await CodeFixTestRunner.CountRegisteredActionsAsync(request, cancellationToken);

        await Assert.That(count).IsEqualTo(0);
    }
}
