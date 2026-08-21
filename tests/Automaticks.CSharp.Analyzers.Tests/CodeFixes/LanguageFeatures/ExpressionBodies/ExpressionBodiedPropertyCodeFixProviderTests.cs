using Automaticks.CSharp.CodeFixes.LanguageFeatures.ExpressionBodies;
using Automaticks.CSharp.LanguageFeatures.ExpressionBodies;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes.LanguageFeatures.ExpressionBodies;

/// <summary>
///     Tests that ExpressionBodiedPropertyCodeFixProvider converts the expression body to a block.
/// </summary>
public class ExpressionBodiedPropertyCodeFixProviderTests
{
    /// <summary>
    ///     Tests that the arrow is replaced by a block body.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_ExpressionBody_ConvertsToBlock(CancellationToken cancellationToken)
    {
        const string source = "namespace MyApp {\n    public class Shape {\n        public int Size => 1;\n    }\n}\n";

        var analyzer = new ExpressionBodiedPropertyAnalyzer();
        var provider = new ExpressionBodiedPropertyCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("return 1;");
        await Assert.That(fixedSource).DoesNotContain("=>");
    }
}