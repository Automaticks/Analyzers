using Automaticks.Testing.Testability;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Testing.Analyzers.Tests.Testability;

/// <summary>
///     Tests for RedundantEnumSwitchDefaultAnalyzer.
/// </summary>
public class RedundantEnumSwitchDefaultAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_NonEnumSwitchStatement_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NonEnumSwitchStatement_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Bar(int x) {
                                          switch (x) {
                                              case 1: return 1;
                                              default: return 0;
                                          }
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantEnumSwitchDefaultAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST007")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_SwitchExpressionExhaustiveWithDiscard_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SwitchExpressionExhaustiveWithDiscard_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public enum Color { Red, Green, Blue }
                                  public class Foo {
                                      public int Bar(Color c) => c switch {
                                          Color.Red => 1,
                                          Color.Green => 2,
                                          Color.Blue => 3,
                                          _ => 0,
                                      };
                                  }
                              }
                              """;

        var analyzer = new RedundantEnumSwitchDefaultAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST007")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_SwitchExpressionNoDiscard_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SwitchExpressionNoDiscard_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public enum Color { Red, Green, Blue }
                                  public class Foo {
                                      public int Bar(Color c) => c switch {
                                          Color.Red => 1,
                                          Color.Green => 2,
                                          Color.Blue => 3,
                                      };
                                  }
                              }
                              """;

        var analyzer = new RedundantEnumSwitchDefaultAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST007")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_SwitchExpressionNonExhaustiveWithDiscard_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SwitchExpressionNonExhaustiveWithDiscard_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public enum Color { Red, Green, Blue }
                                  public class Foo {
                                      public int Bar(Color c) => c switch {
                                          Color.Red => 1,
                                          Color.Green => 2,
                                          _ => 0,
                                      };
                                  }
                              }
                              """;

        var analyzer = new RedundantEnumSwitchDefaultAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST007")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_SwitchStatementExhaustiveWithDefault_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SwitchStatementExhaustiveWithDefault_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public enum Color { Red, Green, Blue }
                                  public class Foo {
                                      public int Bar(Color c) {
                                          switch (c) {
                                              case Color.Red: return 1;
                                              case Color.Green: return 2;
                                              case Color.Blue: return 3;
                                              default: return 0;
                                          }
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantEnumSwitchDefaultAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(
            diagnostics,
            "ATXTST007",
            "'Color'")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_SwitchStatementExhaustiveWithPatternLabelAndDefault_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SwitchStatementExhaustiveWithPatternLabelAndDefault_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public enum Color { Red, Green, Blue }
                                  public class Foo {
                                      public int Bar(Color c) {
                                          switch (c) {
                                              case Color.Red when true: return 1;
                                              case Color.Green: return 2;
                                              case Color.Blue: return 3;
                                              default: return 0;
                                          }
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantEnumSwitchDefaultAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST007")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_SwitchStatementNoDefault_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SwitchStatementNoDefault_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public enum Color { Red, Green, Blue }
                                  public class Foo {
                                      public void Bar(Color c) {
                                          switch (c) {
                                              case Color.Red: break;
                                              case Color.Green: break;
                                              case Color.Blue: break;
                                          }
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantEnumSwitchDefaultAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST007")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_SwitchStatementNonExhaustiveWithDefault_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SwitchStatementNonExhaustiveWithDefault_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public enum Color { Red, Green, Blue }
                                  public class Foo {
                                      public int Bar(Color c) {
                                          switch (c) {
                                              case Color.Red: return 1;
                                              case Color.Green: return 2;
                                              default: return 0;
                                          }
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantEnumSwitchDefaultAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST007")).IsFalse();
    }
}
