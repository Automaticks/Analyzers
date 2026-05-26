using Automaticks.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Diagnostics.CodeAnalysis.Analyzers.Tests;

/// <summary>
///     Tests for SuppressionCommentAnalyzer.
/// </summary>
public class SuppressionCommentAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_CleanCode_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CleanCode_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                      public void Bar() {}
                                  }
                              }
                              """;

        var analyzer = new SuppressionCommentAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasAnyId(diagnostics, ["ATXDC018", "ATXDC019"])).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_PragmaWarningDisable_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PragmaWarningDisable_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp
                              {
                              #pragma warning disable CS0168
                                  public class Foo {}
                              #pragma warning restore CS0168
                              }
                              """;

        var analyzer = new SuppressionCommentAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXDC018")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_PragmaWarningRestore_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PragmaWarningRestore_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp
                              {
                              #pragma warning restore CS0168
                                  public class Foo {}
                              }
                              """;

        var analyzer = new SuppressionCommentAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXDC018")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_RegularComment_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_RegularComment_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp
                              {
                                  public class Foo {}
                              }
                              """;

        var analyzer = new SuppressionCommentAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasAnyId(diagnostics, ["ATXDC018", "ATXDC019"])).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ReSharperDisableFile_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ReSharperDisableFile_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              // ReSharper disable SomeRule
                              namespace MyApp
                              {
                                  public class Foo {}
                              }
                              """;

        var analyzer = new SuppressionCommentAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXDC019")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ReSharperDisableOnce_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ReSharperDisableOnce_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp
                              {
                                  // ReSharper disable once SomeRule
                                  public class Foo {}
                              }
                              """;

        var analyzer = new SuppressionCommentAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXDC019")).IsTrue();
    }
}
