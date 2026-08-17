using Automaticks.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Threading.Tasks.Analyzers.Tests;

/// <summary>
///     Tests for UnusedCancellationTokenAnalyzer.
/// </summary>
public class UnusedCancellationTokenAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_AbstractMethodWithToken_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AbstractMethodWithToken_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading;
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public abstract class Foo {
                                      public abstract Task BarAsync(CancellationToken token);
                                  }
                              }
                              """;

        var analyzer = new UnusedCancellationTokenAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA011")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExplicitInterfaceImplementationIgnoringToken_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExplicitInterfaceImplementationIgnoringToken_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading;
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public interface IWorker { Task WorkAsync(CancellationToken token); }
                                  public class Foo : IWorker {
                                      Task IWorker.WorkAsync(CancellationToken token) { return Task.CompletedTask; }
                                  }
                              }
                              """;

        var analyzer = new UnusedCancellationTokenAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA011")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExpressionBodiedMethodForwardingToken_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExpressionBodiedMethodForwardingToken_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading;
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public Task BarAsync(CancellationToken token) => Task.Delay(1, token);
                                  }
                              }
                              """;

        var analyzer = new UnusedCancellationTokenAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA011")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_FactAttributedMethodIgnoringToken_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FactAttributedMethodIgnoringToken_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              using System.Threading;
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public sealed class FactAttribute : Attribute { }
                                  public class FooTests {
                                      [FactAttribute]
                                      public Task Bar_Something_Works(CancellationToken token) { return Task.CompletedTask; }
                                  }
                              }
                              """;

        var analyzer = new UnusedCancellationTokenAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA011")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_InterfaceImplementationIgnoringToken_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_InterfaceImplementationIgnoringToken_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading;
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public interface IWorker { Task WorkAsync(CancellationToken token); }
                                  public class Foo : IWorker {
                                      public Task WorkAsync(CancellationToken token) { return Task.CompletedTask; }
                                  }
                              }
                              """;

        var analyzer = new UnusedCancellationTokenAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA011")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_LocalFunctionIgnoringToken_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LocalFunctionIgnoringToken_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading;
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() {
                                          Task DropAsync(CancellationToken token) { return Task.CompletedTask; }
                                          DropAsync(CancellationToken.None);
                                      }
                                  }
                              }
                              """;

        var analyzer = new UnusedCancellationTokenAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA011")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_LocalFunctionWithExpressionBodyIgnoringToken_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LocalFunctionWithExpressionBodyIgnoringToken_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading;
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() {
                                          Task DropAsync(CancellationToken token) => Task.CompletedTask;
                                          DropAsync(CancellationToken.None);
                                      }
                                  }
                              }
                              """;

        var analyzer = new UnusedCancellationTokenAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA011")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_MethodIgnoringToken_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodIgnoringToken_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading;
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public Task BarAsync(CancellationToken token) { return Task.CompletedTask; }
                                  }
                              }
                              """;

        var analyzer = new UnusedCancellationTokenAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA011")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_MethodThrowingIfCancellationRequested_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodThrowingIfCancellationRequested_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading;
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public Task BarAsync(CancellationToken token) {
                                          token.ThrowIfCancellationRequested();
                                          return Task.CompletedTask;
                                      }
                                  }
                              }
                              """;

        var analyzer = new UnusedCancellationTokenAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA011")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_MethodWithoutToken_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodWithoutToken_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public Task BarAsync(int value) { return Task.CompletedTask; }
                                  }
                              }
                              """;

        var analyzer = new UnusedCancellationTokenAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA011")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_OverrideMethodIgnoringToken_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_OverrideMethodIgnoringToken_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading;
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public abstract class Base { public abstract Task WorkAsync(CancellationToken token); }
                                  public class Foo : Base {
                                      public override Task WorkAsync(CancellationToken token) { return Task.CompletedTask; }
                                  }
                              }
                              """;

        var analyzer = new UnusedCancellationTokenAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA011")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_TestAttributedMethodIgnoringToken_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TestAttributedMethodIgnoringToken_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              using System.Threading;
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public sealed class TestAttribute : Attribute { }
                                  public class FooTests {
                                      [Test]
                                      public Task Bar_Something_Works(CancellationToken token) { return Task.CompletedTask; }
                                  }
                              }
                              """;

        var analyzer = new UnusedCancellationTokenAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA011")).IsFalse();
    }
}
