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
}
