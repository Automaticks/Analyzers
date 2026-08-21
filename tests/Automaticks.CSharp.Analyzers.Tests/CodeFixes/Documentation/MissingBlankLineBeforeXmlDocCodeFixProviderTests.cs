using Automaticks.CSharp.CodeFixes.Documentation;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes.Documentation;

/// <summary>
///     Tests for MissingBlankLineBeforeXmlDocCodeFixProvider.
///     Fixtures use escaped newlines so the layout under test is not reported against this file.
/// </summary>
public class MissingBlankLineBeforeXmlDocCodeFixProviderTests
{
    private const string CrampedSource = "namespace MyApp {\n    public class Foo {\n        public void Bar() { }\n        /// <summary>\n        ///     Does a thing.\n        /// </summary>\n        public void Baz() { }\n    }\n}\n";
    private const string SpacedSource = "namespace MyApp {\n    public class Foo {\n        public void Bar() { }\n\n        /// <summary>\n        ///     Does a thing.\n        /// </summary>\n        public void Baz() { }\n    }\n}\n";
    private const string TwoCrampedSource = "namespace MyApp {\n    public class Foo {\n        public void Bar() { }\n        /// <summary>\n        ///     First.\n        /// </summary>\n        public void Baz() { }\n        /// <summary>\n        ///     Second.\n        /// </summary>\n        public void Qux() { }\n    }\n}\n";

    /// <summary>
    ///     Tests that repeated application spaces every cramped doc comment.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyAllFixes_SeveralCrampedDocComments_SpacesEveryOne(CancellationToken cancellationToken)
    {
        var analyzer = new MissingBlankLineBeforeXmlDocAnalyzer();
        var provider = new MissingBlankLineBeforeXmlDocCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = TwoCrampedSource
        };
        var fixedSource = await CodeFixTestRunner.ApplyAllFixesAsync(request, cancellationToken);
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
    ///     Tests that the documentation and members survive the insertion.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_CrampedDocComment_KeepsSurroundingCode(CancellationToken cancellationToken)
    {
        var analyzer = new MissingBlankLineBeforeXmlDocAnalyzer();
        var provider = new MissingBlankLineBeforeXmlDocCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = CrampedSource
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("public void Bar() { }");
        await Assert.That(fixedSource).Contains("Does a thing.");
        await Assert.That(fixedSource).Contains("public void Baz() { }");
    }

    /// <summary>
    ///     Tests that a blank line is inserted before the doc comment.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_CrampedDocComment_LeavesNoDiagnostic(CancellationToken cancellationToken)
    {
        var analyzer = new MissingBlankLineBeforeXmlDocAnalyzer();
        var provider = new MissingBlankLineBeforeXmlDocCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = CrampedSource
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
    ///     Tests that a document without any line break falls back to inserting a newline.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFixForSpan_DocumentWithoutLineBreaks_InsertsNewlineFallback(CancellationToken cancellationToken)
    {
        const string source = "public class Foo { public void Bar() { } }";
        var analyzer = new MissingBlankLineBeforeXmlDocAnalyzer();
        var provider = new MissingBlankLineBeforeXmlDocCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var span = new TextSpan(0, 0);
        var fixedSource = await CodeFixTestRunner.ApplyFixForSpanAsync(
            request,
            MissingBlankLineBeforeXmlDocAnalyzer.Rule,
            span,
            cancellationToken);

        await Assert.That(fixedSource).IsEqualTo("\n" + source);
    }

    /// <summary>
    ///     Tests that an empty document skips the position clamp and still inserts a newline.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFixForSpan_EmptyDocument_SkipsClamp(CancellationToken cancellationToken)
    {
        const string source = "";
        var analyzer = new MissingBlankLineBeforeXmlDocAnalyzer();
        var provider = new MissingBlankLineBeforeXmlDocCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var span = new TextSpan(0, 0);
        var fixedSource = await CodeFixTestRunner.ApplyFixForSpanAsync(
            request,
            MissingBlankLineBeforeXmlDocAnalyzer.Rule,
            span,
            cancellationToken);

        await Assert.That(fixedSource).IsEqualTo("\n");
    }

    /// <summary>
    ///     Tests that a span at the end of a non-empty document clamps to the last line.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFixForSpan_PositionAtDocumentEnd_ClampsToLastLine(CancellationToken cancellationToken)
    {
        const string source = "namespace MyApp {\n    public class Foo { }\n}\n";
        var analyzer = new MissingBlankLineBeforeXmlDocAnalyzer();
        var provider = new MissingBlankLineBeforeXmlDocCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var span = new TextSpan(source.Length, 0);
        var fixedSource = await CodeFixTestRunner.ApplyFixForSpanAsync(
            request,
            MissingBlankLineBeforeXmlDocAnalyzer.Rule,
            span,
            cancellationToken);

        await Assert.That(fixedSource).IsEqualTo("namespace MyApp {\n    public class Foo { }\n\n}\n");
    }

    /// <summary>
    ///     Tests that a correctly spaced doc comment is never offered a fix.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountFixable_SpacedDocComment_ReportsZero(CancellationToken cancellationToken)
    {
        var analyzer = new MissingBlankLineBeforeXmlDocAnalyzer();
        var provider = new MissingBlankLineBeforeXmlDocCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = SpacedSource
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
        var provider = new MissingBlankLineBeforeXmlDocCodeFixProvider();
        var fixAllProvider = provider.GetFixAllProvider();

        await Assert.That(fixAllProvider).IsEqualTo(WellKnownFixAllProviders.BatchFixer);
    }
}
