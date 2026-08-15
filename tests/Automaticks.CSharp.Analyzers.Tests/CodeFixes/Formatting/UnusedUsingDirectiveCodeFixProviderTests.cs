using Automaticks.CSharp.CodeFixes.Formatting;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes.Formatting;

/// <summary>
///     Tests for UnusedUsingDirectiveCodeFixProvider.
/// </summary>
public class UnusedUsingDirectiveCodeFixProviderTests
{
    /// <summary>
    ///     Tests that repeated application clears every unused directive.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyAllFixes_SeveralUnusedDirectives_RemovesEveryOne(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Text;
                              using System.IO;

                              namespace MyApp {
                                  public class Foo { }
                              }
                              """;

        var analyzer = new UnusedUsingDirectiveAnalyzer();
        var provider = new UnusedUsingDirectiveCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyAllFixesAsync(request, cancellationToken);

        await Assert.That(fixedSource).DoesNotContain("using System.Text;");
        await Assert.That(fixedSource).DoesNotContain("using System.IO;");
    }

    /// <summary>
    ///     Tests that a single unused directive is removed.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_UnusedDirective_RemovesDirective(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Text;

                              namespace MyApp {
                                  public class Foo { }
                              }
                              """;

        var analyzer = new UnusedUsingDirectiveAnalyzer();
        var provider = new UnusedUsingDirectiveCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).DoesNotContain("using System.Text;");
        await Assert.That(fixedSource).Contains("public class Foo");
    }

    /// <summary>
    ///     Tests that a referenced directive is never offered a fix.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountFixable_UsedDirective_ReportsZero(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Text;

                              namespace MyApp {
                                  public class Foo {
                                      public StringBuilder Builder { get; }
                                  }
                              }
                              """;

        var analyzer = new UnusedUsingDirectiveAnalyzer();
        var provider = new UnusedUsingDirectiveCodeFixProvider();
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
