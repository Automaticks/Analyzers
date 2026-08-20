using Automaticks.CSharp.LanguageFeatures;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures.ExternalInterfaces;

/// <summary>
///     Tests InterfaceDefaultImplementationAnalyzer against members it does not inspect.
/// </summary>
public class InterfaceDefaultImplementationAnalyzerNestedTypeTests
{
    /// <summary>
    ///     Tests that a type nested in an interface is stepped over.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NestedTypeInInterface_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface IShape {
                                      public class Corner { }
                                      void Draw();
                                  }
                              }
                              """;

        var analyzer = new InterfaceDefaultImplementationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.InterfaceDefaultImplementation)).IsFalse();
    }
}
