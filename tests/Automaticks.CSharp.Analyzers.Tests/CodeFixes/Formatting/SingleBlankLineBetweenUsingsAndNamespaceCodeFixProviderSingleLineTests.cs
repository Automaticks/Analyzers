using Automaticks.CSharp.CodeFixes.Formatting;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes.Formatting;

/// <summary>
///     Tests the namespace spacing fix against a source that holds no line break at all.
/// </summary>
public class SingleBlankLineBetweenUsingsAndNamespaceCodeFixProviderSingleLineTests
{
    private const string Source = "using System;namespace MyApp { public class Foo { } }";

    /// <summary>
    ///     Tests that the fix falls back to a line feed when the source has none to copy.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_SourceWithoutLineBreak_InsertsLineFeed(CancellationToken cancellationToken)
    {
        var analyzer = new SingleBlankLineBetweenUsingsAndNamespaceAnalyzer();
        var provider = new SingleBlankLineBetweenUsingsAndNamespaceCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = Source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("\n");
        await Assert.That(fixedSource).DoesNotContain("\r");
    }
}
