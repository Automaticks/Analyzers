using Automaticks.CSharp.LanguageFeatures.ExpressionBodies;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures.ExpressionBodies;

/// <summary>
///     Tests that ExpressionBodiedDestructorAnalyzer leaves block bodies alone.
/// </summary>
public class ExpressionBodiedDestructorAnalyzerBlockBodyTests
{
    /// <summary>
    ///     Tests that a block body is not reported.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BlockBody_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = "namespace MyApp { public class Shape { ~Shape() { System.Console.WriteLine(); } } }";

        var analyzer = new ExpressionBodiedDestructorAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS082")).IsFalse();
    }
}