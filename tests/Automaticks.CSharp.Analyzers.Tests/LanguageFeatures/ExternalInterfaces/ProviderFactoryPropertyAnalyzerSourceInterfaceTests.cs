using Automaticks.CSharp.LanguageFeatures;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures.ExternalInterfaces;

/// <summary>
///     Tests ProviderFactoryPropertyAnalyzer against explicit implementations declared here.
/// </summary>
public class ProviderFactoryPropertyAnalyzerSourceInterfaceTests
{
    /// <summary>
    ///     Tests that an explicit implementation of a local interface is still reported.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExplicitImplementationOfSourceInterface_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface IShape {
                                      string Name { get; }
                                  }
                                  public interface ISized {
                                      string Size { get; }
                                  }
                                  public class DataProvider : IShape, ISized {
                                      string IShape.Name { get { return string.Empty; } }
                                      string ISized.Size { get { return string.Empty; } }
                                  }
                              }
                              """;

        var analyzer = new ProviderFactoryPropertyAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.ProviderFactoryProperty)).IsTrue();
    }
}
