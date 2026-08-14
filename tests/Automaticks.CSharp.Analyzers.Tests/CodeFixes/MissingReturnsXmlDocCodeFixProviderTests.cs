using Automaticks.CSharp.CodeFixes.Documentation;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes;

/// <summary>
///     Tests for MissingReturnsXmlDocCodeFixProvider.
/// </summary>
public class MissingReturnsXmlDocCodeFixProviderTests
{
    /// <summary>
    ///     Tests that repeated application documents every method.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyAllFixes_SeveralMethods_DocumentsEveryOne(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      /// <summary>
                                      ///     Does a thing.
                                      /// </summary>
                                      public int Bar() { return 0; }

                                      /// <summary>
                                      ///     Does another thing.
                                      /// </summary>
                                      public string Baz() { return string.Empty; }
                                  }
                              }
                              """;

        var analyzer = new MissingReturnsXmlDocAnalyzer();
        var provider = new MissingReturnsXmlDocCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
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
    }

    /// <summary>
    ///     Tests that the element is appended after an existing summary block.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_MethodWithSummary_AppendsAfterSummary(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      /// <summary>
                                      ///     Does a thing.
                                      /// </summary>
                                      public int Bar() { return 0; }
                                  }
                              }
                              """;

        var analyzer = new MissingReturnsXmlDocAnalyzer();
        var provider = new MissingReturnsXmlDocCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);
        var summaryEnd = fixedSource.IndexOf("</summary>", System.StringComparison.Ordinal);
        var returnsIndex = fixedSource.IndexOf("<returns>", System.StringComparison.Ordinal);
        var memberIndex = fixedSource.IndexOf("public int Bar", System.StringComparison.Ordinal);

        await Assert.That(summaryEnd).IsLessThan(returnsIndex);
        await Assert.That(returnsIndex).IsLessThan(memberIndex);
    }

    /// <summary>
    ///     Tests that the fixed source no longer reports the rule.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_MethodWithSummary_LeavesNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      /// <summary>
                                      ///     Does a thing.
                                      /// </summary>
                                      public int Bar() { return 0; }
                                  }
                              }
                              """;

        var analyzer = new MissingReturnsXmlDocAnalyzer();
        var provider = new MissingReturnsXmlDocCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
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
    ///     Tests that a void method is never offered a fix.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountFixable_VoidMethod_ReportsZero(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      /// <summary>
                                      ///     Does a thing.
                                      /// </summary>
                                      public void Bar() { }
                                  }
                              }
                              """;

        var analyzer = new MissingReturnsXmlDocAnalyzer();
        var provider = new MissingReturnsXmlDocCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var count = await CodeFixTestRunner.CountFixableAsync(request, cancellationToken);

        await Assert.That(count).IsEqualTo(0);
    }
}
