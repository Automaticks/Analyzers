using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class ObjectInitializerCodeStyleAnalyzerTests
{

    [Test]
    public async Task Analyze_MultiLineMemberEndingOnSameLineAsCloseBrace_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { public int A { get; set; } }
                                  public class Bar {
                                      public void Run() {
                                          var x = new Foo
                                          {
                                              A =
                                                  1 };
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ObjectInitializerCodeStyleAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS059")).IsTrue();
    }

    [Test]
    public async Task Analyze_MultiLineMemberEndingSameLineAsNextMemberStart_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { public int A { get; set; } public int B { get; set; } }
                                  public class Bar {
                                      public void Run() {
                                          var x = new Foo
                                          {
                                              A =
                                                  1, B = 2
                                          };
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ObjectInitializerCodeStyleAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS059")).IsTrue();
    }

    // ── ATXCS059: format violations ────────────────────────────────────────

    [Test]
    public async Task Analyze_ObjectInitializerFullyInline_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { public int A { get; set; } }
                                  public class Bar {
                                      public void Run() {
                                          var x = new Foo { A = 1 };
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ObjectInitializerCodeStyleAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS059")).IsTrue();
    }

    [Test]
    public async Task Analyze_ObjectInitializerOpenBraceOnSameLineAsType_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { public int A { get; set; } }
                                  public class Bar {
                                      public void Run() {
                                          var x = new Foo {
                                              A = 1
                                          };
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ObjectInitializerCodeStyleAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS059")).IsTrue();
    }

    [Test]
    public async Task Analyze_ObjectInitializerCloseBraceOnSameLineAsLastMember_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { public int A { get; set; } }
                                  public class Bar {
                                      public void Run() {
                                          var x = new Foo
                                          {
                                              A = 1 };
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ObjectInitializerCodeStyleAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS059")).IsTrue();
    }

    [Test]
    public async Task Analyze_ObjectInitializerMultipleMembersOnSameLine_ReportsDiagnosticOnEachOffender()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { public int A { get; set; } public int B { get; set; } }
                                  public class Bar {
                                      public void Run() {
                                          var x = new Foo
                                          {
                                              A = 1, B = 2
                                          };
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ObjectInitializerCodeStyleAnalyzer(), source);

        await Assert.That(diagnostics.Count(d => d.Id == "ATXCS059")).IsEqualTo(2);
    }

    [Test]
    public async Task Analyze_CollectionInitializerFullyInline_ReportsDiagnostic()
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Bar {
                                      public void Run() {
                                          var x = new List<int> { 1, 2, 3 };
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ObjectInitializerCodeStyleAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS059")).IsTrue();
    }

    [Test]
    public async Task Analyze_ArrayInitializerFullyInline_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Bar {
                                      public void Run() {
                                          var x = new int[] { 1, 2 };
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ObjectInitializerCodeStyleAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS059")).IsTrue();
    }

    [Test]
    public async Task Analyze_WithExpressionFullyInline_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public record Foo(int X, int Y);
                                  public class Bar {
                                      public void Run() {
                                          var a = new Foo(1, 2);
                                          var b = a with { X = 10 };
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ObjectInitializerCodeStyleAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS059")).IsTrue();
    }

    [Test]
    public async Task Analyze_NestedObjectInitializerInline_ReportsDiagnosticOnInner()
    {
        const string source = """
                              namespace MyApp {
                                  public class Inner { public int X { get; set; } }
                                  public class Outer { public Inner Child { get; set; } }
                                  public class Bar {
                                      public void Run() {
                                          var x = new Outer
                                          {
                                              Child = new Inner { X = 1 }
                                          };
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ObjectInitializerCodeStyleAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS059")).IsTrue();
    }

    [Test]
    public async Task Analyze_SingleMemberCollectionInitializerInline_ReportsDiagnostic()
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Bar {
                                      public void Run() {
                                          var x = new List<int> { 1 };
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ObjectInitializerCodeStyleAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS059")).IsTrue();
    }

    // ── ATXCS060: empty braces ──────────────────────────────────────────────

    [Test]
    public async Task Analyze_EmptyObjectInitializer_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { }
                                  public class Bar {
                                      public void Run() {
                                          var x = new Foo { };
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ObjectInitializerCodeStyleAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS060")).IsTrue();
    }

    [Test]
    public async Task Analyze_EmptyCollectionInitializer_ReportsDiagnostic()
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Bar {
                                      public void Run() {
                                          var x = new List<int> { };
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ObjectInitializerCodeStyleAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS060")).IsTrue();
    }

    [Test]
    public async Task Analyze_EmptyWithInitializer_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public record Foo(int X);
                                  public class Bar {
                                      public void Run() {
                                          var a = new Foo(1);
                                          var b = a with { };
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ObjectInitializerCodeStyleAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS060")).IsTrue();
    }

    // ── GOOD: no diagnostic expected ──────────────────────────────────────────

    [Test]
    public async Task Analyze_ProperlyFormattedObjectInitializer_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { public int A { get; set; } public int B { get; set; } }
                                  public class Bar {
                                      public void Run() {
                                          var x = new Foo
                                          {
                                              A = 1,
                                              B = 2
                                          };
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ObjectInitializerCodeStyleAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS059" || d.Id == "ATXCS060")).IsFalse();
    }

    [Test]
    public async Task Analyze_ProperlyFormattedCollectionInitializer_ReportsNoDiagnostic()
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Bar {
                                      public void Run() {
                                          var x = new List<int>
                                          {
                                              1,
                                              2,
                                              3
                                          };
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ObjectInitializerCodeStyleAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS059" || d.Id == "ATXCS060")).IsFalse();
    }

    [Test]
    public async Task Analyze_ProperlyFormattedWithExpression_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public record Foo(int X, int Y);
                                  public class Bar {
                                      public void Run() {
                                          var a = new Foo(1, 2);
                                          var b = a with
                                          {
                                              X = 10
                                          };
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ObjectInitializerCodeStyleAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS059" || d.Id == "ATXCS060")).IsFalse();
    }

    [Test]
    public async Task Analyze_NoInitializer_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { }
                                  public class Bar {
                                      public void Run() {
                                          var x = new Foo();
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ObjectInitializerCodeStyleAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS059" || d.Id == "ATXCS060")).IsFalse();
    }

    [Test]
    public async Task Analyze_ProperlyFormattedSingleMemberObjectInitializer_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { public int A { get; set; } }
                                  public class Bar {
                                      public void Run() {
                                          var x = new Foo
                                          {
                                              A = 1
                                          };
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ObjectInitializerCodeStyleAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS059" || d.Id == "ATXCS060")).IsFalse();
    }

    [Test]
    public async Task Analyze_ProperlyFormattedNestedInitializer_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Inner { public int X { get; set; } }
                                  public class Outer { public Inner Child { get; set; } }
                                  public class Bar {
                                      public void Run() {
                                          var x = new Outer
                                          {
                                              Child = new Inner
                                              {
                                                  X = 1
                                              }
                                          };
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ObjectInitializerCodeStyleAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS059" || d.Id == "ATXCS060")).IsFalse();
    }

    [Test]
    public async Task Analyze_ProperlyFormattedArrayInitializer_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Bar {
                                      public void Run() {
                                          var x = new int[]
                                          {
                                              1,
                                              2
                                          };
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ObjectInitializerCodeStyleAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS059" || d.Id == "ATXCS060")).IsFalse();
    }
}
