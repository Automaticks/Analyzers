using Automaticks.CSharp.Formatting;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Formatting;

/// <summary>
///     Tests the preprocessor and comment trivia paths between usings and the namespace.
/// </summary>
public class SingleBlankLineBetweenUsingsAndNamespaceAnalyzerTriviaTests
{
    /// <summary>
    ///     Tests that Analyze_CommentBeforeNamespace_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CommentBeforeNamespace_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;

                              // describes the namespace
                              namespace MyApp {
                                  public class Foo { }
                              }
                              """;

        var analyzer = new SingleBlankLineBetweenUsingsAndNamespaceAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.SingleBlankLineBetweenUsingsAndNamespace)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ElseAndRegionDirectives_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ElseAndRegionDirectives_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;

                              #region Types
                              #if NET8_0
                              #else
                              #endif
                              #endregion

                              namespace MyApp {
                                  public class Foo { }
                              }
                              """;

        var analyzer = new SingleBlankLineBetweenUsingsAndNamespaceAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.SingleBlankLineBetweenUsingsAndNamespace)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_PreprocessorDirectiveBeforeNamespace_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PreprocessorDirectiveBeforeNamespace_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;

                              #if NET8_0
                              #endif

                              namespace MyApp {
                                  public class Foo { }
                              }
                              """;

        var analyzer = new SingleBlankLineBetweenUsingsAndNamespaceAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.SingleBlankLineBetweenUsingsAndNamespace)).IsFalse();
    }
}
