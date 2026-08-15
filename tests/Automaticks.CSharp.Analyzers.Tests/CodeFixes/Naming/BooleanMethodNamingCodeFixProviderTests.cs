using Automaticks.CSharp.CodeFixes.Naming;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes.Naming;

/// <summary>
///     Tests for BooleanMethodNamingCodeFixProvider.
/// </summary>
public class BooleanMethodNamingCodeFixProviderTests
{
    /// <summary>
    ///     Tests that the renamed method no longer reports the rule.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_BooleanMethod_LeavesNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool Validate() { return true; }
                                  }
                              }
                              """;

        var analyzer = new BooleanMethodNamingAnalyzer();
        var provider = new BooleanMethodNamingCodeFixProvider();
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
    ///     Tests that the method is renamed and call sites follow.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_BooleanMethod_RenamesCallSite(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool Validate() { return true; }

                                      public bool HasResult() { return Validate(); }
                                  }
                              }
                              """;

        var analyzer = new BooleanMethodNamingAnalyzer();
        var provider = new BooleanMethodNamingCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("public bool CanValidate()");
        await Assert.That(fixedSource).Contains("return CanValidate();");
    }

    /// <summary>
    ///     Tests that an allowed prefix is never offered a fix.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountFixable_AllowedPrefix_ReportsZero(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool CanValidate() { return true; }
                                  }
                              }
                              """;

        var analyzer = new BooleanMethodNamingAnalyzer();
        var provider = new BooleanMethodNamingCodeFixProvider();
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
