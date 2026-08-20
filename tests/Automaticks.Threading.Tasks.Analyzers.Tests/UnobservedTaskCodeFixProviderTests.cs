using Automaticks.Threading.Tasks.CodeFixes;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Threading.Tasks.Analyzers.Tests;

/// <summary>Tests for UnobservedTaskCodeFixProvider.</summary>
public class UnobservedTaskCodeFixProviderTests
{
    /// <summary>Verifies repeated application awaits every discarded invocation.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyAllFixes_MultipleDiscardedCalls_AwaitsEveryCall(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public async Task RunAsync() {
                                          WorkAsync();
                                          WorkAsync();
                                      }

                                      public Task WorkAsync() { return Task.CompletedTask; }
                                  }
                              }
                              """;

        var analyzer = new UnobservedTaskAnalyzer();
        var provider = new UnobservedTaskCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyAllFixesAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("await WorkAsync();");
        await Assert.That(fixedSource).DoesNotContain("\n            WorkAsync();");
    }

    /// <summary>Verifies an existing discard assignment is replaced by a real await.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_DiscardAssignment_ReplacesWithAwait(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public async Task RunAsync() {
                                          _ = WorkAsync();
                                      }

                                      public Task WorkAsync() { return Task.CompletedTask; }
                                  }
                              }
                              """;

        var analyzer = new UnobservedTaskAnalyzer();
        var provider = new UnobservedTaskCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("await WorkAsync();");
        await Assert.That(fixedSource).DoesNotContain("_ = WorkAsync();");
    }

    /// <summary>Verifies a discarded call inside an async local function is awaited.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_DiscardedCallInsideAsyncLocalFunction_InsertsAwait(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public void Run() {
                                          async Task LocalAsync() {
                                              WorkAsync();
                                          }
                                      }

                                      public Task WorkAsync() { return Task.CompletedTask; }
                                  }
                              }
                              """;

        var analyzer = new UnobservedTaskAnalyzer();
        var provider = new UnobservedTaskCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("await WorkAsync();");
    }

    /// <summary>Verifies a discarded call inside an async lambda is awaited.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_InsideAsyncLambda_InsertsAwait(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public void Run() {
                                          Func<Task> handler = async () => {
                                              WorkAsync();
                                          };
                                      }

                                      public Task WorkAsync() { return Task.CompletedTask; }
                                  }
                              }
                              """;

        var analyzer = new UnobservedTaskAnalyzer();
        var provider = new UnobservedTaskCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("await WorkAsync();");
    }

    /// <summary>Verifies a discarded ValueTask call is awaited.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_ValueTaskCall_InsertsAwait(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public async Task RunAsync() {
                                          WorkAsync();
                                      }

                                      public ValueTask WorkAsync() { return default; }
                                  }
                              }
                              """;

        var analyzer = new UnobservedTaskAnalyzer();
        var provider = new UnobservedTaskCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("await WorkAsync();");
    }

    /// <summary>Verifies no fix is offered when the diagnostic location has no invocation ancestor.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountOfferedActions_DiagnosticAtNodeWithNoInvocationAncestor_OffersNoFix(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                  }
                              }
                              """;

        var analyzer = new UnobservedTaskAtClassDeclarationAnalyzer();
        var provider = new UnobservedTaskCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var offered = await CodeFixTestRunner.CountOfferedActionsAsync(request, cancellationToken);

        await Assert.That(offered).IsEqualTo(0);
    }

    /// <summary>Verifies no fix is offered when the discarded call is inside a constructor.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountOfferedActions_DiscardedCallInsideConstructor_OffersNoFix(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public Foo() {
                                          WorkAsync();
                                      }

                                      public Task WorkAsync() { return Task.CompletedTask; }
                                  }
                              }
                              """;

        var analyzer = new UnobservedTaskAnalyzer();
        var provider = new UnobservedTaskCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var offered = await CodeFixTestRunner.CountOfferedActionsAsync(request, cancellationToken);

        await Assert.That(offered).IsEqualTo(0);
    }

    /// <summary>Verifies no fix is offered when the invocation has no enclosing expression statement.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountOfferedActions_InvocationHasNoEnclosingStatement_OffersNoFix(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public async Task RunAsync() {
                                          var work = WorkAsync();
                                      }

                                      public Task WorkAsync() { return Task.CompletedTask; }
                                  }
                              }
                              """;

        var analyzer = new UnobservedTaskAtLocalDeclarationAnalyzer();
        var provider = new UnobservedTaskCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var offered = await CodeFixTestRunner.CountOfferedActionsAsync(request, cancellationToken);

        await Assert.That(offered).IsEqualTo(0);
    }

    /// <summary>Verifies no fix is offered when the enclosing method is not async.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountOfferedActions_NonAsyncEnclosingMethod_OffersNoFix(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public void Run() {
                                          WorkAsync();
                                      }

                                      public Task WorkAsync() { return Task.CompletedTask; }
                                  }
                              }
                              """;

        var analyzer = new UnobservedTaskAnalyzer();
        var provider = new UnobservedTaskCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var offered = await CodeFixTestRunner.CountOfferedActionsAsync(request, cancellationToken);

        await Assert.That(offered).IsEqualTo(0);
    }

    /// <summary>Verifies GetFixAllProvider returns the batch fixer.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetFixAllProvider_Called_ReturnsBatchFixer(CancellationToken cancellationToken)
    {
        var provider = new UnobservedTaskCodeFixProvider();
        var fixAllProvider = provider.GetFixAllProvider();

        await Assert.That(fixAllProvider).IsNotNull();
    }
}
