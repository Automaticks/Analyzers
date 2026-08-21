using Automaticks.CSharp.LanguageFeatures;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures.ExternalInterfaces;

/// <summary>
///     Tests DirectCastAnalyzer against a cast whose target type does not resolve.
/// </summary>
public class DirectCastAnalyzerUnresolvedTypeTests
{
    /// <summary>
    ///     Tests that a cast to an unknown type is still reported.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CastToUnknownType_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Shape {
                                      public void Draw(object value) {
                                          var result = (Missing)value;
                                      }
                                  }
                              }
                              """;

        var analyzer = new DirectCastAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.DirectCast)).IsTrue();
    }
}
