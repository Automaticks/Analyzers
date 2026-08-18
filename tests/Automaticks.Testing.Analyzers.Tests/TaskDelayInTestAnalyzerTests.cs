using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Testing.Analyzers.Tests;

/// <summary>
///     Tests for TaskDelayInTestAnalyzer.
/// </summary>
public class TaskDelayInTestAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_TestProjectWithCustomDelayMethod_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TestProjectWithCustomDelayMethod_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp.Tests {
                                  public class FooTests {
                                      private static Task Delay(int ms) { return Task.CompletedTask; }
                                      public async Task Bar_Something_ReturnsTrue() { await Delay(200); }
                                  }
                              }
                              """;

        var analyzer = new TaskDelayInTestAnalyzer();
        var options = new AnalysisOptions
{
    IsTestProject = true
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST004")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_TestProjectWithFullyQualifiedTaskDelay_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TestProjectWithFullyQualifiedTaskDelay_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp.Tests {
                                  public class FooTests {
                                      public async System.Threading.Tasks.Task Bar_Something_ReturnsTrue() {
                                          await System.Threading.Tasks.Task.Delay(200);
                                      }
                                  }
                              }
                              """;

        var analyzer = new TaskDelayInTestAnalyzer();
        var options = new AnalysisOptions
{
    IsTestProject = true
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST004")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_TestProjectWithTaskDelay_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TestProjectWithTaskDelay_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp.Tests {
                                  public class FooTests {
                                      public async Task Bar_Something_ReturnsTrue() { await Task.Delay(200); }
                                  }
                              }
                              """;

        var analyzer = new TaskDelayInTestAnalyzer();
        var options = new AnalysisOptions
{
    IsTestProject = true
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST004")).IsTrue();
    }
}
