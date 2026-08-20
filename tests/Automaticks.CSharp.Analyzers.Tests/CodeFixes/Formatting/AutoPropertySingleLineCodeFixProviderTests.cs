using Automaticks.CSharp.CodeFixes.Formatting;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes.Formatting;

/// <summary>
///     Tests for AutoPropertySingleLineCodeFixProvider.
///     Fixtures use escaped newlines so the layout under test is not reported against this file.
/// </summary>
public class AutoPropertySingleLineCodeFixProviderTests
{
    private const string AttributedMultiLineSource = "using System;\nnamespace MyApp {\n    public class Foo {\n        [Obsolete(\"x\")]\n        public string Name\n        {\n            get;\n            set;\n        }\n    }\n}\n";
    private const string MultiLineSource = "namespace MyApp {\n    public class Foo {\n        public string Name\n        {\n            get;\n            set;\n        }\n    }\n}\n";
    private const string NoModifierMultiLineSource = "namespace MyApp {\n    public class Foo {\n        string Name\n        {\n            get;\n            set;\n        }\n    }\n}\n";
    private const string SingleLineSource = "namespace MyApp {\n    public class Foo {\n        public string Name { get; set; }\n    }\n}\n";
    private const string TwoMultiLineSource = "namespace MyApp {\n    public class Foo {\n        public string Name\n        {\n            get;\n            set;\n        }\n\n        public int Count\n        {\n            get;\n            init;\n        }\n    }\n}\n";
    [Test]
    public async Task ApplyAllFixes_SeveralMultiLineProperties_CollapsesEveryOne(CancellationToken cancellationToken)
    {
        var analyzer = new AutoPropertySingleLineAnalyzer();
        var provider = new AutoPropertySingleLineCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = TwoMultiLineSource
        };
        var fixedSource = await CodeFixTestRunner.ApplyAllFixesAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("public string Name { get; set; }");
        await Assert.That(fixedSource).Contains("public int Count { get; init; }");
    }

    /// <summary>
    ///     Tests that an attribute on its own line is left where it is.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_AttributedProperty_KeepsAttributeOnItsOwnLine(CancellationToken cancellationToken)
    {
        var analyzer = new AutoPropertySingleLineAnalyzer();
        var provider = new AutoPropertySingleLineCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = AttributedMultiLineSource
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("[Obsolete(\"x\")]\n        public string Name { get; set; }");
    }

    /// <summary>
    ///     Tests that the property collapses onto a single line.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_MultiLineProperty_CollapsesToOneLine(CancellationToken cancellationToken)
    {
        var analyzer = new AutoPropertySingleLineAnalyzer();
        var provider = new AutoPropertySingleLineCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = MultiLineSource
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("public string Name { get; set; }");
    }

    /// <summary>
    ///     Tests that repeated application collapses every multi-line auto-property.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    /// <summary>
    ///     Tests that the collapsed property keeps its indentation and no longer reports.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_MultiLineProperty_LeavesNoDiagnostic(CancellationToken cancellationToken)
    {
        var analyzer = new AutoPropertySingleLineAnalyzer();
        var provider = new AutoPropertySingleLineCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = MultiLineSource
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
        await Assert.That(fixedSource).Contains("        public string Name");
    }

    /// <summary>
    ///     Tests that a property without modifiers is collapsed from its first token.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_PropertyWithoutModifiers_CollapsesToSingleLine(CancellationToken cancellationToken)
    {
        var analyzer = new AutoPropertySingleLineAnalyzer();
        var provider = new AutoPropertySingleLineCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = NoModifierMultiLineSource
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("string Name { get; set; }");
    }

    /// <summary>
    ///     Tests that a property already on one line is never offered a fix.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountFixable_SingleLineProperty_ReportsZero(CancellationToken cancellationToken)
    {
        var analyzer = new AutoPropertySingleLineAnalyzer();
        var provider = new AutoPropertySingleLineCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = SingleLineSource
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
        var provider = new AutoPropertySingleLineCodeFixProvider();
        var fixAllProvider = provider.GetFixAllProvider();

        await Assert.That(fixAllProvider).IsEqualTo(WellKnownFixAllProviders.BatchFixer);
    }
}
