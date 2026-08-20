using Automaticks.CSharp.LanguageFeatures;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures.ExternalInterfaces;

/// <summary>
///     Tests ParamsParameterAnalyzer against parameters that carry several modifiers.
/// </summary>
public class ParamsParameterAnalyzerModifierOrderTests
{
    /// <summary>
    ///     Tests that params is still found when another modifier precedes it.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExtensionParamsParameter_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public static class Shape {
                                      public static void Draw(this params int[] sides) { }
                                  }
                              }
                              """;

        var analyzer = new ParamsParameterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.ParamsParameter)).IsTrue();
    }

    /// <summary>
    ///     Tests that a parameter carrying only another modifier is left alone.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PlainParameter_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public static class Shape {
                                      public static void Draw(this string owner, int sides) { }
                                  }
                              }
                              """;

        var analyzer = new ParamsParameterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.ParamsParameter)).IsFalse();
    }
}
