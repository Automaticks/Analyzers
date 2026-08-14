using Automaticks.CSharp.CodeFixes.Documentation;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes;

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
