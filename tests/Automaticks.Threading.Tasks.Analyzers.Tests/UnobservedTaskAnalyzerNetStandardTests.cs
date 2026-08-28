using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Threading.Tasks.Analyzers.Tests;

/// <summary>
///     Tests UnobservedTaskAnalyzer against a netstandard2.0 compilation, where ValueTask does not resolve.
/// </summary>
public class UnobservedTaskAnalyzerNetStandardTests
{
    /// <summary>
    ///     Tests that a generic non-task return type does not crash the analyzer when ValueTask is unresolvable.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NonTaskGenericReturnTypeOnNetStandard_ReportsNoAnalyzerFailure(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Foo {
                                      public List<int> GetList() { return null; }
                                      public void Caller() { GetList(); }
                                  }
                              }
                              """;

        var options = new AnalysisOptions
        {
            PlatformReferences = NetStandardReferences.GetFacade()
        };
        var analyzer = new UnobservedTaskAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "AD0001")).IsFalse();
        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA010")).IsFalse();
    }

    /// <summary>
    ///     Tests that a discarded task is still reported when ValueTask is unresolvable.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TaskReturningMethodOnNetStandard_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public Task<int> GetValueAsync() { return null; }
                                      public void Caller() { GetValueAsync(); }
                                  }
                              }
                              """;

        var options = new AnalysisOptions
        {
            PlatformReferences = NetStandardReferences.GetFacade()
        };
        var analyzer = new UnobservedTaskAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "AD0001")).IsFalse();
        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA010")).IsTrue();
    }
}
