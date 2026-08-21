using Automaticks.CSharp.LanguageFeatures;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures.ExternalInterfaces;

/// <summary>
///     Tests that ParameterDefaultValueAnalyzer leaves indexers overriding compiled code alone.
/// </summary>
public class ParameterDefaultValueAnalyzerExternalIndexerTests
{
    /// <summary>
    ///     Tests that an indexer overriding a compiled base indexer is not reported.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_OverrideOfExternalIndexer_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string externalSource = """
                                      namespace External {
                                          public class Base {
                                              public virtual int this[int index = 1] { get { return index; } }
                                          }
                                      }
                                      """;
        const string source = """
                              namespace MyApp {
                                  public class Shape : External.Base {
                                      public override int this[int index = 1] { get { return index; } }
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
    ///     Tests that an indexer overriding a base declared here is still reported.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_OverrideOfSourceIndexer_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Base {
                                      public virtual int this[int index = 1] { get { return index; } }
                                  }
                                  public class Shape : Base {
                                      public override int this[int index = 1] { get { return index; } }
                                  }
                              }
                              """;

        var analyzer = new ParameterDefaultValueAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.ParameterDefaultValue)).IsTrue();
    }
}
