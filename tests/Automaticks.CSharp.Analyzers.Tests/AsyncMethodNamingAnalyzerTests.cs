using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class AsyncMethodNamingAnalyzerTests
{
    [Test]
    public async Task Analyze_PrivateTaskMethodWithoutAsyncSuffix_ReportsDiagnostic()
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      private Task DoWork() => Task.CompletedTask;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AsyncMethodNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS003")).IsTrue();
    }

    [Test]
    public async Task Analyze_ProtectedTaskMethodWithoutAsyncSuffix_ReportsDiagnostic()
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      protected Task DoWork() => Task.CompletedTask;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AsyncMethodNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS003")).IsTrue();
    }

    [Test]
    public async Task Analyze_PublicTaskMethodWithAsyncSuffix_ReportsNoDiagnostic()
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public Task DoWorkAsync() => Task.CompletedTask;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AsyncMethodNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS003")).IsFalse();
    }

    [Test]
    public async Task Analyze_PublicTaskMethodWithoutAsyncSuffix_ReportsDiagnostic()
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public Task DoWork() => Task.CompletedTask;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AsyncMethodNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS003")).IsTrue();
    }

    [Test]
    public async Task Analyze_PublicTaskOfTMethodWithoutAsyncSuffix_ReportsDiagnostic()
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public Task<int> GetValue() => Task.FromResult(0);
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AsyncMethodNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS003")).IsTrue();
    }

    [Test]
    public async Task Analyze_IAsyncEnumerableMethodWithoutAsyncSuffix_ReportsDiagnostic()
    {
        const string source = """
                              using System.Collections.Generic;
                              using System.Threading;
                              namespace MyApp {
                                  public class Foo {
                                      public IAsyncEnumerable<int> GetItems(CancellationToken ct) => throw new System.NotImplementedException();
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AsyncMethodNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS003")).IsTrue();
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AsyncMethodNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS003")).IsFalse();
    }

    [Test]
    public async Task Analyze_StaticMain_ReportsNoDiagnostic()
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Program {
                                      public static async Task Main(string[] args) { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AsyncMethodNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS003")).IsFalse();
    }

    [Test]
    public async Task Analyze_ExternalOverride_ReportsNoDiagnostic()
    {
        const string externalSource = """
                                      using System.Threading.Tasks;
                                      public abstract class Base {
                                          public abstract Task DoWork();
                                      }
                                      """;

        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Derived : Base {
                                      public override Task DoWork() => Task.CompletedTask;
                                  }
                              }
                              """;

        var externalRef = AnalyzerTestRunner.CompileToReference(externalSource);
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AsyncMethodNamingAnalyzer(), source, [externalRef]);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS003")).IsFalse();
    }

    [Test]
    public async Task Analyze_ExplicitExternalInterfaceImplementation_ReportsNoDiagnostic()
    {
        const string externalSource = """
                                      using System.Threading.Tasks;
                                      public interface IWorker {
                                          Task DoWork();
                                      }
                                      """;

        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Worker : IWorker {
                                      Task IWorker.DoWork() => Task.CompletedTask;
                                  }
                              }
                              """;

        var externalRef = AnalyzerTestRunner.CompileToReference(externalSource);
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AsyncMethodNamingAnalyzer(), source, [externalRef]);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS003")).IsFalse();
    }

    [Test]
    public async Task Analyze_ImplicitExternalInterfaceImplementation_ReportsNoDiagnostic()
    {
        const string externalSource = """
                                      using System.Threading.Tasks;
                                      public interface IWorker {
                                          Task DoWork();
                                      }
                                      """;

        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Worker : IWorker {
                                      public Task DoWork() => Task.CompletedTask;
                                  }
                              }
                              """;

        var externalRef = AnalyzerTestRunner.CompileToReference(externalSource);
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AsyncMethodNamingAnalyzer(), source, [externalRef]);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS003")).IsFalse();
    }

    [Test]
    public async Task Analyze_TestMethodWithFactAttribute_ReportsNoDiagnostic()
    {
        const string externalSource = """
                                      namespace TUnit {
                                          [System.AttributeUsage(System.AttributeTargets.Method)]
                                          public sealed class FactAttribute : System.Attribute { }
                                      }
                                      """;

        const string source = """
                              using System.Threading.Tasks;
                              using TUnit;
                              namespace MyApp {
                                  public class MyTests {
                                      [Fact]
                                      public async Task VerifyBehavior() { }
                                  }
                              }
                              """;

        var externalRef = AnalyzerTestRunner.CompileToReference(externalSource);
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AsyncMethodNamingAnalyzer(), source, [externalRef]);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS003")).IsFalse();
    }

    [Test]
    public async Task Analyze_TestMethodWithTestAttribute_ReportsNoDiagnostic()
    {
        const string externalSource = """
                                      namespace TUnit {
                                          [System.AttributeUsage(System.AttributeTargets.Method)]
                                          public sealed class TestAttribute : System.Attribute { }
                                      }
                                      """;

        const string source = """
                              using System.Threading.Tasks;
                              using TUnit;
                              namespace MyApp {
                                  public class MyTests {
                                      [Test]
                                      public async Task VerifyBehavior() { }
                                  }
                              }
                              """;

        var externalRef = AnalyzerTestRunner.CompileToReference(externalSource);
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AsyncMethodNamingAnalyzer(), source, [externalRef]);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS003")).IsFalse();
    }
}
