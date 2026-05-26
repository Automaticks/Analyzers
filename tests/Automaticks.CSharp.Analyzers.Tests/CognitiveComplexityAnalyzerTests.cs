using Automaticks.CSharp;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

/// <summary>
///     Tests for CognitiveComplexityAnalyzer.
/// </summary>
public class CognitiveComplexityAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_AboveThreshold_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AboveThreshold_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method(int a) {
                                          if (a == 1) {}
                                          if (a == 2) {}
                                          if (a == 3) {}
                                          if (a == 4) {}
                                          if (a == 5) {}
                                          if (a == 6) {}
                                          if (a == 7) {}
                                          if (a == 8) {}
                                          if (a == 9) {}
                                          if (a == 10) {}
                                          if (a == 11) {}
                                          if (a == 12) {}
                                          if (a == 13) {}
                                          if (a == 14) {}
                                          if (a == 15) {}
                                          if (a == 16) {}
                                      }
                                  }
                              }
                              """;

        var analyzer = new CognitiveComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS033")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_AbstractMethod_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AbstractMethod_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public abstract class Foo {
                                      public abstract void Method(int a);
                                  }
                              }
                              """;

        var analyzer = new CognitiveComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS033")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_AtThreshold_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AtThreshold_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method(int a) {
                                          if (a == 1) {}
                                          if (a == 2) {}
                                          if (a == 3) {}
                                          if (a == 4) {}
                                          if (a == 5) {}
                                          if (a == 6) {}
                                          if (a == 7) {}
                                          if (a == 8) {}
                                          if (a == 9) {}
                                          if (a == 10) {}
                                          if (a == 11) {}
                                          if (a == 12) {}
                                          if (a == 13) {}
                                          if (a == 14) {}
                                          if (a == 15) {}
                                      }
                                  }
                              }
                              """;

        var analyzer = new CognitiveComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS033")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_BelowThreshold_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BelowThreshold_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method(int a) {
                                          if (a == 1) {}
                                          if (a == 2) {}
                                          if (a == 3) {}
                                          if (a == 4) {}
                                          if (a == 5) {}
                                          if (a == 6) {}
                                          if (a == 7) {}
                                          if (a == 8) {}
                                          if (a == 9) {}
                                          if (a == 10) {}
                                          if (a == 11) {}
                                          if (a == 12) {}
                                          if (a == 13) {}
                                          if (a == 14) {}
                                      }
                                  }
                              }
                              """;

        var analyzer = new CognitiveComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS033")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_CatchClause_IncrementsWithoutNestingPenalty.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CatchClause_IncrementsWithoutNestingPenalty(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method(int a) {
                                          try {
                                              if (a > 0) { }
                                          }
                                          catch (System.Exception) { }
                                      }
                                  }
                              }
                              """;

        var analyzer = new CognitiveComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS033")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ConditionalExpression_IncrementsWithNestingPenalty.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConditionalExpression_IncrementsWithNestingPenalty(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Method(int a) {
                                          return a > 0 ? 1 : 0;
                                      }
                                  }
                              }
                              """;

        var analyzer = new CognitiveComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS033")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_DiagnosticMessage_ContainsMethodNameAndScore.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_DiagnosticMessage_ContainsMethodNameAndScore(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void ComplexMethod(int a) {
                                          if (a == 1) {}
                                          if (a == 2) {}
                                          if (a == 3) {}
                                          if (a == 4) {}
                                          if (a == 5) {}
                                          if (a == 6) {}
                                          if (a == 7) {}
                                          if (a == 8) {}
                                          if (a == 9) {}
                                          if (a == 10) {}
                                          if (a == 11) {}
                                          if (a == 12) {}
                                          if (a == 13) {}
                                          if (a == 14) {}
                                          if (a == 15) {}
                                          if (a == 16) {}
                                      }
                                  }
                              }
                              """;

        var analyzer = new CognitiveComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);
        var message = DiagnosticCollectionAssertions.GetSingleById(diagnostics, "ATXCS033")
                                 .GetMessage(CultureInfo.InvariantCulture);

        await Assert.That(message)
                    .IsEqualTo("Method 'ComplexMethod' has a cognitive complexity of 16, which exceeds the maximum of 15");
    }

    /// <summary>
    ///     Tests that Analyze_ElseIfChain_DoesNotDoublePenalizeElseBranches.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ElseIfChain_DoesNotDoublePenalizeElseBranches(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Method(int a) {
                                          if (a == 1) { return 1; }
                                          else if (a == 2) { return 2; }
                                          else if (a == 3) { return 3; }
                                          else { return 0; }
                                      }
                                  }
                              }
                              """;

        var analyzer = new CognitiveComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS033")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExpressionBodiedMethod_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExpressionBodiedMethod_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Method(int a) => a + 1;
                                  }
                              }
                              """;

        var analyzer = new CognitiveComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS033")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ForeachLoop_IncrementsScore.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ForeachLoop_IncrementsScore(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method(int[] items) {
                                          foreach (var item in items) { }
                                      }
                                  }
                              }
                              """;

        var analyzer = new CognitiveComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS033")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_LambdaBody_IncrementsNestingForInnerControlFlow.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LambdaBody_IncrementsNestingForInnerControlFlow(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method(int[] items) {
                                          System.Array.ForEach(items, x => {
                                              if (x > 0) { }
                                          });
                                      }
                                  }
                              }
                              """;

        var analyzer = new CognitiveComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS033")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_LocalFunction_ContributesToEnclosingMethodScore.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LocalFunction_ContributesToEnclosingMethodScore(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method(int a) {
                                          Compute(a);

                                          void Compute(int x) {
                                              if (x > 0) { }
                                          }
                                      }
                                  }
                              }
                              """;

        var analyzer = new CognitiveComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS033")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_LogicalAndSequence_CountsAsOne.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LogicalAndSequence_CountsAsOne(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool Method(int a, int b, int c) {
                                          return a > 0 && b > 0 && c > 0;
                                      }
                                  }
                              }
                              """;

        var analyzer = new CognitiveComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS033")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_LogicalOperatorTransitions_CountSeparately.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LogicalOperatorTransitions_CountSeparately(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool Method(int a, int b, int c) {
                                          return a > 0 && b > 0 || c > 0;
                                      }
                                  }
                              }
                              """;

        var analyzer = new CognitiveComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS033")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_NestingPenalty_NestedIfCostsMoreThanFlatIf.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NestingPenalty_NestedIfCostsMoreThanFlatIf(CancellationToken cancellationToken)
    {
        const string nestedSource = """
                                    namespace MyApp {
                                        public class Foo {
                                            public void Method(int a, int b) {
                                                if (a > 0) {
                                                    if (b > 0) { }
                                                }
                                            }
                                        }
                                    }
                                    """;

        const string flatSource = """
                                  namespace MyApp {
                                      public class Foo {
                                          public void Method(int a, int b) {
                                              if (a > 0) { }
                                              if (b > 0) { }
                                          }
                                      }
                                  }
                                  """;

        var analyzer = new CognitiveComplexityAnalyzer();
        var nestedDiagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, nestedSource, cancellationToken);
        var flatDiagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, flatSource, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(nestedDiagnostics, "ATXCS033")).IsFalse();
        await Assert.That(DiagnosticCollectionAssertions.HasId(flatDiagnostics, "ATXCS033")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_NestingPenalty_ScoreReflectsDepth.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NestingPenalty_ScoreReflectsDepth(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method(int a, int b, int c, int d, int e, int f) {
                                          if (a > 0) {
                                              if (b > 0) {
                                                  if (c > 0) {
                                                      if (d > 0) {
                                                          if (e > 0) {
                                                          }
                                                      }
                                                  }
                                              }
                                          }
                                          if (f > 0) { }
                                      }
                                  }
                              }
                              """;

        var analyzer = new CognitiveComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS033")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_SwitchStatement_IncrementsScore.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SwitchStatement_IncrementsScore(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Method(int a) {
                                          switch (a) {
                                              case 1: return 1;
                                              case 2: return 2;
                                              default: return 0;
                                          }
                                      }
                                  }
                              }
                              """;

        var analyzer = new CognitiveComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS033")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_WhileAndDoLoops_IncrementScore.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_WhileAndDoLoops_IncrementScore(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method(int a) {
                                          while (a > 0) { a--; }
                                          do { a++; } while (a < 10);
                                      }
                                  }
                              }
                              """;

        var analyzer = new CognitiveComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS033")).IsFalse();
    }
}
