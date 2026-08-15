using Automaticks.CSharp.CodeFixes.LanguageFeatures;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes.LanguageFeatures;

/// <summary>
///     Tests for InterfaceDefaultImplementationCodeFixProvider.
/// </summary>
public class InterfaceDefaultImplementationCodeFixProviderTests
{
    /// <summary>
    ///     Tests that an expression bodied property becomes a get only contract.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_ExpressionBodiedProperty_RemovesBody(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo {
                                      int Size => 3;
                                  }
                              }
                              """;

        var analyzer = new InterfaceDefaultImplementationAnalyzer();
        var provider = new InterfaceDefaultImplementationCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).DoesNotContain("=> 3");
        await Assert.That(fixedSource).Contains("get;");
    }

    /// <summary>
    ///     Tests that the fixed member no longer reports the rule.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_MethodWithBody_LeavesNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo {
                                      int Compute() { return 3; }
                                  }
                              }
                              """;

        var analyzer = new InterfaceDefaultImplementationAnalyzer();
        var provider = new InterfaceDefaultImplementationCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("int Compute();");
        await Assert.That(fixedSource).DoesNotContain("return 3;");
    }

    /// <summary>
    ///     Tests that a method body is replaced by a semicolon.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_MethodWithBody_RemovesBody(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo {
                                      int Compute() { return 3; }
                                  }
                              }
                              """;

        var analyzer = new InterfaceDefaultImplementationAnalyzer();
        var provider = new InterfaceDefaultImplementationCodeFixProvider();
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
    ///     Tests that a property with accessor bodies has each accessor body removed.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_PropertyWithAccessorBodies_RemovesBodies(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo {
                                      int Size
                                      {
                                          get { return 3; }
                                      }
                                  }
                              }
                              """;

        var analyzer = new InterfaceDefaultImplementationAnalyzer();
        var provider = new InterfaceDefaultImplementationCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).DoesNotContain("return 3;");
        await Assert.That(fixedSource).Contains("get;");
    }

    /// <summary>
    ///     Tests that a contract only interface is never offered a fix.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountFixable_ContractOnlyInterface_ReportsZero(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo {
                                      int Compute();

                                      int Size { get; }
                                  }
                              }
                              """;

        var analyzer = new InterfaceDefaultImplementationAnalyzer();
        var provider = new InterfaceDefaultImplementationCodeFixProvider();
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
    ///     Tests that an interface event with accessor bodies is never offered a fix.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountOfferedActions_InterfaceEventWithAccessorBodies_OffersNoFix(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo {
                                      event System.EventHandler Changed
                                      {
                                          add { }
                                          remove { }
                                      }
                                  }
                              }
                              """;

        var analyzer = new InterfaceDefaultImplementationAnalyzer();
        var provider = new InterfaceDefaultImplementationCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var offered = await CodeFixTestRunner.CountOfferedActionsAsync(request, cancellationToken);

        await Assert.That(offered).IsEqualTo(0);
    }

    /// <summary>
    ///     Tests that the provider always exposes the batch Fix All provider.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetFixAllProvider_Always_ReturnsBatchFixer(CancellationToken cancellationToken)
    {
        var provider = new InterfaceDefaultImplementationCodeFixProvider();
        var fixAllProvider = provider.GetFixAllProvider();

        await Assert.That(fixAllProvider).IsEqualTo(WellKnownFixAllProviders.BatchFixer);
    }
}
