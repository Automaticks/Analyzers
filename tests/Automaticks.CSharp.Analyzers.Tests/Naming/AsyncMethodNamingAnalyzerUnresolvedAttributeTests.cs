using Automaticks.CSharp.Naming;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Naming;

/// <summary>
///     Tests AsyncMethodNamingAnalyzer against attributes that do not resolve.
/// </summary>
public class AsyncMethodNamingAnalyzerUnresolvedAttributeTests
{
    /// <summary>
    ///     Tests that an unresolved attribute is not mistaken for a test attribute.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodWithUnresolvedAttribute_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Shape {
                                      [Missing]
                                      public async Task Draw() { await Task.Delay(1); }
                                  }
                              }
                              """;

        var analyzer = new AsyncMethodNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.AsyncMethodNaming)).IsTrue();
    }
}
