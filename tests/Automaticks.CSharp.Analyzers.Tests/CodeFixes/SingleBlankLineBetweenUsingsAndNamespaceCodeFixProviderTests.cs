using Automaticks.CSharp.CodeFixes.Formatting;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes;

/// <summary>
///     Tests for SingleBlankLineBetweenUsingsAndNamespaceCodeFixProvider.
///     Fixtures use escaped newlines so the layout under test is not reported against this file.
/// </summary>
public class SingleBlankLineBetweenUsingsAndNamespaceCodeFixProviderTests
{
    private const string CrampedSource = "using System;\nnamespace MyApp {\n    public class Foo { }\n}\n";
    private const string SpacedSource = "using System;\n\nnamespace MyApp {\n    public class Foo { }\n}\n";
    private const string WindowsLineBreakCrampedSource = "using System;\r\nnamespace MyApp {\r\n    public class Foo { }\r\n}\r\n";

    /// <summary>
    ///     Tests that the using directive and namespace both survive the insertion.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_CrampedNamespace_KeepsSurroundingCode(CancellationToken cancellationToken)
    {
        var analyzer = new SingleBlankLineBetweenUsingsAndNamespaceAnalyzer();
        var provider = new SingleBlankLineBetweenUsingsAndNamespaceCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = CrampedSource
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("using System;");
        await Assert.That(fixedSource).Contains("namespace MyApp");
        await Assert.That(fixedSource).Contains("public class Foo { }");
    }

    /// <summary>
    ///     Tests that a blank line is inserted before the namespace declaration.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_CrampedNamespace_LeavesNoDiagnostic(CancellationToken cancellationToken)
    {
        var analyzer = new SingleBlankLineBetweenUsingsAndNamespaceAnalyzer();
        var provider = new SingleBlankLineBetweenUsingsAndNamespaceCodeFixProvider();
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
    ///     Tests that the inserted break matches the Windows line ending style of the file.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_WindowsLineBreakFile_InsertsMatchingLineBreak(CancellationToken cancellationToken)
    {
        var analyzer = new SingleBlankLineBetweenUsingsAndNamespaceAnalyzer();
        var provider = new SingleBlankLineBetweenUsingsAndNamespaceCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = WindowsLineBreakCrampedSource
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("using System;\r\n\r\nnamespace MyApp");
    }

    /// <summary>
    ///     Tests that a correctly spaced file is never offered a fix.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountFixable_SpacedNamespace_ReportsZero(CancellationToken cancellationToken)
    {
        var analyzer = new SingleBlankLineBetweenUsingsAndNamespaceAnalyzer();
        var provider = new SingleBlankLineBetweenUsingsAndNamespaceCodeFixProvider();
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
