using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures;

/// <summary>
///     Tests for ParamsParameterAnalyzer.
/// </summary>
public class ParamsParameterAnalyzerTests
{

    /// <summary>
    ///     Tests that Analyze_MethodWithoutParams_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodWithoutParams_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(IReadOnlyList<int> values) { }
                                  }
                              }
                              """;

        var analyzer = new ParamsParameterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS055")).IsFalse();
    }
    /// <summary>
    ///     Tests that Analyze_MethodWithParamsArray_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodWithParamsArray_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(params int[] values) { }
                                  }
                              }
                              """;

        var analyzer = new ParamsParameterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS055")).IsTrue();
    }
}
