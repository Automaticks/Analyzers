using Automaticks.CSharp.CodeFixes.LanguageFeatures.ExpressionBodies;
using Automaticks.CSharp.LanguageFeatures.ExpressionBodies;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes.LanguageFeatures.ExpressionBodies;

/// <summary>
///     Tests that ExpressionBodiedConstructorCodeFixProvider converts the expression body to a block.
/// </summary>
public class ExpressionBodiedConstructorCodeFixProviderTests
{
    /// <summary>
    ///     Tests that the arrow is replaced by a block body.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_ExpressionBody_ConvertsToBlock(CancellationToken cancellationToken)
    {
        const string source = "namespace MyApp {\n    public class Shape {\n        private int _size;\n        public Shape() => _size = 1;\n    }\n}\n";

        var analyzer = new ExpressionBodiedConstructorAnalyzer();
        var provider = new ExpressionBodiedConstructorCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("_size = 1;");
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
        var analyzer = new ExpressionBodiedConstructorAnalyzer();
        var provider = new ExpressionBodiedConstructorCodeFixProvider();
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