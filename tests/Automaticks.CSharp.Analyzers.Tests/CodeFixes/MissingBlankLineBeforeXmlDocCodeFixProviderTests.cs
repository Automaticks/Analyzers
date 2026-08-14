using Automaticks.CSharp.CodeFixes.Documentation;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes;

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
}
