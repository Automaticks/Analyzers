using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Threading.Tasks.Analyzers.Tests;

/// <summary>
///     Tests for UnobservedTaskAnalyzer.
/// </summary>
public class UnobservedTaskAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_DiscardAssignmentAsSubExpression_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_DiscardAssignmentAsSubExpression_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public Task DoWorkAsync() => Task.CompletedTask;
                                      public void Caller() {
                                          if ((_ = DoWorkAsync()) != null) { }
                                      }
                                  }
                              }
                              """;

        var analyzer = new UnobservedTaskAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA010")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_GenericMethodReturningTypeParameter_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_GenericMethodReturningTypeParameter_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public T GetValue<T>() => default(T);
                                      public void Caller<TResult>() { GetValue<TResult>(); }
                                  }
                              }
                              """;

        var analyzer = new UnobservedTaskAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA010")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_NonTaskGenericReturnType_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NonTaskGenericReturnType_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Foo {
                                      public List<int> GetListAsync() => new List<int>();
                                      public void Caller() { GetListAsync(); }
                                  }
                              }
                              """;

        var analyzer = new UnobservedTaskAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA010")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_TaskMethodAssignedToExistingVariable_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TaskMethodAssignedToExistingVariable_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public Task DoWorkAsync() => Task.CompletedTask;
                                      public void Caller() {
                                          Task t = Task.CompletedTask;
                                          t = DoWorkAsync();
                                      }
                                  }
                              }
                              """;

        var analyzer = new UnobservedTaskAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA010")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_TaskMethodAssignedToVariable_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TaskMethodAssignedToVariable_ReportsNoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new UnobservedTaskAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA010")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_TaskMethodAwaited_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TaskMethodAwaited_ReportsNoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new UnobservedTaskAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA010")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_TaskMethodPassedAsArgument_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TaskMethodPassedAsArgument_ReportsNoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new UnobservedTaskAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA010")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_TaskMethodReturned_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TaskMethodReturned_ReportsNoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new UnobservedTaskAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA010")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_TaskMethodWithDiscardAssignment_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TaskMethodWithDiscardAssignment_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new UnobservedTaskAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA010")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_TaskOfGenericTypeReturningMethodCalledAsStatement_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TaskOfGenericTypeReturningMethodCalledAsStatement_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new UnobservedTaskAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA010")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_TaskReturningMethodCalledAsStatement_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TaskReturningMethodCalledAsStatement_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new UnobservedTaskAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA010")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_UndeclaredMethodInvocation_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_UndeclaredMethodInvocation_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Caller() { Undeclared(); }
                                  }
                              }
                              """;

        var analyzer = new UnobservedTaskAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA010")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ValueTaskOfGenericTypeReturningMethodCalledAsStatement_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ValueTaskOfGenericTypeReturningMethodCalledAsStatement_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new UnobservedTaskAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA010")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ValueTaskReturningMethodCalledAsStatement_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ValueTaskReturningMethodCalledAsStatement_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new UnobservedTaskAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA010")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_VoidMethodCalledAsStatement_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_VoidMethodCalledAsStatement_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void DoWork() { }
                                      public void Caller() { DoWork(); }
                                  }
                              }
                              """;

        var analyzer = new UnobservedTaskAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA010")).IsFalse();
    }
}
