using Automaticks.CSharp.LanguageFeatures.ExpressionBodies;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures.ExpressionBodies;

/// <summary>
///     Tests that ExpressionBodiedPropertyAnalyzer leaves block bodies alone.
/// </summary>
public class ExpressionBodiedPropertyAnalyzerBlockBodyTests
{
    /// <summary>
    ///     Tests that a block body is not reported.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BlockBody_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = "namespace MyApp { public class Shape { public int Size { get { return 1; } } } }";

        var analyzer = new ExpressionBodiedPropertyAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS077")).IsFalse();
    }
}