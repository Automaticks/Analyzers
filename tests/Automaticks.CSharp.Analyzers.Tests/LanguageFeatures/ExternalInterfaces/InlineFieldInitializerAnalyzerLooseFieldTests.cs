using Automaticks.CSharp.LanguageFeatures;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures.ExternalInterfaces;

/// <summary>
///     Tests InlineFieldInitializerAnalyzer when no type encloses the declaration.
/// </summary>
public class InlineFieldInitializerAnalyzerLooseFieldTests
{
    /// <summary>
    ///     Tests that a field left directly in a namespace is still reported.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FieldDirectlyInNamespace_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  private int _size = Fallback;
                              }
                              """;

        var analyzer = new InlineFieldInitializerAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.InlineFieldInitializer)).IsTrue();
    }
}
