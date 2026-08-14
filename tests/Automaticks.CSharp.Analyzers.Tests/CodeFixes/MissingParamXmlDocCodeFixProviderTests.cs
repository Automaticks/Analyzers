using Automaticks.CSharp.CodeFixes.Documentation;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes;

/// <summary>
///     Tests for MissingParamXmlDocCodeFixProvider.
/// </summary>
public class MissingParamXmlDocCodeFixProviderTests
{
    /// <summary>
    ///     Tests that repeated application documents every parameter.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyAllFixes_SeveralParameters_DocumentsEveryOne(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      /// <summary>
                                      ///     Does a thing.
                                      /// </summary>
                                      public void Bar(int size, string name) { }
                                  }
                              }
                              """;

        var analyzer = new MissingParamXmlDocAnalyzer();
        var provider = new MissingParamXmlDocCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyAllFixesAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("<param name=\"size\"></param>");
        await Assert.That(fixedSource).Contains("<param name=\"name\"></param>");
    }

    /// <summary>
    ///     Tests that a member with no documentation still gains the element.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_MemberWithoutDocumentation_AddsElement(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(int size) { }
                                  }
                              }
                              """;

        var analyzer = new MissingParamXmlDocAnalyzer();
        var provider = new MissingParamXmlDocCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("/// <param name=\"size\"></param>");
        await Assert.That(fixedSource).Contains("public void Bar(int size) { }");
    }

    /// <summary>
    ///     Tests that the element is appended after an existing summary block.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_MemberWithSummary_AppendsAfterSummary(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      /// <summary>
                                      ///     Does a thing.
                                      /// </summary>
                                      public void Bar(int size) { }
                                  }
                              }
                              """;

        var analyzer = new MissingParamXmlDocAnalyzer();
        var provider = new MissingParamXmlDocCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);
        var summaryEnd = fixedSource.IndexOf("</summary>", System.StringComparison.Ordinal);
        var paramIndex = fixedSource.IndexOf("<param", System.StringComparison.Ordinal);
        var memberIndex = fixedSource.IndexOf("public void Bar", System.StringComparison.Ordinal);

        await Assert.That(summaryEnd).IsLessThan(paramIndex);
        await Assert.That(paramIndex).IsLessThan(memberIndex);
    }

    /// <summary>
    ///     Tests that the fixed source no longer reports the rule.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_MemberWithSummary_LeavesNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      /// <summary>
                                      ///     Does a thing.
                                      /// </summary>
                                      public void Bar(int size) { }
                                  }
                              }
                              """;

        var analyzer = new MissingParamXmlDocAnalyzer();
        var provider = new MissingParamXmlDocCodeFixProvider();
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
    ///     Tests that a fully documented member is never offered a fix.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountFixable_DocumentedParameter_ReportsZero(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      /// <summary>
                                      ///     Does a thing.
                                      /// </summary>
                                      /// <param name="size">The size.</param>
                                      public void Bar(int size) { }
                                  }
                              }
                              """;

        var analyzer = new MissingParamXmlDocAnalyzer();
        var provider = new MissingParamXmlDocCodeFixProvider();
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
