using Automaticks.Testing.Analyzers.Tests.Stubs;
using Automaticks.Testing.Testability;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Testing.Analyzers.Tests.Testability;

/// <summary>
///     Tests for RedundantEnumSwitchDefaultAnalyzer.
/// </summary>
public class RedundantEnumSwitchDefaultAnalyzerTests
{
    private const string SwitchExpressionSource = """
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
    private const string SwitchStatementSource = """
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

    /// <summary>
    ///     Tests that a test casting an out-of-range value does not silence the rule on its own.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CastingTestInSameCompilation_ReportsDiagnostic(CancellationToken cancellationToken)
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
                                              default: throw new System.ArgumentOutOfRangeException(nameof(c));
                                          }
                                      }
                                  }
                                  public class FooTests {
                                      public void Bar_InvalidColor_Throws() {
                                          const Color invalid = (Color)99;
                                          var foo = new Foo();
                                          foo.Bar(invalid);
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantEnumSwitchDefaultAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST007")).IsTrue();
    }

    /// <summary>
    ///     Tests that a report without an entry for the analysed file leaves the rule reporting.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CoverageReportMissingFile_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string report = """
                              <coverage version="1.9"><packages><package name="MyApp"><classes>
                                <class name="MyApp.Other" filename="MyApp/Other.cs"><lines>
                                  <line number="9" hits="1" />
                                </lines></class>
                              </classes></package></packages></coverage>
                              """;

        var diagnostics = await AnalyzeWithReportAsync(SwitchStatementSource, report, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST007")).IsTrue();
    }

    /// <summary>
    ///     Tests that a report showing the default branch executed silences the rule.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CoverageReportShowsDefaultCovered_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string report = """
                              <coverage version="1.9"><packages><package name="MyApp"><classes>
                                <class name="MyApp.Foo" filename="MyApp/Foo.cs"><lines>
                                  <line number="9" hits="1" />
                                </lines></class>
                              </classes></package></packages></coverage>
                              """;

        var diagnostics = await AnalyzeWithReportAsync(SwitchStatementSource, report, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST007")).IsFalse();
    }

    /// <summary>
    ///     Tests that a report showing the default branch never executed leaves the rule reporting.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CoverageReportShowsDefaultUncovered_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string report = """
                              <coverage version="1.9"><packages><package name="MyApp"><classes>
                                <class name="MyApp.Foo" filename="MyApp/Foo.cs"><lines>
                                  <line number="9" hits="0" />
                                </lines></class>
                              </classes></package></packages></coverage>
                              """;

        var diagnostics = await AnalyzeWithReportAsync(SwitchStatementSource, report, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST007")).IsTrue();
    }

    /// <summary>
    ///     Tests that a report showing the discard arm executed silences the rule.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CoveredDiscardArm_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string report = """
                              <coverage version="1.9"><packages><package name="MyApp"><classes>
                                <class name="MyApp.Foo" filename="MyApp/Foo.cs"><lines>
                                  <line number="8" hits="1" />
                                </lines></class>
                              </classes></package></packages></coverage>
                              """;

        var diagnostics = await AnalyzeWithReportAsync(SwitchExpressionSource, report, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST007")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_NonEnumSwitchExpression_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NonEnumSwitchExpression_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public string Bar(int x) => x switch {
                                          1 => "one",
                                          _ => "other",
                                      };
                                  }
                              }
                              """;

        var analyzer = new RedundantEnumSwitchDefaultAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST007")).IsFalse();
    }

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
    ///     Tests that Analyze_SwitchExpressionWithOrPatternArm_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SwitchExpressionWithOrPatternArm_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public enum Color { Red, Green, Blue }
                                  public class Foo {
                                      public string Bar(Color c) => c switch {
                                          Color.Red or Color.Green => "warm",
                                          Color.Blue => "cool",
                                          _ => "unknown",
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

    private async Task<ImmutableArray<Diagnostic>> AnalyzeWithReportAsync(
        string source,
        string reportXml,
        CancellationToken cancellationToken)
    {
        var analyzer = new RedundantEnumSwitchDefaultAnalyzer();
        var additionalText = new TestAdditionalText("C:/repo/artifacts/coverage.cobertura.xml", reportXml);
        var additionalFiles = new List<AdditionalText>
        {
            additionalText,
        };
        var options = new AnalysisOptions
        {
            FilePath = "C:/repo/MyApp/Foo.cs",
            AdditionalFiles = additionalFiles,
        };
        return await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);
    }
}
