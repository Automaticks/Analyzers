using Automaticks.CSharp.CodeFixes.Formatting;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes.Formatting;

/// <summary>
///     Tests for ObjectInitializerEmptyBracesCodeFixProvider.
/// </summary>
public class ObjectInitializerEmptyBracesCodeFixProviderTests
{
    /// <summary>
    ///     Tests that an existing argument list is preserved.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_CreationWithArguments_KeepsArguments(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Widget {
                                      public Widget(int size) { }
                                  }

                                  public class Foo {
                                      public void Bar() {
                                          var widget = new Widget(3) { };
                                      }
                                  }
                              }
                              """;

        var analyzer = new ObjectInitializerCodeStyleAnalyzer();
        var provider = new ObjectInitializerEmptyBracesCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("new Widget(3)");
        await Assert.That(fixedSource).DoesNotContain("new Widget(3) { }");
    }

    /// <summary>
    ///     Tests that a creation without arguments gains an empty argument list.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_CreationWithoutArguments_AddsEmptyArgumentList(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Widget { }

                                  public class Foo {
                                      public void Bar() {
                                          var widget = new Widget { };
                                      }
                                  }
                              }
                              """;

        var analyzer = new ObjectInitializerCodeStyleAnalyzer();
        var provider = new ObjectInitializerEmptyBracesCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("new Widget()");
    }

    /// <summary>
    ///     Tests that the fixed source no longer reports the rule.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_EmptyInitializer_LeavesNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Widget { }

                                  public class Foo {
                                      public void Bar() {
                                          var widget = new Widget { };
                                      }
                                  }
                              }
                              """;

        var analyzer = new ObjectInitializerCodeStyleAnalyzer();
        var provider = new ObjectInitializerEmptyBracesCodeFixProvider();
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
    ///     Tests that a populated initializer is never offered this fix.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountFixable_PopulatedInitializer_ReportsZero(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Widget {
                                      public int Size { get; set; }
                                  }

                                  public class Foo {
                                      public void Bar() {
                                          var widget = new Widget
                                          {
                                              Size = 3
                                          };
                                      }
                                  }
                              }
                              """;

        var analyzer = new ObjectInitializerCodeStyleAnalyzer();
        var provider = new ObjectInitializerEmptyBracesCodeFixProvider();
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
    ///     Tests that an array creation is deliberately left alone.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountOfferedActions_EmptyArrayInitializer_OffersNoFix(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() {
                                          var values = new int[] { };
                                      }
                                  }
                              }
                              """;

        var analyzer = new ObjectInitializerCodeStyleAnalyzer();
        var provider = new ObjectInitializerEmptyBracesCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var offered = await CodeFixTestRunner.CountOfferedActionsAsync(request, cancellationToken);

        await Assert.That(offered).IsEqualTo(0);
    }
}
