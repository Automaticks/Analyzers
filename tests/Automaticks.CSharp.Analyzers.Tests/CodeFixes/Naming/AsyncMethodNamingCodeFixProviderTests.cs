using Automaticks.CSharp.CodeFixes.Naming;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes.Naming;

/// <summary>
///     Tests for AsyncMethodNamingCodeFixProvider.
/// </summary>
public class AsyncMethodNamingCodeFixProviderTests
{
    /// <summary>
    ///     Tests that call sites are renamed along with the declaration.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_MethodWithCallSite_RenamesCallSite(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public Task Work() { return Task.CompletedTask; }

                                      public Task Run() { return Work(); }
                                  }
                              }
                              """;

        var analyzer = new AsyncMethodNamingAnalyzer();
        var provider = new AsyncMethodNamingCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("public Task WorkAsync()");
        await Assert.That(fixedSource).Contains("return WorkAsync();");
    }

    /// <summary>
    ///     Tests that the renamed method no longer reports the rule.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_TaskReturningMethod_LeavesNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public Task Work() { return Task.CompletedTask; }
                                  }
                              }
                              """;

        var analyzer = new AsyncMethodNamingAnalyzer();
        var provider = new AsyncMethodNamingCodeFixProvider();
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

        await Assert.That(remaining).IsEqualTo(0);
    }

    /// <summary>
    ///     Tests that a span outside any method declaration offers no action.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountActionsForSpan_SpanOutsideMethod_ReportsZero(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { }
                              }
                              """;

        var analyzer = new AsyncMethodNamingAnalyzer();
        var provider = new AsyncMethodNamingCodeFixProvider();
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
            AsyncMethodNamingAnalyzer.Rule,
            span,
            cancellationToken);

        await Assert.That(count).IsEqualTo(0);
    }

    /// <summary>
    ///     Tests that a correctly named method is never offered a fix.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountFixable_SuffixedMethod_ReportsZero(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public Task WorkAsync() { return Task.CompletedTask; }
                                  }
                              }
                              """;

        var analyzer = new AsyncMethodNamingAnalyzer();
        var provider = new AsyncMethodNamingCodeFixProvider();
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
        var provider = new AsyncMethodNamingCodeFixProvider();
        var fixAllProvider = provider.GetFixAllProvider();

        await Assert.That(fixAllProvider).IsEqualTo(WellKnownFixAllProviders.BatchFixer);
    }
}
