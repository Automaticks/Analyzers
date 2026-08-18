using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Documentation;

/// <summary>
///     Tests for PlainCommentAnalyzer.
/// </summary>
public class PlainCommentAnalyzerTests
{

    /// <summary>
    ///     Tests that Analyze_BlockComment_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BlockComment_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp
                              {
                                  /* block comment */
                                  public class Foo {}
                              }
                              """;

        var analyzer = new PlainCommentAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS041")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ExtensibleMarkupLanguageDocMultiLine_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExtensibleMarkupLanguageDocMultiLine_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp
                              {
                                  /// <summary>
                                  ///     A multi-line XML doc comment.
                                  /// </summary>
                                  public class Foo {}
                              }
                              """;

        var analyzer = new PlainCommentAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS041")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExtensibleMarkupLanguageDocSingleLine_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExtensibleMarkupLanguageDocSingleLine_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp
                              {
                                  /// <summary>
                                  ///     An XML doc comment.
                                  /// </summary>
                                  public class Foo {}
                              }
                              """;

        var analyzer = new PlainCommentAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS041")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_MultipleCommentsInFile_ReportsOneDiagnosticPerComment.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MultipleCommentsInFile_ReportsOneDiagnosticPerComment(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                      // comment one
                                      // comment two
                                      public void Bar() {} // comment three
                                  }
                              }
                              """;

        var analyzer = new PlainCommentAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.CountId(diagnostics, "ATXCS041")).IsEqualTo(3);
    }

    /// <summary>
    ///     Tests that Analyze_NoComments_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NoComments_NoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new PlainCommentAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS041")).IsFalse();
    }
    /// <summary>
    ///     Tests that Analyze_SingleLineComment_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SingleLineComment_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp
                              {
                                  // single line comment
                                  public class Foo {}
                              }
                              """;

        var analyzer = new PlainCommentAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS041")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_TrailingInlineComment_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TrailingInlineComment_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                      private const int Value = 42; // inline trailing comment
                                  }
                              }
                              """;

        var analyzer = new PlainCommentAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS041")).IsTrue();
    }
}
