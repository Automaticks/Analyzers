using Automaticks.CSharp.Naming;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Naming;

/// <summary>
///     Tests BooleanMethodNamingAnalyzer against names shorter than the allowed prefixes.
/// </summary>
public class BooleanMethodNamingAnalyzerShortNameTests
{
    /// <summary>
    ///     Tests that a name too short to hold a prefix is reported.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ShortMethodName_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Shape {
                                      public bool Go() { return true; }
                                  }
                              }
                              """;

        var analyzer = new BooleanMethodNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.BooleanMethodNaming)).IsTrue();
    }
}
