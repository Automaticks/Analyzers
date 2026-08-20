using Automaticks.CSharp.CodeFixes.Documentation;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes.Documentation;

/// <summary>
///     Tests for SummaryXmlDocFormatCodeFixProvider.
/// </summary>
public class SummaryXmlDocFormatCodeFixProviderTests
{
    /// <summary>
    ///     Tests that a multi-line summary whose content lacks the 4-space indent is corrected.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_MultiLineSummaryWithoutContentIndent_AddsFourSpaceIndent(CancellationToken cancellationToken)
    {
        const string source = "namespace MyApp {\n    /// <summary>\n    /// Wrong indent here.\n    /// </summary>\n    public class Foo { }\n}\n";

        var analyzer = new SummaryXmlDocFormatAnalyzer();
        var provider = new SummaryXmlDocFormatCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("    ///     Wrong indent here.");
    }

    /// <summary>
    ///     Tests that inline tags inside the prose survive the reformat.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_ProseWithInlineTag_KeepsTag(CancellationToken cancellationToken)
    {
        const string source = "namespace MyApp {\n    /// <summary>Uses <c>Foo</c> here.</summary>\n    public class Foo { }\n}\n";

        var analyzer = new SummaryXmlDocFormatAnalyzer();
        var provider = new SummaryXmlDocFormatCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("<c>Foo</c>");
    }

    /// <summary>
    ///     Tests that a single line summary is split across lines.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_SingleLineSummary_LeavesNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = "namespace MyApp {\n    /// <summary>Does a thing.</summary>\n    public class Foo { }\n}\n";

        var analyzer = new SummaryXmlDocFormatAnalyzer();
        var provider = new SummaryXmlDocFormatCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);
        var verifyRequest = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = fixedSource
        };
        var remaining = await CodeFixTestRunner.CountFixableAsync(verifyRequest, cancellationToken);

        await Assert.That(remaining).IsEqualTo(0);
    }

    /// <summary>
    ///     Tests that the prose moves onto its own indented line.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_SingleLineSummary_MovesProseToOwnLine(CancellationToken cancellationToken)
    {
        const string source = "namespace MyApp {\n    /// <summary>Does a thing.</summary>\n    public class Foo { }\n}\n";

        var analyzer = new SummaryXmlDocFormatAnalyzer();
        var provider = new SummaryXmlDocFormatCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("    /// <summary>");
        await Assert.That(fixedSource).Contains("    ///     Does a thing.");
        await Assert.That(fixedSource).Contains("    /// </summary>");
    }

    /// <summary>
    ///     Tests that a summary at column zero on the file's last line uses an empty indent and newline fallback.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFixForSpan_SummaryAtColumnZeroOnLastLine_UsesEmptyIndentAndNewlineFallback(CancellationToken cancellationToken)
    {
        const string source = "namespace MyApp {\n/** <summary>Does a thing.</summary> */ public class Foo { }";

        var analyzer = new SummaryXmlDocFormatAnalyzer();
        var provider = new SummaryXmlDocFormatCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var start = source.IndexOf("Does a thing.", System.StringComparison.Ordinal);
        var span = new TextSpan(start, "Does a thing.".Length);
        var fixedSource = await CodeFixTestRunner.ApplyFixForSpanAsync(
            request,
            SummaryXmlDocFormatAnalyzer.Rule,
            span,
            cancellationToken);

        await Assert.That(fixedSource).IsEqualTo("namespace MyApp {\n/// <summary>\n///     Does a thing.\n/// </summary>");
    }

    /// <summary>
    ///     Tests that a span with no enclosing summary element offers no fix.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountActionsForSpan_NoEnclosingSummaryElement_ReportsZero(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                              }
                              """;

        var analyzer = new SummaryXmlDocFormatAnalyzer();
        var provider = new SummaryXmlDocFormatCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var start = source.IndexOf("System", System.StringComparison.Ordinal);
        var span = new TextSpan(start, "System".Length);
        var count = await CodeFixTestRunner.CountActionsForSpanAsync(
            request,
            SummaryXmlDocFormatAnalyzer.Rule,
            span,
            cancellationToken);

        await Assert.That(count).IsEqualTo(0);
    }

    /// <summary>
    ///     Tests that a correctly formatted summary is never offered a fix.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountFixable_WellFormattedSummary_ReportsZero(CancellationToken cancellationToken)
    {
        const string source = "namespace MyApp {\n    /// <summary>\n    ///     Does a thing.\n    /// </summary>\n    public class Foo { }\n}\n";

        var analyzer = new SummaryXmlDocFormatAnalyzer();
        var provider = new SummaryXmlDocFormatCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var count = await CodeFixTestRunner.CountFixableAsync(request, cancellationToken);

        await Assert.That(count).IsEqualTo(0);
    }

    /// <summary>
    ///     Tests that the provider always exposes the batch Fix All provider.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetFixAllProvider_Always_ReturnsBatchFixer(CancellationToken cancellationToken)
    {
        var provider = new SummaryXmlDocFormatCodeFixProvider();
        var fixAllProvider = provider.GetFixAllProvider();

        await Assert.That(fixAllProvider).IsEqualTo(WellKnownFixAllProviders.BatchFixer);
    }
}
