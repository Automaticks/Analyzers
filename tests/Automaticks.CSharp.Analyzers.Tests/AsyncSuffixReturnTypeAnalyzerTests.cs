using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class AsyncSuffixReturnTypeAnalyzerTests
{
    [Test]
    public async Task Analyze_InterfaceVoidMethodWithAsyncSuffix_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo {
                                      void RequestAsync();
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AsyncSuffixReturnTypeAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS009")).IsTrue();
    }

    [Test]
    public async Task Analyze_SyncValueTypeMethodWithAsyncSuffix_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int GetCountAsync() => 0;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AsyncSuffixReturnTypeAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS009")).IsTrue();
    }

    [Test]
    public async Task Analyze_TaskMethodWithAsyncSuffix_ReportsNoDiagnostic()
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public Task DoWorkAsync() => Task.CompletedTask;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AsyncSuffixReturnTypeAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS009")).IsFalse();
    }

    [Test]
    public async Task Analyze_TaskOfTMethodWithAsyncSuffix_ReportsNoDiagnostic()
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public Task<int> GetValueAsync() => Task.FromResult(0);
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AsyncSuffixReturnTypeAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS009")).IsFalse();
    }

    [Test]
    public async Task Analyze_ValueTaskMethodWithAsyncSuffix_ReportsNoDiagnostic()
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public ValueTask ProcessAsync() => ValueTask.CompletedTask;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AsyncSuffixReturnTypeAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS009")).IsFalse();
    }

    [Test]
    public async Task Analyze_ValueTaskOfTMethodWithAsyncSuffix_ReportsNoDiagnostic()
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public ValueTask<string> FetchAsync() => ValueTask.FromResult(string.Empty);
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AsyncSuffixReturnTypeAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS009")).IsFalse();
    }

    [Test]
    public async Task Analyze_VoidMethodWithAsyncSuffix_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void RequestAsync() {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AsyncSuffixReturnTypeAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS009")).IsTrue();
    }

    [Test]
    public async Task Analyze_IAsyncEnumerableMethodWithAsyncSuffix_ReportsNoDiagnostic()
    {
        const string source = """
                              using System.Collections.Generic;
                              using System.Threading;
                              namespace MyApp {
                                  public class Foo {
                                      public IAsyncEnumerable<int> GetItemsAsync(CancellationToken ct) => throw new System.NotImplementedException();
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AsyncSuffixReturnTypeAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS009")).IsFalse();
    }
}
