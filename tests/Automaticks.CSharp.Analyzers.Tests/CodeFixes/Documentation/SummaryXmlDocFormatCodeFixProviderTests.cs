using Automaticks.CSharp.CodeFixes.Documentation;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes.Documentation;

/// <summary>
///     Tests for SummaryXmlDocFormatCodeFixProvider.
/// </summary>
public class SummaryXmlDocFormatCodeFixProviderTests
{
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
