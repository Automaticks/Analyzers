using Automaticks.CSharp;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Naming;

/// <summary>
///     Tests for AsyncMethodNamingAnalyzer.
/// </summary>
public class AsyncMethodNamingAnalyzerTests
{

    /// <summary>
    ///     Tests that Analyze_ExplicitExternalInterfaceImplementation_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExplicitExternalInterfaceImplementation_ReportsNoDiagnostic(CancellationToken cancellationToken)
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
        var analyzer = new AsyncMethodNamingAnalyzer();
        var options = new AnalysisOptions
{
    AdditionalReferences = [externalRef]
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS003")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExternalOverride_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExternalOverride_ReportsNoDiagnostic(CancellationToken cancellationToken)
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
        var analyzer = new AsyncMethodNamingAnalyzer();
        var options = new AnalysisOptions
{
    AdditionalReferences = [externalRef]
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS003")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_IAsyncEnumerableMethodWithAsyncSuffix_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_IAsyncEnumerableMethodWithAsyncSuffix_ReportsNoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new AsyncMethodNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS003")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_IAsyncEnumerableMethodWithoutAsyncSuffix_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_IAsyncEnumerableMethodWithoutAsyncSuffix_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new AsyncMethodNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS003")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ImplicitExternalInterfaceImplementation_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ImplicitExternalInterfaceImplementation_ReportsNoDiagnostic(CancellationToken cancellationToken)
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
        var analyzer = new AsyncMethodNamingAnalyzer();
        var options = new AnalysisOptions
{
    AdditionalReferences = [externalRef]
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS003")).IsFalse();
    }
    /// <summary>
    ///     Tests that Analyze_PrivateTaskMethodWithoutAsyncSuffix_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PrivateTaskMethodWithoutAsyncSuffix_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      private Task DoWork() => Task.CompletedTask;
                                  }
                              }
                              """;

        var analyzer = new AsyncMethodNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS003")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ProtectedTaskMethodWithoutAsyncSuffix_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ProtectedTaskMethodWithoutAsyncSuffix_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      protected Task DoWork() => Task.CompletedTask;
                                  }
                              }
                              """;

        var analyzer = new AsyncMethodNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS003")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_PublicTaskMethodWithAsyncSuffix_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PublicTaskMethodWithAsyncSuffix_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public Task DoWorkAsync() => Task.CompletedTask;
                                  }
                              }
                              """;

        var analyzer = new AsyncMethodNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS003")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_PublicTaskMethodWithoutAsyncSuffix_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PublicTaskMethodWithoutAsyncSuffix_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public Task DoWork() => Task.CompletedTask;
                                  }
                              }
                              """;

        var analyzer = new AsyncMethodNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS003")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_PublicTaskOfGenericTypeMethodWithoutAsyncSuffix_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PublicTaskOfGenericTypeMethodWithoutAsyncSuffix_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public Task<int> GetValue() => Task.FromResult(0);
                                  }
                              }
                              """;

        var analyzer = new AsyncMethodNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS003")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_StaticMain_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StaticMain_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Program {
                                      public static async Task Main(string[] args) { }
                                  }
                              }
                              """;

        var analyzer = new AsyncMethodNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS003")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_TestMethodWithFactAttribute_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TestMethodWithFactAttribute_ReportsNoDiagnostic(CancellationToken cancellationToken)
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
        var analyzer = new AsyncMethodNamingAnalyzer();
        var options = new AnalysisOptions
{
    AdditionalReferences = [externalRef]
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS003")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_TestMethodWithTestAttribute_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TestMethodWithTestAttribute_ReportsNoDiagnostic(CancellationToken cancellationToken)
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
        var analyzer = new AsyncMethodNamingAnalyzer();
        var options = new AnalysisOptions
{
    AdditionalReferences = [externalRef]
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS003")).IsFalse();
    }
}
