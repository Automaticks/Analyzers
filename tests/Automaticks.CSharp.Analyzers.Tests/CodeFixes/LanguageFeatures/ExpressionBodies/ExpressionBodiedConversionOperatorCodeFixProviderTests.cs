using Automaticks.CSharp.CodeFixes.LanguageFeatures.ExpressionBodies;
using Automaticks.CSharp.LanguageFeatures.ExpressionBodies;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes.LanguageFeatures.ExpressionBodies;

/// <summary>
///     Tests that ExpressionBodiedConversionOperatorCodeFixProvider converts the expression body to a block.
/// </summary>
public class ExpressionBodiedConversionOperatorCodeFixProviderTests
{
    /// <summary>
    ///     Tests that the arrow is replaced by a block body.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_ExpressionBody_ConvertsToBlock(CancellationToken cancellationToken)
    {
        const string source = "namespace MyApp {\n    public class Shape {\n        public static explicit operator int(Shape value) => 0;\n    }\n}\n";

        var analyzer = new ExpressionBodiedConversionOperatorAnalyzer();
        var provider = new ExpressionBodiedConversionOperatorCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("return 0;");
        await Assert.That(fixedSource).DoesNotContain("=>");
    }

    /// <summary>
    ///     Tests that the provider advertises the document Fix All scope.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetFixAllProvider_Always_SupportsDocumentScope(CancellationToken cancellationToken)
    {
        var analyzer = new ExpressionBodiedConversionOperatorAnalyzer();
        var provider = new ExpressionBodiedConversionOperatorCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = "namespace MyApp { }"
        };
        var scopes = CodeFixTestRunner.GetSupportedFixAllScopes(request);

        await Assert.That(scopes).Contains(FixAllScope.Document);
    }
}