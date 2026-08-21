using Automaticks.CSharp.LanguageFeatures;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures.ExternalInterfaces;

/// <summary>
///     Tests RedundantNullCheckAnalyzer against a throw that has no type behind it.
/// </summary>
public class RedundantNullCheckAnalyzerThrowNullTests
{
    /// <summary>
    ///     Tests that throwing a bare null is not read as an argument null check.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ThrowOfNullLiteral_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              #nullable enable
                              namespace MyApp {
                                  public class Shape {
                                      public void Draw(string owner) {
                                          if (owner == null) { throw null; }
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantNullCheckAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.RedundantNullCheck)).IsFalse();
    }
}
