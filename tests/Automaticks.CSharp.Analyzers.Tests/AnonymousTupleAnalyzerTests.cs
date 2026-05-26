using Automaticks.CSharp;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

/// <summary>
///     Tests for AnonymousTupleAnalyzer.
/// </summary>
public class AnonymousTupleAnalyzerTests
{
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
