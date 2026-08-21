using Automaticks.CSharp.CodeFixes.LanguageFeatures.ExpressionBodies;
using Automaticks.CSharp.LanguageFeatures.ExpressionBodies;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes.LanguageFeatures.ExpressionBodies;

/// <summary>
///     Tests that ExpressionBodiedIndexerCodeFixProvider converts the expression body to a block.
/// </summary>
public class ExpressionBodiedIndexerCodeFixProviderTests
{
    /// <summary>
    ///     Tests that the arrow is replaced by a block body.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_ExpressionBody_ConvertsToBlock(CancellationToken cancellationToken)
    {
        const string source = "namespace MyApp {\n    public class Shape {\n        public int this[int index] => index;\n    }\n}\n";

        var analyzer = new ExpressionBodiedIndexerAnalyzer();
        var provider = new ExpressionBodiedIndexerCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("return index;");
        await Assert.That(fixedSource).DoesNotContain("=>");
    }
}