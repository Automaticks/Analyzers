using Automaticks.CSharp.Complexity;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Complexity;

/// <summary>
///     Tests ExcessiveParameterCountAnalyzer against an interface carrying several indexers.
/// </summary>
public class ExcessiveParameterCountAnalyzerMultipleIndexerTests
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
                                              int this[int a, int b, int c, int d, int e, int f] { get; }
                                              int this[string a, string b, string c, string d, string e, string f] { get; }
                                          }
                                      }
                                      """;
        const string source = """
                              namespace MyApp {
                                  public class Shape : External.IShape {
                                      public int this[int a, int b, int c, int d, int e, int f] { get { return a; } }
                                      public int this[string a, string b, string c, string d, string e, string f] { get { return 0; } }
                                  }
                              }
                              """;

        var externalRef = AnalyzerTestRunner.CompileToReference(externalSource);
        var analyzer = new ExcessiveParameterCountAnalyzer();
        var options = new AnalysisOptions
        {
            AdditionalReferences = [externalRef]
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS022")).IsFalse();
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
                                              int this[int a, int b, int c, int d, int e, int f] { get; }
                                              int this[string a, string b, string c, string d, string e, string f] { get; }
                                          }
                                      }
                                      """;
        const string source = """
                              namespace MyApp {
                                  public class Shape : External.IShape {
                                      public int this[string a, string b, string c, string d, string e, string f] { get { return 0; } }
                                  }
                              }
                              """;

        var externalRef = AnalyzerTestRunner.CompileToReference(externalSource);
        var analyzer = new ExcessiveParameterCountAnalyzer();
        var options = new AnalysisOptions
        {
            AdditionalReferences = [externalRef]
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS022")).IsFalse();
    }
}
