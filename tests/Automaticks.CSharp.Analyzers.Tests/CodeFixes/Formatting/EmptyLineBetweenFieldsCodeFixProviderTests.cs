using Automaticks.CSharp.CodeFixes.Formatting;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes.Formatting;

/// <summary>
///     Tests for EmptyLineBetweenFieldsCodeFixProvider.
///     Fixtures use escaped newlines so the blank lines under test cannot be reported
///     against this file by the very rule being exercised.
/// </summary>
public class EmptyLineBetweenFieldsCodeFixProviderTests
{
    private const string AdjacentFieldsSource = "namespace MyApp {\n    public class Foo {\n        private readonly int first;\n        private readonly int second;\n    }\n}\n";
    private const string SeparatedFieldsSource = "namespace MyApp {\n    public class Foo {\n        private readonly int first;\n\n        private readonly int second;\n    }\n}\n";
    private const string ThreeSeparatedFieldsSource = "namespace MyApp {\n    public class Foo {\n        private readonly int first;\n\n        private readonly int second;\n\n        private readonly int third;\n    }\n}\n";

    /// <summary>
    ///     Tests that repeated application closes every gap between adjacent fields.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyAllFixes_SeveralSeparatedFields_RemovesEveryBlankLine(CancellationToken cancellationToken)
    {
        var analyzer = new EmptyLineBetweenFieldsAnalyzer();
        var provider = new EmptyLineBetweenFieldsCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = ThreeSeparatedFieldsSource
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
        await Assert.That(fixedSource).Contains("private readonly int third;");
    }

    /// <summary>
    ///     Tests that both field declarations survive the blank line removal.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_SeparatedFields_KeepsBothFields(CancellationToken cancellationToken)
    {
        var analyzer = new EmptyLineBetweenFieldsAnalyzer();
        var provider = new EmptyLineBetweenFieldsCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = SeparatedFieldsSource
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("private readonly int first;");
        await Assert.That(fixedSource).Contains("private readonly int second;");
    }

    /// <summary>
    ///     Tests that the blank line between the fields is removed.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_SeparatedFields_LeavesNoDiagnostic(CancellationToken cancellationToken)
    {
        var analyzer = new EmptyLineBetweenFieldsAnalyzer();
        var provider = new EmptyLineBetweenFieldsCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = SeparatedFieldsSource
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
    ///     Tests that already adjacent fields are never offered a fix.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountFixable_AdjacentFields_ReportsZero(CancellationToken cancellationToken)
    {
        var analyzer = new EmptyLineBetweenFieldsAnalyzer();
        var provider = new EmptyLineBetweenFieldsCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = AdjacentFieldsSource
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
        var provider = new EmptyLineBetweenFieldsCodeFixProvider();
        var fixAllProvider = provider.GetFixAllProvider();

        await Assert.That(fixAllProvider).IsEqualTo(WellKnownFixAllProviders.BatchFixer);
    }
}
