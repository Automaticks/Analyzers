using Automaticks.CSharp.CodeFixes.Formatting;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes.Formatting;

/// <summary>
///     Tests for DuplicateUsingDirectiveCodeFixProvider.
/// </summary>
public class DuplicateUsingDirectiveCodeFixProviderTests
{
    /// <summary>
    ///     Tests that repeated application clears every duplicate directive.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyAllFixes_SeveralDuplicates_LeavesNoDuplicate(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              using System.Text;
                              using System;
                              using System.Text;

                              namespace MyApp {
                                  public class Foo { }
                              }
                              """;

        var analyzer = new DuplicateUsingDirectiveAnalyzer();
        var provider = new DuplicateUsingDirectiveCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyAllFixesAsync(request, cancellationToken);
        var verifyRequest = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = fixedSource
        };
        var remaining = await CodeFixTestRunner.CountFixableAsync(verifyRequest, cancellationToken);

        await Assert.That(remaining).IsEqualTo(0);
    }

    /// <summary>
    ///     Tests that the duplicate directive is removed and a single copy kept.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_DuplicateDirective_KeepsSingleCopy(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              using System;

                              namespace MyApp {
                                  public class Foo { }
                              }
                              """;

        var analyzer = new DuplicateUsingDirectiveAnalyzer();
        var provider = new DuplicateUsingDirectiveCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);
        var verifyRequest = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = fixedSource
        };
        var remaining = await CodeFixTestRunner.CountFixableAsync(verifyRequest, cancellationToken);

        await Assert.That(fixedSource).Contains("using System;");
        await Assert.That(remaining).IsEqualTo(0);
    }

    /// <summary>
    ///     Tests that a span outside any using directive offers no action.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountActionsForSpan_SpanOutsideUsingDirective_ReportsZero(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;

                              namespace MyApp {
                                  public class Foo { }
                              }
                              """;

        var analyzer = new DuplicateUsingDirectiveAnalyzer();
        var provider = new DuplicateUsingDirectiveCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var start = source.IndexOf("Foo", StringComparison.Ordinal);
        var span = new TextSpan(start, "Foo".Length);
        var count = await CodeFixTestRunner.CountActionsForSpanAsync(
            request,
            DuplicateUsingDirectiveAnalyzer.Rule,
            span,
            cancellationToken);

        await Assert.That(count).IsEqualTo(0);
    }

    /// <summary>
    ///     Tests that source without duplicates offers no fix.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountFixable_NoDuplicates_ReportsZero(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              using System.Text;

                              namespace MyApp {
                                  public class Foo { }
                              }
                              """;

        var analyzer = new DuplicateUsingDirectiveAnalyzer();
        var provider = new DuplicateUsingDirectiveCodeFixProvider();
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
        var provider = new DuplicateUsingDirectiveCodeFixProvider();
        var fixAllProvider = provider.GetFixAllProvider();

        await Assert.That(fixAllProvider).IsEqualTo(WellKnownFixAllProviders.BatchFixer);
    }
}
