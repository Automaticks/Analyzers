using Automaticks.CSharp.CodeFixes.Documentation;
using Microsoft.CodeAnalysis.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes.Documentation;

/// <summary>
///     Tests for RemarksXmlDocCodeFixProvider.
/// </summary>
public class RemarksXmlDocCodeFixProviderTests
{
    /// <summary>
    ///     Tests that repeated application clears every remarks element.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyAllFixes_SeveralRemarks_RemovesEveryOne(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///     Does a thing.
                                  /// </summary>
                                  /// <remarks>First note.</remarks>
                                  public class Foo {
                                      /// <summary>
                                      ///     Does another thing.
                                      /// </summary>
                                      /// <remarks>Second note.</remarks>
                                      public void Bar() { }
                                  }
                              }
                              """;

        var analyzer = new RemarksXmlDocAnalyzer();
        var provider = new RemarksXmlDocCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyAllFixesAsync(request, cancellationToken);

        await Assert.That(fixedSource).DoesNotContain("<remarks>");
    }

    /// <summary>
    ///     Tests that a remarks element with no preceding summary is removed.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_RemarksAsFirstElement_RemovesElement(CancellationToken cancellationToken)
    {
        const string source = "namespace MyApp {\n    ///<remarks>Only remarks, no summary.</remarks>\n    public class Foo { }\n}\n";

        var analyzer = new RemarksXmlDocAnalyzer();
        var provider = new RemarksXmlDocCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).DoesNotContain("remarks");
    }

    /// <summary>
    ///     Tests that the remarks element is removed while the summary survives.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_RemarksElement_KeepsSummary(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///     Does a thing.
                                  /// </summary>
                                  /// <remarks>A note.</remarks>
                                  public class Foo { }
                              }
                              """;

        var analyzer = new RemarksXmlDocAnalyzer();
        var provider = new RemarksXmlDocCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).DoesNotContain("remarks");
        await Assert.That(fixedSource).Contains("Does a thing.");
    }

    /// <summary>
    ///     Tests that a remarks element immediately following a summary with no text between is removed.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_RemarksImmediatelyAfterSummary_RemovesElement(CancellationToken cancellationToken)
    {
        const string source = "namespace MyApp {\n    /// <summary>x</summary><remarks>y</remarks>\n    public class Foo { }\n}\n";

        var analyzer = new RemarksXmlDocAnalyzer();
        var provider = new RemarksXmlDocCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).DoesNotContain("remarks");
        await Assert.That(fixedSource).Contains("<summary>x</summary>");
    }

    /// <summary>
    ///     Tests that a self-closing remarks element is removed.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_SelfClosingRemarks_RemovesElement(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///     Does a thing.
                                  /// </summary>
                                  /// <remarks />
                                  public class Foo { }
                              }
                              """;

        var analyzer = new RemarksXmlDocAnalyzer();
        var provider = new RemarksXmlDocCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).DoesNotContain("remarks");
    }

    /// <summary>
    ///     Tests that a remarks element nested inside another element is left unchanged.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFixForSpan_RemarksNestedInsideAnotherElement_KeepsSourceUnchanged(CancellationToken cancellationToken)
    {
        const string source = "namespace MyApp {\n    /// <summary><remarks>nested</remarks></summary>\n    public class Foo { }\n}\n";

        var analyzer = new RemarksXmlDocAnalyzer();
        var provider = new RemarksXmlDocCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var start = source.IndexOf("<remarks>", System.StringComparison.Ordinal) + 1;
        var span = new TextSpan(start, "remarks".Length);
        var fixedSource = await CodeFixTestRunner.ApplyFixForSpanAsync(
            request,
            RemarksXmlDocAnalyzer.Rule,
            span,
            cancellationToken);

        await Assert.That(fixedSource).IsEqualTo(source);
    }

    /// <summary>
    ///     Tests that a span with no enclosing remarks element offers no fix.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountActionsForSpan_NoEnclosingRemarksElement_ReportsZero(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                              }
                              """;

        var analyzer = new RemarksXmlDocAnalyzer();
        var provider = new RemarksXmlDocCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var start = source.IndexOf("System", System.StringComparison.Ordinal);
        var span = new TextSpan(start, "System".Length);
        var count = await CodeFixTestRunner.CountActionsForSpanAsync(
            request,
            RemarksXmlDocAnalyzer.Rule,
            span,
            cancellationToken);

        await Assert.That(count).IsEqualTo(0);
    }

    /// <summary>
    ///     Tests that documentation without remarks offers no fix.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountFixable_NoRemarks_ReportsZero(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///     Does a thing.
                                  /// </summary>
                                  public class Foo { }
                              }
                              """;

        var analyzer = new RemarksXmlDocAnalyzer();
        var provider = new RemarksXmlDocCodeFixProvider();
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
