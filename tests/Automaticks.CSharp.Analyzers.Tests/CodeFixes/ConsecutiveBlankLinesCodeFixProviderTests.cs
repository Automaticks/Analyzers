using Automaticks.CSharp.CodeFixes.Formatting;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes;

/// <summary>
///     Tests for ConsecutiveBlankLinesCodeFixProvider.
///     Fixtures use escaped newlines because ATXCS044 counts raw file lines, so literal blank
///     lines inside a fixture would be reported against this test file itself.
/// </summary>
public class ConsecutiveBlankLinesCodeFixProviderTests
{
    private const string FourBlankLinesSource = "namespace MyApp {\n    public class Foo {\n        public void Bar() { }\n\n\n\n\n        public void Baz() { }\n    }\n}\n";
    private const string OneBlankLineSource = "namespace MyApp {\n    public class Foo {\n        public void Bar() { }\n\n        public void Baz() { }\n    }\n}\n";
    private const string TwoBlankLinesSource = "namespace MyApp {\n    public class Foo {\n        public void Bar() { }\n\n\n        public void Baz() { }\n    }\n}\n";

    /// <summary>
    ///     Tests that a long run of blank lines collapses until the rule is satisfied.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyAllFixes_LongBlankRun_CollapsesToSingleBlankLine(CancellationToken cancellationToken)
    {
        var analyzer = new ConsecutiveBlankLinesAnalyzer();
        var provider = new ConsecutiveBlankLinesCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = FourBlankLinesSource
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
        await Assert.That(fixedSource).Contains("public void Bar() { }");
        await Assert.That(fixedSource).Contains("public void Baz() { }");
    }

    /// <summary>
    ///     Tests that surrounding code survives the blank line removal.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_TwoBlankLines_KeepsSurroundingCode(CancellationToken cancellationToken)
    {
        var analyzer = new ConsecutiveBlankLinesAnalyzer();
        var provider = new ConsecutiveBlankLinesCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = TwoBlankLinesSource
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("namespace MyApp");
        await Assert.That(fixedSource).Contains("public void Bar() { }");
        await Assert.That(fixedSource).Contains("public void Baz() { }");
    }

    /// <summary>
    ///     Tests that two blank lines collapse to one.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_TwoBlankLines_LeavesNoDiagnostic(CancellationToken cancellationToken)
    {
        var analyzer = new ConsecutiveBlankLinesAnalyzer();
        var provider = new ConsecutiveBlankLinesCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = TwoBlankLinesSource
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
    ///     Tests that a single blank line is never offered a fix.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountFixable_SingleBlankLine_ReportsZero(CancellationToken cancellationToken)
    {
        var analyzer = new ConsecutiveBlankLinesAnalyzer();
        var provider = new ConsecutiveBlankLinesCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = OneBlankLineSource
        };
        var count = await CodeFixTestRunner.CountFixableAsync(request, cancellationToken);

        await Assert.That(count).IsEqualTo(0);
    }
}
