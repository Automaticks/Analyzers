using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CommunityToolkit.Mvvm.Analyzers.Tests;

/// <summary>
///     Tests for CommandLambdaAnalyzer.
/// </summary>
public class CommandLambdaAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_AsyncRelayCommandWithLambda_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AsyncRelayCommandWithLambda_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading;
                              using System.Threading.Tasks;
                              namespace CommunityToolkit.Mvvm.Input {
                                  public class AsyncRelayCommand {
                                      public AsyncRelayCommand(System.Func<CancellationToken, Task> execute) {}
                                  }
                              }
                              namespace MyApp {
                                  public class Foo {
                                      private Task DoWorkAsync(CancellationToken ct) => Task.CompletedTask;
                                      public Foo() {
                                          var cmd = new CommunityToolkit.Mvvm.Input.AsyncRelayCommand(ct => DoWorkAsync(ct));
                                      }
                                  }
                              }
                              """;

        var analyzer = new CommandLambdaAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXMV001")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_AsyncRelayCommandWithMethodGroup_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AsyncRelayCommandWithMethodGroup_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading;
                              using System.Threading.Tasks;
                              namespace CommunityToolkit.Mvvm.Input {
                                  public class AsyncRelayCommand {
                                      public AsyncRelayCommand(System.Func<CancellationToken, Task> execute) {}
                                  }
                              }
                              namespace MyApp {
                                  public class Foo {
                                      private Task DoWorkAsync(CancellationToken ct) => Task.CompletedTask;
                                      public Foo() {
                                          var cmd = new CommunityToolkit.Mvvm.Input.AsyncRelayCommand(DoWorkAsync);
                                      }
                                  }
                              }
                              """;

        var analyzer = new CommandLambdaAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXMV001")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_RelayCommandWithLambda_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_RelayCommandWithLambda_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace CommunityToolkit.Mvvm.Input {
                                  public class RelayCommand {
                                      public RelayCommand(System.Action execute) {}
                                  }
                              }
                              namespace MyApp {
                                  public class Foo {
                                      private void DoWork() {}
                                      public Foo() {
                                          var cmd = new CommunityToolkit.Mvvm.Input.RelayCommand(() => DoWork());
                                      }
                                  }
                              }
                              """;

        var analyzer = new CommandLambdaAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXMV001")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_RelayCommandWithMethodGroup_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_RelayCommandWithMethodGroup_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace CommunityToolkit.Mvvm.Input {
                                  public class RelayCommand {
                                      public RelayCommand(System.Action execute) {}
                                  }
                              }
                              namespace MyApp {
                                  public class Foo {
                                      private void DoWork() {}
                                      public Foo() {
                                          var cmd = new CommunityToolkit.Mvvm.Input.RelayCommand(DoWork);
                                      }
                                  }
                              }
                              """;

        var analyzer = new CommandLambdaAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXMV001")).IsFalse();
    }
}
