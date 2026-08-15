using Automaticks.CSharp.CodeFixes.LanguageFeatures;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes.LanguageFeatures;

/// <summary>
///     Tests for RedundantNullCheckCodeFixProvider.
/// </summary>
public class RedundantNullCheckCodeFixProviderTests
{
    /// <summary>
    ///     Tests that a coalesce throw collapses to the operand.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_CoalesceThrow_KeepsAssignedValue(CancellationToken cancellationToken)
    {
        const string source = """
                              #nullable enable
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      private string name;

                                      public void Bar(string value) {
                                          name = value ?? throw new ArgumentNullException(nameof(value));
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantNullCheckAnalyzer();
        var provider = new RedundantNullCheckCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("name = value;");
        await Assert.That(fixedSource).DoesNotContain("ArgumentNullException");
    }

    /// <summary>
    ///     Tests that a ThrowIfNull call inside an expression bodied member is left untouched.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_ExpressionBodiedThrowIfNull_KeepsSourceUnchanged(CancellationToken cancellationToken)
    {
        const string source = """
                              #nullable enable
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(string value) => ArgumentNullException.ThrowIfNull(value);
                                  }
                              }
                              """;

        var analyzer = new RedundantNullCheckAnalyzer();
        var provider = new RedundantNullCheckCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("ArgumentNullException.ThrowIfNull(value);");
    }

    /// <summary>
    ///     Tests that an if guard statement is removed.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_IfThrowGuard_RemovesStatement(CancellationToken cancellationToken)
    {
        const string source = """
                              #nullable enable
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(string value) {
                                          if (value == null)
                                          {
                                              throw new ArgumentNullException(nameof(value));
                                          }

                                          System.Console.WriteLine(value);
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantNullCheckAnalyzer();
        var provider = new RedundantNullCheckCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).DoesNotContain("ArgumentNullException");
        await Assert.That(fixedSource).Contains("System.Console.WriteLine(value);");
    }

    /// <summary>
    ///     Tests that a nullable parameter guard is never offered a fix.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountFixable_NullableParameter_ReportsZero(CancellationToken cancellationToken)
    {
        const string source = """
                              #nullable enable
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(string? value) {
                                          if (value == null)
                                          {
                                              throw new ArgumentNullException(nameof(value));
                                          }
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantNullCheckAnalyzer();
        var provider = new RedundantNullCheckCodeFixProvider();
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
        var provider = new RedundantNullCheckCodeFixProvider();
        var fixAllProvider = provider.GetFixAllProvider();

        await Assert.That(fixAllProvider).IsEqualTo(WellKnownFixAllProviders.BatchFixer);
    }
}
