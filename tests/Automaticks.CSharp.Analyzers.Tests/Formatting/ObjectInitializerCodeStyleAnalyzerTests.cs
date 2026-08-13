using Automaticks.CSharp;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Formatting;

/// <summary>
///     Tests for ObjectInitializerCodeStyleAnalyzer.
/// </summary>
public class ObjectInitializerCodeStyleAnalyzerTests
{

    /// <summary>
    ///     Tests that Analyze_ArrayInitializerFullyInline_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ArrayInitializerFullyInline_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new ObjectInitializerCodeStyleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS059")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_CollectionInitializerFullyInline_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CollectionInitializerFullyInline_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new ObjectInitializerCodeStyleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS059")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_EmptyCollectionInitializer_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_EmptyCollectionInitializer_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new ObjectInitializerCodeStyleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS060")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_EmptyObjectInitializer_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_EmptyObjectInitializer_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new ObjectInitializerCodeStyleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS060")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_EmptyWithInitializer_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_EmptyWithInitializer_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new ObjectInitializerCodeStyleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS060")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_MultiLineMemberEndingOnSameLineAsCloseBrace_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MultiLineMemberEndingOnSameLineAsCloseBrace_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new ObjectInitializerCodeStyleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS059")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_MultiLineMemberEndingSameLineAsNextMemberStart_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MultiLineMemberEndingSameLineAsNextMemberStart_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new ObjectInitializerCodeStyleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS059")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_NestedObjectInitializerInline_ReportsDiagnosticOnInner.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NestedObjectInitializerInline_ReportsDiagnosticOnInner(CancellationToken cancellationToken)
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

        var analyzer = new ObjectInitializerCodeStyleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS059")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_NoInitializer_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NoInitializer_ReportsNoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new ObjectInitializerCodeStyleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasAnyId(diagnostics, ["ATXCS059", "ATXCS060"])).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ObjectInitializerCloseBraceOnSameLineAsLastMember_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ObjectInitializerCloseBraceOnSameLineAsLastMember_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new ObjectInitializerCodeStyleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS059")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ObjectInitializerFullyInline_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ObjectInitializerFullyInline_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new ObjectInitializerCodeStyleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS059")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ObjectInitializerMultipleMembersOnSameLine_ReportsDiagnosticOnEachOffender.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ObjectInitializerMultipleMembersOnSameLine_ReportsDiagnosticOnEachOffender(CancellationToken cancellationToken)
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

        var analyzer = new ObjectInitializerCodeStyleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.CountId(diagnostics, "ATXCS059")).IsEqualTo(2);
    }

    /// <summary>
    ///     Tests that Analyze_ObjectInitializerOpenBraceOnSameLineAsType_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ObjectInitializerOpenBraceOnSameLineAsType_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new ObjectInitializerCodeStyleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS059")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ProperlyFormattedArrayInitializer_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ProperlyFormattedArrayInitializer_ReportsNoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new ObjectInitializerCodeStyleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasAnyId(diagnostics, ["ATXCS059", "ATXCS060"])).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ProperlyFormattedCollectionInitializer_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ProperlyFormattedCollectionInitializer_ReportsNoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new ObjectInitializerCodeStyleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasAnyId(diagnostics, ["ATXCS059", "ATXCS060"])).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ProperlyFormattedNestedInitializer_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ProperlyFormattedNestedInitializer_ReportsNoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new ObjectInitializerCodeStyleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasAnyId(diagnostics, ["ATXCS059", "ATXCS060"])).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ProperlyFormattedObjectInitializer_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ProperlyFormattedObjectInitializer_ReportsNoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new ObjectInitializerCodeStyleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasAnyId(diagnostics, ["ATXCS059", "ATXCS060"])).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ProperlyFormattedSingleMemberObjectInitializer_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ProperlyFormattedSingleMemberObjectInitializer_ReportsNoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new ObjectInitializerCodeStyleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasAnyId(diagnostics, ["ATXCS059", "ATXCS060"])).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ProperlyFormattedWithExpression_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ProperlyFormattedWithExpression_ReportsNoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new ObjectInitializerCodeStyleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasAnyId(diagnostics, ["ATXCS059", "ATXCS060"])).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_SingleMemberCollectionInitializerInline_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SingleMemberCollectionInitializerInline_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new ObjectInitializerCodeStyleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS059")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_WithExpressionFullyInline_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_WithExpressionFullyInline_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new ObjectInitializerCodeStyleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS059")).IsTrue();
    }
}
