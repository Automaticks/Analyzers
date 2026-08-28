using Automaticks.CSharp.Naming;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Naming;

/// <summary>
///     Tests AsyncReturnTypeHelper against a netstandard2.0 compilation, where ValueTask does not resolve.
/// </summary>
public class AsyncReturnTypeHelperNetStandardTests
{
    /// <summary>
    ///     Tests that a generic non-async return type does not crash the analyzer when ValueTask is unresolvable.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NonAsyncGenericReturnTypeOnNetStandard_ReportsNoAnalyzerFailure(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Foo {
                                      public List<int> GetList() { return null; }
                                  }
                              }
                              """;

        var options = new AnalysisOptions
        {
            PlatformReferences = NetStandardReferences.GetFacade()
        };
        var analyzer = new AsyncMethodNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "AD0001")).IsFalse();
        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.AsyncMethodNaming)).IsFalse();
    }

    /// <summary>
    ///     Tests that a task-returning method missing the Async suffix is still reported when ValueTask is unresolvable.
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
                                      public Task<int> GetValue() { return null; }
                                  }
                              }
                              """;

        var options = new AnalysisOptions
        {
            PlatformReferences = NetStandardReferences.GetFacade()
        };
        var analyzer = new AsyncMethodNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "AD0001")).IsFalse();
        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.AsyncMethodNaming)).IsTrue();
    }
}
