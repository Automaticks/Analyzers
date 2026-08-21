using Automaticks.CSharp.Naming;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Naming;

/// <summary>
///     Tests AsyncSuffixReturnTypeAnalyzer against return types that only look awaitable.
/// </summary>
public class AsyncSuffixReturnTypeAnalyzerReturnShapesTests
{
    /// <summary>
    ///     Tests that an array return type is not read as a task.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ArrayReturnType_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Shape {
                                      public int[] DrawAsync() { return new int[0]; }
                                  }
                              }
                              """;

        var analyzer = new AsyncSuffixReturnTypeAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.AsyncSuffixReturnType)).IsTrue();
    }

    /// <summary>
    ///     Tests that a look-alike declared outside the framework namespace is not read as a task.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ForeignAsyncEnumerable_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface IAsyncEnumerable<T> { }
                                  public class Shape {
                                      public IAsyncEnumerable<int> DrawAsync() { return null!; }
                                  }
                              }
                              """;

        var analyzer = new AsyncSuffixReturnTypeAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.AsyncSuffixReturnType)).IsTrue();
    }

    /// <summary>
    ///     Tests that a real async enumerable is accepted as an awaitable return type.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FrameworkAsyncEnumerable_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Shape {
                                      public IAsyncEnumerable<int> DrawAsync() { return null!; }
                                  }
                              }
                              """;

        var analyzer = new AsyncSuffixReturnTypeAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.AsyncSuffixReturnType)).IsFalse();
    }

    /// <summary>
    ///     Tests that a generic type unrelated to tasks is reported.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_GenericNonTaskReturnType_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Shape {
                                      public List<int> DrawAsync() { return null!; }
                                  }
                              }
                              """;

        var analyzer = new AsyncSuffixReturnTypeAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.AsyncSuffixReturnType)).IsTrue();
    }

    /// <summary>
    ///     Tests that a look-alike with the wrong arity is not read as a task.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TwoArgumentAsyncEnumerable_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace System.Collections.Generic {
                                  public interface IAsyncEnumerable<TKey, TValue> { }
                              }
                              namespace MyApp {
                                  public class Shape {
                                      public System.Collections.Generic.IAsyncEnumerable<int, int> DrawAsync() { return null!; }
                                  }
                              }
                              """;

        var analyzer = new AsyncSuffixReturnTypeAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.AsyncSuffixReturnType)).IsTrue();
    }
}
