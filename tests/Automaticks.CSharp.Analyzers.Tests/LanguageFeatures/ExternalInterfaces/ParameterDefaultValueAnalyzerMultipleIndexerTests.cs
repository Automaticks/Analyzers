using Automaticks.CSharp.LanguageFeatures;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures.ExternalInterfaces;

/// <summary>
///     Tests ParameterDefaultValueAnalyzer against an interface carrying several indexers.
/// </summary>
public class ParameterDefaultValueAnalyzerMultipleIndexerTests
{
    /// <summary>
    ///     Tests that each indexer is matched to its own interface member.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TwoExternalInterfaceIndexers_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string externalSource = """
                                      namespace External {
                                          public interface IShape {
                                              int this[int index = 1] { get; }
                                              int this[string name = ""] { get; }
                                          }
                                      }
                                      """;
        const string source = """
                              namespace MyApp {
                                  public class Shape : External.IShape {
                                      public int this[int index = 1] { get { return index; } }
                                      public int this[string name = ""] { get { return 0; } }
                                  }
                              }
                              """;

        var externalRef = AnalyzerTestRunner.CompileToReference(externalSource);
        var analyzer = new ParameterDefaultValueAnalyzer();
        var options = new AnalysisOptions
        {
            AdditionalReferences = [externalRef]
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.ParameterDefaultValue)).IsFalse();
    }

    /// <summary>
    ///     Tests that an interface indexer left unimplemented does not match anything.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_UnimplementedExternalInterfaceIndexer_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string externalSource = """
                                      namespace External {
                                          public interface IShape {
                                              int this[int index = 1] { get; }
                                              int this[string name = ""] { get; }
                                          }
                                      }
                                      """;
        const string source = """
                              namespace MyApp {
                                  public class Shape : External.IShape {
                                      public int this[int index = 1] { get { return index; } }
                                  }
                              }
                              """;

        var externalRef = AnalyzerTestRunner.CompileToReference(externalSource);
        var analyzer = new ParameterDefaultValueAnalyzer();
        var options = new AnalysisOptions
        {
            AdditionalReferences = [externalRef]
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.ParameterDefaultValue)).IsFalse();
    }
}
