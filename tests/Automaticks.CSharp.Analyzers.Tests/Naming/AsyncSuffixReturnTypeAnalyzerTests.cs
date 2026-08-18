using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Naming;

/// <summary>
///     Tests for AsyncSuffixReturnTypeAnalyzer.
/// </summary>
public class AsyncSuffixReturnTypeAnalyzerTests
{

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

        var analyzer = new AsyncSuffixReturnTypeAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS009")).IsFalse();
    }
    /// <summary>
    ///     Tests that Analyze_InterfaceVoidMethodWithAsyncSuffix_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_InterfaceVoidMethodWithAsyncSuffix_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo {
                                      void RequestAsync();
                                  }
                              }
                              """;

        var analyzer = new AsyncSuffixReturnTypeAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS009")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_SyncValueTypeMethodWithAsyncSuffix_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SyncValueTypeMethodWithAsyncSuffix_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int GetCountAsync() => 0;
                                  }
                              }
                              """;

        var analyzer = new AsyncSuffixReturnTypeAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS009")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_TaskMethodWithAsyncSuffix_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TaskMethodWithAsyncSuffix_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public Task DoWorkAsync() => Task.CompletedTask;
                                  }
                              }
                              """;

        var analyzer = new AsyncSuffixReturnTypeAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS009")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_TaskOfGenericTypeMethodWithAsyncSuffix_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TaskOfGenericTypeMethodWithAsyncSuffix_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public Task<int> GetValueAsync() => Task.FromResult(0);
                                  }
                              }
                              """;

        var analyzer = new AsyncSuffixReturnTypeAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS009")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ValueTaskMethodWithAsyncSuffix_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ValueTaskMethodWithAsyncSuffix_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public ValueTask ProcessAsync() => ValueTask.CompletedTask;
                                  }
                              }
                              """;

        var analyzer = new AsyncSuffixReturnTypeAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS009")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ValueTaskOfGenericTypeMethodWithAsyncSuffix_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ValueTaskOfGenericTypeMethodWithAsyncSuffix_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public ValueTask<string> FetchAsync() => ValueTask.FromResult(string.Empty);
                                  }
                              }
                              """;

        var analyzer = new AsyncSuffixReturnTypeAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS009")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_VoidMethodWithAsyncSuffix_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_VoidMethodWithAsyncSuffix_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void RequestAsync() {}
                                  }
                              }
                              """;

        var analyzer = new AsyncSuffixReturnTypeAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS009")).IsTrue();
    }
}
