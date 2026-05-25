using Automaticks.Threading.Tasks;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.Threading.Tasks.Analyzers.Tests;

public class UnobservedTaskAnalyzerTests
{
    [Test]
    public async Task Analyze_TaskMethodAssignedToVariable_ReportsNoDiagnostic()
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public Task DoWorkAsync() => Task.CompletedTask;
                                      public void Caller() { var t = DoWorkAsync(); }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new UnobservedTaskAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXTA010")).IsFalse();
    }

    [Test]
    public async Task Analyze_TaskMethodAwaited_ReportsNoDiagnostic()
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public Task DoWorkAsync() => Task.CompletedTask;
                                      public async Task Caller() { await DoWorkAsync(); }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new UnobservedTaskAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXTA010")).IsFalse();
    }

    [Test]
    public async Task Analyze_TaskMethodPassedAsArgument_ReportsNoDiagnostic()
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public Task DoWorkAsync() => Task.CompletedTask;
                                      public void Accept(Task t) { }
                                      public void Caller() { Accept(DoWorkAsync()); }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new UnobservedTaskAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXTA010")).IsFalse();
    }

    [Test]
    public async Task Analyze_TaskMethodReturned_ReportsNoDiagnostic()
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public Task DoWorkAsync() => Task.CompletedTask;
                                      public Task Caller() { return DoWorkAsync(); }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new UnobservedTaskAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXTA010")).IsFalse();
    }

    [Test]
    public async Task Analyze_TaskMethodWithDiscardAssignment_ReportsDiagnostic()
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public Task DoWorkAsync() => Task.CompletedTask;
                                      public void Caller() { _ = DoWorkAsync(); }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new UnobservedTaskAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXTA010")).IsTrue();
    }

    [Test]
    public async Task Analyze_TaskOfTReturningMethodCalledAsStatement_ReportsDiagnostic()
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public Task<int> GetValueAsync() => Task.FromResult(0);
                                      public void Caller() { GetValueAsync(); }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new UnobservedTaskAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXTA010")).IsTrue();
    }

    [Test]
    public async Task Analyze_TaskReturningMethodCalledAsStatement_ReportsDiagnostic()
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public Task DoWorkAsync() => Task.CompletedTask;
                                      public void Caller() { DoWorkAsync(); }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new UnobservedTaskAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXTA010")).IsTrue();
    }

    [Test]
    public async Task Analyze_ValueTaskOfTReturningMethodCalledAsStatement_ReportsDiagnostic()
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public ValueTask<int> GetValueAsync() => ValueTask.FromResult(0);
                                      public void Caller() { GetValueAsync(); }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new UnobservedTaskAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXTA010")).IsTrue();
    }

    [Test]
    public async Task Analyze_ValueTaskReturningMethodCalledAsStatement_ReportsDiagnostic()
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public ValueTask DoWorkAsync() => ValueTask.CompletedTask;
                                      public void Caller() { DoWorkAsync(); }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new UnobservedTaskAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXTA010")).IsTrue();
    }

    [Test]
    public async Task Analyze_VoidMethodCalledAsStatement_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void DoWork() { }
                                      public void Caller() { DoWork(); }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new UnobservedTaskAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXTA010")).IsFalse();
    }
}
