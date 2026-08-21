using Automaticks.Testing.Testability;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Testing.Analyzers.Tests.Testability;

/// <summary>
///     Tests for DebugConditionalCompilationAnalyzer.
/// </summary>
public class DebugConditionalCompilationAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_ElifDebug_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ElifDebug_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() {
                              #if FOO
                                          DoFoo();
                              #elif DEBUG
                                          DoDebug();
                              #endif
                                      }
                                      private void DoDebug() {}
                                      private void DoFoo() {}
                                  }
                              }
                              """;

        var analyzer = new DebugConditionalCompilationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(
            diagnostics,
            "ATXTST008",
            "#elif")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_IfDebug_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_IfDebug_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() {
                              #if DEBUG
                                          DoDebug();
                              #endif
                                      }
                                      private void DoDebug() {}
                                  }
                              }
                              """;

        var analyzer = new DebugConditionalCompilationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(
            diagnostics,
            "ATXTST008",
            "#if")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_IfDebugAndOtherSymbol_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_IfDebugAndOtherSymbol_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() {
                              #if DEBUG && FOO
                                          DoDebug();
                              #endif
                                      }
                                      private void DoDebug() {}
                                  }
                              }
                              """;

        var analyzer = new DebugConditionalCompilationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST008")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_IfNotDebug_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_IfNotDebug_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() {
                              #if !DEBUG
                                          DoRelease();
                              #endif
                                      }
                                      private void DoRelease() {}
                                  }
                              }
                              """;

        var analyzer = new DebugConditionalCompilationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST008")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_IfUnrelatedSymbol_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_IfUnrelatedSymbol_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() {
                              #if FOO
                                          DoFoo();
                              #endif
                                      }
                                      private void DoFoo() {}
                                  }
                              }
                              """;

        var analyzer = new DebugConditionalCompilationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST008")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_RegionDirective_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_RegionDirective_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                              #region Helpers
                                      public void Bar() {}
                              #endregion
                                  }
                              }
                              """;

        var analyzer = new DebugConditionalCompilationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST008")).IsFalse();
    }
}
