using Automaticks.CSharp.Formatting;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Formatting;

/// <summary>
///     Tests InlineNewExpressionAnalyzer against initializers that hang off a property.
/// </summary>
public class InlineNewExpressionAnalyzerPropertyInitializerTests
{
    /// <summary>
    ///     Tests that a property initializer is not read as a variable declaration.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PropertyInitializer_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Box { }
                                  public class Shape {
                                      public Box Corner { get; } = new Box();
                                  }
                              }
                              """;

        var analyzer = new InlineNewExpressionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.InlineNewExpression)).IsTrue();
    }
}
