using Automaticks.CSharp.Complexity;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Complexity.LineLimits;

/// <summary>
///     Tests MethodLineLimitAnalyzer against local functions written as expressions.
/// </summary>
public class MethodLineLimitAnalyzerLocalFunctionTests
{
    /// <summary>
    ///     Tests that a local function with an expression body is measured and passed over.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExpressionBodiedLocalFunction_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Shape {
                                      public int Make() {
                                          int Double(int value) => value * 2;
                                          return Double(2);
                                      }
                                  }
                              }
                              """;

        var analyzer = new MethodLineLimitAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.MethodLineLimit)).IsFalse();
    }
}
