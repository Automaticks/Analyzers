using Automaticks.CSharp.LanguageFeatures.ExpressionBodies;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures.ExpressionBodies;

/// <summary>
///     Tests that ExpressionBodiedLocalFunctionAnalyzer leaves block bodies alone.
/// </summary>
public class ExpressionBodiedLocalFunctionAnalyzerBlockBodyTests
{
    /// <summary>
    ///     Tests that a block body is not reported.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BlockBody_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = "namespace MyApp { public class Shape { public void Draw() { int Twice(int v) { return v * 2; } Twice(1); } } }";

        var analyzer = new ExpressionBodiedLocalFunctionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS076")).IsFalse();
    }
}