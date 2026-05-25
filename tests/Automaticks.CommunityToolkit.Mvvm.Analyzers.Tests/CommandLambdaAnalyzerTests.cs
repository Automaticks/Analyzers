using Automaticks.CommunityToolkit.Mvvm;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CommunityToolkit.Mvvm.Analyzers.Tests;

public class CommandLambdaAnalyzerTests
{
    [Test]
    public async Task Analyze_AsyncRelayCommandWithLambda_ReportsDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CommandLambdaAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXMV001")).IsTrue();
    }

    [Test]
    public async Task Analyze_AsyncRelayCommandWithMethodGroup_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CommandLambdaAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXMV001")).IsFalse();
    }

    [Test]
    public async Task Analyze_RelayCommandWithLambda_ReportsDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CommandLambdaAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXMV001")).IsTrue();
    }

    [Test]
    public async Task Analyze_RelayCommandWithMethodGroup_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CommandLambdaAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXMV001")).IsFalse();
    }
}
