using Automaticks.CSharp.CodeFixes.Formatting;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes.Formatting;

/// <summary>
///     Tests for ObjectInitializerFormatCodeFixProvider.
/// </summary>
public class ObjectInitializerFormatCodeFixProviderTests
{
    /// <summary>
    ///     Tests that a collection initializer is spread across lines.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_CollectionInitializer_LeavesNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() {
                                          var values = new List<int> { 1, 2 };
                                      }
                                  }
                              }
                              """;

        var analyzer = new ObjectInitializerCodeStyleAnalyzer();
        var provider = new ObjectInitializerFormatCodeFixProvider();
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
    ///     Tests that the reformatted initializer no longer reports.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_SingleLineInitializer_LeavesNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Widget {
                                      public int Size { get; set; }
                                      public int Weight { get; set; }
                                  }

                                  public class Foo {
                                      public void Bar() {
                                          var widget = new Widget { Size = 1, Weight = 2 };
                                      }
                                  }
                              }
                              """;

        var analyzer = new ObjectInitializerCodeStyleAnalyzer();
        var provider = new ObjectInitializerFormatCodeFixProvider();
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
    ///     Tests that every member ends up on its own line with the braces separated.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_SingleLineInitializer_SplitsMembersOntoOwnLines(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Widget {
                                      public int Size { get; set; }
                                      public int Weight { get; set; }
                                  }

                                  public class Foo {
                                      public void Bar() {
                                          var widget = new Widget { Size = 1, Weight = 2 };
                                      }
                                  }
                              }
                              """;

        var analyzer = new ObjectInitializerCodeStyleAnalyzer();
        var provider = new ObjectInitializerFormatCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyAllFixesAsync(request, cancellationToken);

        await Assert.That(fixedSource).DoesNotContain("new Widget { Size = 1, Weight = 2 }");
        await Assert.That(fixedSource).Contains("var widget = new Widget");
        await Assert.That(fixedSource).Contains("            {");
        await Assert.That(fixedSource).Contains("                Size = 1,");
        await Assert.That(fixedSource).Contains("                Weight = 2");
        await Assert.That(fixedSource).Contains("            };");
    }

    /// <summary>
    ///     Tests that a correctly formatted initializer is never offered a fix.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountFixable_WellFormattedInitializer_ReportsZero(CancellationToken cancellationToken)
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
                                              Size = 1
                                          };
                                      }
                                  }
                              }
                              """;

        var analyzer = new ObjectInitializerCodeStyleAnalyzer();
        var provider = new ObjectInitializerFormatCodeFixProvider();
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
        var provider = new ObjectInitializerFormatCodeFixProvider();
        var fixAllProvider = provider.GetFixAllProvider();

        await Assert.That(fixAllProvider).IsEqualTo(WellKnownFixAllProviders.BatchFixer);
    }
}
