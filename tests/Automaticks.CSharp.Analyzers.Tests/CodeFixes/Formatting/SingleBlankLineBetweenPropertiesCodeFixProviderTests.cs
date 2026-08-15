using Automaticks.CSharp.CodeFixes.Formatting;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes.Formatting;

/// <summary>
///     Tests for SingleBlankLineBetweenPropertiesCodeFixProvider.
///     Fixtures use escaped newlines so the layout under test is not reported against this file.
/// </summary>
public class SingleBlankLineBetweenPropertiesCodeFixProviderTests
{
    private const string CrampedSource = "namespace MyApp {\n    public class Foo {\n        public int Size { get; set; }\n        public int Weight { get; set; }\n    }\n}\n";
    private const string DocumentedCrampedSource = "namespace MyApp {\n    public class Foo {\n        public int Size { get; set; }\n        /// <summary>\n        ///     The weight.\n        /// </summary>\n        public int Weight { get; set; }\n    }\n}\n";
    private const string SpacedSource = "namespace MyApp {\n    public class Foo {\n        public int Size { get; set; }\n\n        public int Weight { get; set; }\n    }\n}\n";

    /// <summary>
    ///     Tests that both members survive the insertion.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_CrampedMembers_KeepsBothMembers(CancellationToken cancellationToken)
    {
        var analyzer = new SingleBlankLineBetweenPropertiesAnalyzer();
        var provider = new SingleBlankLineBetweenPropertiesCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = CrampedSource
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("public int Size { get; set; }");
        await Assert.That(fixedSource).Contains("public int Weight { get; set; }");
    }

    /// <summary>
    ///     Tests that the cramped members gain a separating blank line.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_CrampedMembers_LeavesNoDiagnostic(CancellationToken cancellationToken)
    {
        var analyzer = new SingleBlankLineBetweenPropertiesAnalyzer();
        var provider = new SingleBlankLineBetweenPropertiesCodeFixProvider();
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
    ///     Tests that the blank line lands above a documentation block rather than inside it.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_DocumentedMember_InsertsBlankLineAboveDocumentation(CancellationToken cancellationToken)
    {
        var analyzer = new SingleBlankLineBetweenPropertiesAnalyzer();
        var provider = new SingleBlankLineBetweenPropertiesCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = DocumentedCrampedSource
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("public int Size { get; set; }\n\n        /// <summary>");
    }

    /// <summary>
    ///     Tests that correctly spaced members are never offered a fix.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountFixable_SpacedMembers_ReportsZero(CancellationToken cancellationToken)
    {
        var analyzer = new SingleBlankLineBetweenPropertiesAnalyzer();
        var provider = new SingleBlankLineBetweenPropertiesCodeFixProvider();
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
