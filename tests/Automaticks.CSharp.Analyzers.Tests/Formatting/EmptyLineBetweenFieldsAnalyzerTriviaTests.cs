using Automaticks.CSharp.Formatting;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Formatting;

/// <summary>
///     Tests the trivia shapes EmptyLineBetweenFieldsAnalyzer walks between field declarations.
/// </summary>
public class EmptyLineBetweenFieldsAnalyzerTriviaTests
{
    /// <summary>
    ///     Tests that Analyze_BlankLineAfterCommentBetweenFields_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BlankLineAfterCommentBetweenFields_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private int _first;

                                      // explains the second field
                                      private int _second;
                                  }
                              }
                              """;

        var analyzer = new EmptyLineBetweenFieldsAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.EmptyLineBetweenFields)).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_CommentBetweenFieldsWithoutBlankLine_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CommentBetweenFieldsWithoutBlankLine_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private int _first;
                                      // explains the second field
                                      private int _second;
                                  }
                              }
                              """;

        var analyzer = new EmptyLineBetweenFieldsAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.EmptyLineBetweenFields)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_TrailingCommentBetweenFields_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TrailingCommentBetweenFields_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private int _first; // trailing note
                                      private int _second;
                                  }
                              }
                              """;

        var analyzer = new EmptyLineBetweenFieldsAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.EmptyLineBetweenFields)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_TrailingCommentThenBlankLine_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TrailingCommentThenBlankLine_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private int _first; // trailing note

                                      private int _second;
                                  }
                              }
                              """;

        var analyzer = new EmptyLineBetweenFieldsAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.EmptyLineBetweenFields)).IsTrue();
    }
}
