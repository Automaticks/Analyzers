using Automaticks.CSharp.Formatting;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Formatting;

/// <summary>
///     Tests the trivia shapes SingleBlankLineBetweenPropertiesAnalyzer walks.
/// </summary>
public class SingleBlankLineBetweenPropertiesAnalyzerTriviaTests
{
    /// <summary>
    ///     Tests that Analyze_CommentBetweenPropertiesWithoutBlankLine_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CommentBetweenPropertiesWithoutBlankLine_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int First { get; set; }
                                      // explains the second property
                                      public int Second { get; set; }
                                  }
                              }
                              """;

        var analyzer = new SingleBlankLineBetweenPropertiesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.SingleBlankLineBetweenProperties)).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_CommentWithBlankLineBetweenProperties_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CommentWithBlankLineBetweenProperties_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int First { get; set; }

                                      // explains the second property
                                      public int Second { get; set; }
                                  }
                              }
                              """;

        var analyzer = new SingleBlankLineBetweenPropertiesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.SingleBlankLineBetweenProperties)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_MultipleBlankLinesBetweenProperties_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MultipleBlankLinesBetweenProperties_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = "namespace MyApp {\n    public class Foo {\n        public int First { get; set; }\n\n\n        public int Second { get; set; }\n    }\n}";

        var analyzer = new SingleBlankLineBetweenPropertiesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.SingleBlankLineBetweenProperties)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_TrailingCommentBetweenProperties_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TrailingCommentBetweenProperties_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int First { get; set; } // trailing note

                                      public int Second { get; set; }
                                  }
                              }
                              """;

        var analyzer = new SingleBlankLineBetweenPropertiesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.SingleBlankLineBetweenProperties)).IsFalse();
    }
}
