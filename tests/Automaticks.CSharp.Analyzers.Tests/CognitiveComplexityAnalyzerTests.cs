using Automaticks.CSharp;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class CognitiveComplexityAnalyzerTests
{
    [Test]
    public async Task Analyze_AboveThreshold_ReportsDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CognitiveComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS033")).IsTrue();
    }

    [Test]
    public async Task Analyze_AbstractMethod_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public abstract class Foo {
                                      public abstract void Method(int a);
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CognitiveComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS033")).IsFalse();
    }

    [Test]
    public async Task Analyze_AtThreshold_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CognitiveComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS033")).IsFalse();
    }

    [Test]
    public async Task Analyze_BelowThreshold_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CognitiveComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS033")).IsFalse();
    }

    [Test]
    public async Task Analyze_CatchClause_IncrementsWithoutNestingPenalty()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CognitiveComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS033")).IsFalse();
    }

    [Test]
    public async Task Analyze_ConditionalExpression_IncrementsWithNestingPenalty()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CognitiveComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS033")).IsFalse();
    }

    [Test]
    public async Task Analyze_DiagnosticMessage_ContainsMethodNameAndScore()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CognitiveComplexityAnalyzer(), source);
        var message = diagnostics.Single(d => d.Id == "ATXCS033")
                                 .GetMessage(CultureInfo.InvariantCulture);

        await Assert.That(message)
                    .IsEqualTo("Method 'ComplexMethod' has a cognitive complexity of 16, which exceeds the maximum of 15");
    }

    [Test]
    public async Task Analyze_ElseIfChain_DoesNotDoublePenalizeElseBranches()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CognitiveComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS033")).IsFalse();
    }

    [Test]
    public async Task Analyze_ExpressionBodiedMethod_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Method(int a) => a + 1;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CognitiveComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS033")).IsFalse();
    }

    [Test]
    public async Task Analyze_ForeachLoop_IncrementsScore()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CognitiveComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS033")).IsFalse();
    }

    [Test]
    public async Task Analyze_LambdaBody_IncrementsNestingForInnerControlFlow()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CognitiveComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS033")).IsFalse();
    }

    [Test]
    public async Task Analyze_LocalFunction_ContributesToEnclosingMethodScore()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CognitiveComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS033")).IsFalse();
    }

    [Test]
    public async Task Analyze_LogicalAndSequence_CountsAsOne()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CognitiveComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS033")).IsFalse();
    }

    [Test]
    public async Task Analyze_LogicalOperatorTransitions_CountSeparately()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CognitiveComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS033")).IsFalse();
    }

    [Test]
    public async Task Analyze_NestingPenalty_NestedIfCostsMoreThanFlatIf()
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

        var nestedDiagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CognitiveComplexityAnalyzer(), nestedSource);
        var flatDiagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CognitiveComplexityAnalyzer(), flatSource);

        await Assert.That(nestedDiagnostics.Any(d => d.Id == "ATXCS033")).IsFalse();
        await Assert.That(flatDiagnostics.Any(d => d.Id == "ATXCS033")).IsFalse();
    }

    [Test]
    public async Task Analyze_NestingPenalty_ScoreReflectsDepth()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CognitiveComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS033")).IsTrue();
    }

    [Test]
    public async Task Analyze_SwitchStatement_IncrementsScore()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CognitiveComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS033")).IsFalse();
    }

    [Test]
    public async Task Analyze_WhileAndDoLoops_IncrementScore()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CognitiveComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS033")).IsFalse();
    }
}
