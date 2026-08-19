using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures;

/// <summary>
///     Tests for AnonymousTupleAnalyzer.
/// </summary>
public class AnonymousTupleAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_DeconstructionOfCustomDeconstructibleType_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_DeconstructionOfCustomDeconstructibleType_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Point {
                                      public void Deconstruct(out int x, out int y) { x = 1; y = 2; }
                                  }
                                  public class Foo {
                                      public void Bar() {
                                          var (a, b) = new Point();
                                      }
                                  }
                              }
                              """;

        var analyzer = new AnonymousTupleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS012")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_DeconstructionOfNonTupleType_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_DeconstructionOfNonTupleType_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Point {
                                      public void Deconstruct(out int first, out int second) { first = 1; second = 2; }
                                  }
                                  public class Foo {
                                      public void Run(Point point) {
                                          (var first, var second) = point;
                                      }
                                  }
                              }
                              """;

        var analyzer = new AnonymousTupleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS012")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ForEachDeconstructionVariable_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ForEachDeconstructionVariable_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(IEnumerable<KeyValuePair<int, int>> pairs) {
                                          foreach (var (key, item) in pairs) {
                                              System.Console.WriteLine(key + item);
                                          }
                                      }
                                  }
                              }
                              """;

        var analyzer = new AnonymousTupleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS012")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_NamedTupleTypeDeclaration_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NamedTupleTypeDeclaration_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() {
                                          (string Name, int Age) p = ("Bob", 30);
                                      }
                                  }
                              }
                              """;

        var analyzer = new AnonymousTupleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS012")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_OutVarSingleDeclaration_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_OutVarSingleDeclaration_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private void TryGet(out int x) { x = 1; }
                                      public void Bar() {
                                          TryGet(out var x);
                                      }
                                  }
                              }
                              """;

        var analyzer = new AnonymousTupleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS012")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_StronglyTypedRecord_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StronglyTypedRecord_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public record MyType(string A, int B);
                                  public class Foo {
                                      public void Bar()
                                      {
                                          var x = new MyType("hello", 42);
                                      }
                                  }
                              }
                              """;

        var analyzer = new AnonymousTupleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS012")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_TupleAssignedFromNonTupleExpression_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TupleAssignedFromNonTupleExpression_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Run() {
                                          var a = 1;
                                          var b = 2;
                                          var pair = System.Tuple.Create(1, 2);
                                          (a, b) = (pair.Item1, pair.Item2);
                                      }
                                  }
                              }
                              """;

        var analyzer = new AnonymousTupleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS012")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_TupleAssignedFromTupleVariable_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TupleAssignedFromTupleVariable_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Run(System.ValueTuple<int, int> pair) {
                                          var a = 1;
                                          var b = 2;
                                          (a, b) = pair;
                                      }
                                  }
                              }
                              """;

        var analyzer = new AnonymousTupleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS012")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_TupleAssignedToDeclarationExpression_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TupleAssignedToDeclarationExpression_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Run() {
                                          var a = 1;
                                          var b = 2;
                                          (var x, var y) = (a, b);
                                      }
                                  }
                              }
                              """;

        var analyzer = new AnonymousTupleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS012")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_TupleDeconstruction_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TupleDeconstruction_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private (string, int) GetTuple()
                                      {
                                          return ("a", 1);
                                      }
                                      public void Bar()
                                      {
                                          var (x, y) = GetTuple();
                                      }
                                  }
                              }
                              """;

        var analyzer = new AnonymousTupleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS012")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_TupleLiteral_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TupleLiteral_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar()
                                      {
                                          var x = ("hello", 42);
                                      }
                                  }
                              }
                              """;

        var analyzer = new AnonymousTupleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS012")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_TupleParameter_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TupleParameter_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar((string, int) p)
                                      {
                                      }
                                  }
                              }
                              """;

        var analyzer = new AnonymousTupleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS012")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_TupleReturnType_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TupleReturnType_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public (string, int) Bar()
                                      {
                                          return ("a", 1);
                                      }
                                  }
                              }
                              """;

        var analyzer = new AnonymousTupleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS012")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_TupleSwapAssignment_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TupleSwapAssignment_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Swap() {
                                          var a = 1;
                                          var b = 2;
                                          (a, b) = (b, a);
                                      }
                                  }
                              }
                              """;

        var analyzer = new AnonymousTupleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS012")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_TupleTypeDeclaration_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TupleTypeDeclaration_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() {
                                          (string, int) x = ("a", 1);
                                      }
                                  }
                              }
                              """;

        var analyzer = new AnonymousTupleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS012")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_TupleWithCallExpressionInAssignment_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TupleWithCallExpressionInAssignment_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Get() => 1;
                                      public void Run() {
                                          var a = 1;
                                          var b = 2;
                                          (a, b) = (Get(), a);
                                      }
                                  }
                              }
                              """;

        var analyzer = new AnonymousTupleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS012")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_TupleWithNamedElementInAssignment_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TupleWithNamedElementInAssignment_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Run() {
                                          var a = 1;
                                          var b = 2;
                                          (a, b) = (First: b, Second: a);
                                      }
                                  }
                              }
                              """;

        var analyzer = new AnonymousTupleAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS012")).IsTrue();
    }
}
