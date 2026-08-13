using Automaticks.CSharp;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures;

/// <summary>
///     Tests for OutParameterAnalyzer.
/// </summary>
public class OutParameterAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_MethodWithNoOutParams_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodWithNoOutParams_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void DoWork(int a, int b) {}
                                  }
                              }
                              """;

        var analyzer = new OutParameterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasAnyId(diagnostics, ["ATXCS023", "ATXCS024"])).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_MethodWithOneOutParamLast_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodWithOneOutParamLast_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool TryGet(int id, out string result) { result = ""; return true; }
                                  }
                              }
                              """;

        var analyzer = new OutParameterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasAnyId(diagnostics, ["ATXCS023", "ATXCS024"])).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_MethodWithOutParamNotLast_ReportsAtxCs024.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodWithOutParamNotLast_ReportsAtxCs024(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void DoWork(out int result, int extra) { result = 0; }
                                  }
                              }
                              """;

        var analyzer = new OutParameterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS024")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_MethodWithTwoOutParams_ReportsAtxCs023.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodWithTwoOutParams_ReportsAtxCs023(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void GetValues(out int a, out int b) { a = 0; b = 0; }
                                  }
                              }
                              """;

        var analyzer = new OutParameterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS023")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_MethodWithTwoOutParamsFirstNotLast_ReportsBothDiagnostics.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodWithTwoOutParamsFirstNotLast_ReportsBothDiagnostics(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void DoWork(out int a, out int b, int extra) { a = 0; b = 0; }
                                  }
                              }
                              """;

        var analyzer = new OutParameterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS023")).IsTrue();
        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS024")).IsTrue();
    }
}
