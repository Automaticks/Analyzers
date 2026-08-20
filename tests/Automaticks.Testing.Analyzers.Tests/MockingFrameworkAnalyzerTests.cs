using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Testing.Analyzers.Tests;

/// <summary>
///     Tests for MockingFrameworkAnalyzer.
/// </summary>
public class MockingFrameworkAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_TestProjectUsingAliasForTupleType_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TestProjectUsingAliasForTupleType_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using IntPair = (int, int);
                              namespace MyApp.Tests {
                                  public class FooTests {}
                              }
                              """;

        var analyzer = new MockingFrameworkAnalyzer();
        var options = new AnalysisOptions
{
    IsTestProject = true
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST001")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_TestProjectUsingMoq_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TestProjectUsingMoq_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using Moq;
                              namespace MyApp.Tests {
                                  public class FooTests {}
                              }
                              """;

        var analyzer = new MockingFrameworkAnalyzer();
        var options = new AnalysisOptions
{
    IsTestProject = true
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST001")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_TestProjectUsingRegularNamespace_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TestProjectUsingRegularNamespace_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp.Tests {
                                  public class FooTests {}
                              }
                              """;

        var analyzer = new MockingFrameworkAnalyzer();
        var options = new AnalysisOptions
{
    IsTestProject = true
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST001")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_TestProjectUsingSubstituteFramework_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TestProjectUsingSubstituteFramework_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using NSubstitute;
                              namespace MyApp.Tests {
                                  public class FooTests {}
                              }
                              """;

        var analyzer = new MockingFrameworkAnalyzer();
        var options = new AnalysisOptions
{
    IsTestProject = true
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST001")).IsTrue();
    }
}
