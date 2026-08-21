using Automaticks.CSharp.LanguageFeatures.ExpressionBodies;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures.ExpressionBodies;

/// <summary>
///     Tests that ExpressionBodiedOperatorAnalyzer leaves block bodies alone.
/// </summary>
public class ExpressionBodiedOperatorAnalyzerBlockBodyTests
{
    /// <summary>
    ///     Tests that a block body is not reported.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BlockBody_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = "namespace MyApp { public class Shape { public static Shape operator +(Shape left, Shape right) { return left; } } }";

        var analyzer = new ExpressionBodiedOperatorAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS079")).IsFalse();
    }
}