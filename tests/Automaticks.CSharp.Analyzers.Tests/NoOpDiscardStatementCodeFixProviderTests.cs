using Automaticks.CSharp.CodeFixes.LanguageFeatures;
using Automaticks.CSharp.LanguageFeatures;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

/// <summary>Tests for NoOpDiscardStatementCodeFixProvider.</summary>
public class NoOpDiscardStatementCodeFixProviderTests
{
    /// <summary>Verifies repeated application removes every no-op discard in the document.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyAllFixes_MultipleDiscards_RemovesEveryOccurrence(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Bar(int input) { _ = this; _ = input; return 1; }
                                      public int Baz() { _ = this; return 2; }
                                  }
                              }
                              """;

        var analyzer = new NoOpDiscardStatementAnalyzer();
        var provider = new NoOpDiscardStatementCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyAllFixesAsync(request, cancellationToken);

        await Assert.That(fixedSource).DoesNotContain("_ = this");
        await Assert.That(fixedSource).DoesNotContain("_ = input");
    }

    /// <summary>Verifies the discard statement is deleted and surrounding code is kept.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_DiscardOfThis_RemovesStatement(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Bar() { _ = this; return 1; }
                                  }
                              }
                              """;

        var analyzer = new NoOpDiscardStatementAnalyzer();
        var provider = new NoOpDiscardStatementCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).DoesNotContain("_ = this");
        await Assert.That(fixedSource).Contains("return 1;");
    }

    /// <summary>Verifies GetFixAllProvider returns the batch fixer.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetFixAllProvider_Called_ReturnsBatchFixer(CancellationToken cancellationToken)
    {
        var provider = new NoOpDiscardStatementCodeFixProvider();
        var fixAllProvider = provider.GetFixAllProvider();

        await Assert.That(fixAllProvider).IsNotNull();
    }
}
