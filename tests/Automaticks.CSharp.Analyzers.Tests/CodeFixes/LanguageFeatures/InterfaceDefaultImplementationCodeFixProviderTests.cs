using Automaticks.CSharp.CodeFixes.LanguageFeatures;
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
}
