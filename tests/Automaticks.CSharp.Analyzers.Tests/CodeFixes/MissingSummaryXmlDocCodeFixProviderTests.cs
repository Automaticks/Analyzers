using Automaticks.CSharp.CodeFixes.Documentation;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes;

/// <summary>
///     Tests for MissingSummaryXmlDocCodeFixProvider.
/// </summary>
public class MissingSummaryXmlDocCodeFixProviderTests
{
    /// <summary>
    ///     Tests that repeated application documents every undocumented member.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyAllFixes_SeveralUndocumentedMembers_DocumentsEveryOne(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() { }

                                      public void Baz() { }
                                  }
                              }
                              """;

        var analyzer = new MissingSummaryXmlDocAnalyzer();
        var provider = new MissingSummaryXmlDocCodeFixProvider();
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
    ///     Tests that the inserted block sits above an attribute list.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_AttributedMember_InsertsBlockAboveAttribute(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      [Obsolete("x")]
                                      public void Bar() { }
                                  }
                              }
                              """;

        var analyzer = new MissingSummaryXmlDocAnalyzer();
        var provider = new MissingSummaryXmlDocCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);
        var summaryIndex = fixedSource.IndexOf("/// <summary>", System.StringComparison.Ordinal);
        var attributeIndex = fixedSource.IndexOf("[Obsolete", System.StringComparison.Ordinal);

        await Assert.That(summaryIndex).IsLessThan(attributeIndex);
    }

    /// <summary>
    ///     Tests that an empty summary block is inserted above the member.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_UndocumentedMethod_InsertsEmptySummary(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() { }
                                  }
                              }
                              """;

        var analyzer = new MissingSummaryXmlDocAnalyzer();
        var provider = new MissingSummaryXmlDocCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("    /// <summary>");
        await Assert.That(fixedSource).Contains("    /// </summary>");
        await Assert.That(fixedSource).Contains("    public class Foo");
    }

    /// <summary>
    ///     Tests that the inserted block satisfies the summary formatting rule.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_UndocumentedMethod_SatisfiesSummaryFormatRule(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() { }
                                  }
                              }
                              """;

        var analyzer = new MissingSummaryXmlDocAnalyzer();
        var provider = new MissingSummaryXmlDocCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);
        var formatAnalyzer = new SummaryXmlDocFormatAnalyzer();
        var formatDiagnostics = await AnalyzerTestRunner.AnalyzeAsync(formatAnalyzer, fixedSource, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(formatDiagnostics, "ATXCS050")).IsFalse();
    }

    /// <summary>
    ///     Tests that a documented member is never offered a fix.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountFixable_DocumentedMethod_ReportsZero(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///     A thing.
                                  /// </summary>
                                  public class Foo {
                                      /// <summary>
                                      ///     Does a thing.
                                      /// </summary>
                                      public void Bar() { }
                                  }
                              }
                              """;

        var analyzer = new MissingSummaryXmlDocAnalyzer();
        var provider = new MissingSummaryXmlDocCodeFixProvider();
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
