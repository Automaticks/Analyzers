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
    ///     Tests that Analyze_AliasedCancellationTokenIgnored_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AliasedCancellationTokenIgnored_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using Signal = System.Threading.CancellationToken;
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public Task BarAsync(Signal token) { return Task.CompletedTask; }
                                  }
                              }
                              """;

        var analyzer = new UnusedCancellationTokenAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA011")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ArglistParameter_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ArglistParameter_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(__arglist) { }
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
    ///     Tests that Analyze_LocalFunctionUsingToken_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LocalFunctionUsingToken_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading;
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() {
                                          Task WaitAsync(CancellationToken token) { return Task.Delay(1, token); }
                                          WaitAsync(CancellationToken.None);
                                      }
                                  }
                              }
                              """;

        var analyzer = new UnusedCancellationTokenAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA011")).IsFalse();
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
    ///     Tests that Analyze_LocalFunctionWithoutToken_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LocalFunctionWithoutToken_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() {
                                          Task WorkAsync(int value) { return Task.CompletedTask; }
                                          WorkAsync(1);
                                      }
                                  }
                              }
                              """;

        var analyzer = new UnusedCancellationTokenAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA011")).IsFalse();
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
    ///     Tests that Analyze_NonImplementingMethodInInterfaceType_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NonImplementingMethodInInterfaceType_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading;
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public interface IWorker { Task WorkAsync(CancellationToken token); }
                                  public class Foo : IWorker {
                                      public Task WorkAsync(CancellationToken token) { return Task.Delay(1, token); }
                                      public Task OtherAsync(CancellationToken token) { return Task.CompletedTask; }
                                  }
                              }
                              """;

        var analyzer = new UnusedCancellationTokenAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA011")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_NonTestAttributeOnMethodIgnoringToken_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NonTestAttributeOnMethodIgnoringToken_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              using System.Threading;
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      [Obsolete]
                                      public Task BarAsync(CancellationToken token) { return Task.CompletedTask; }
                                  }
                              }
                              """;

        var analyzer = new UnusedCancellationTokenAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA011")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_NullableTokenAlongsideOtherParameter_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NullableTokenAlongsideOtherParameter_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading;
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public Task BarAsync(int value, CancellationToken token) { return Task.FromResult(value); }
                                  }
                              }
                              """;

        var analyzer = new UnusedCancellationTokenAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA011")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_NullableTokenType_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NullableTokenType_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading;
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public Task BarAsync(CancellationToken? token) { return Task.CompletedTask; }
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
    ///     Tests that Analyze_QualifiedTokenTypeIgnored_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_QualifiedTokenTypeIgnored_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public Task BarAsync(System.Threading.CancellationToken token) { return Task.CompletedTask; }
                                  }
                              }
                              """;

        var analyzer = new UnusedCancellationTokenAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA011")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_SameNamedMethodBesideExplicitImplementation_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SameNamedMethodBesideExplicitImplementation_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading;
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public interface IWorker { Task RunAsync(CancellationToken token); }
                                  public class Foo : IWorker {
                                      Task IWorker.RunAsync(CancellationToken token) { return Task.Delay(1, token); }
                                      public Task RunAsync(CancellationToken token) { return Task.CompletedTask; }
                                  }
                              }
                              """;

        var analyzer = new UnusedCancellationTokenAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA011")).IsTrue();
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

    /// <summary>
    ///     Tests that Analyze_TestMethodAttributedMethodIgnoringToken_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TestMethodAttributedMethodIgnoringToken_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              using System.Threading;
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public sealed class TestMethodAttribute : Attribute { }
                                  public sealed class TheoryAttribute : Attribute { }
                                  public class FooTests {
                                      [TestMethod]
                                      public Task One_Something_Works(CancellationToken token) { return Task.CompletedTask; }
                                      [Theory]
                                      public Task Two_Something_Works(CancellationToken token) { return Task.CompletedTask; }
                                  }
                              }
                              """;

        var analyzer = new UnusedCancellationTokenAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA011")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_TokenBesideArglist_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TokenBesideArglist_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(CancellationToken token, __arglist) { }
                                  }
                              }
                              """;

        var analyzer = new UnusedCancellationTokenAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA011")).IsTrue();
    }
}
