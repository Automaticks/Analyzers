using Automaticks.Diagnostics.CodeAnalysis.CodeFixes;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Diagnostics.CodeAnalysis.Analyzers.Tests;

/// <summary>Tests for SuppressMessageCodeFixProvider.</summary>
public class SuppressMessageCodeFixProviderTests
{
    /// <summary>Verifies repeated application clears every occurrence in the document.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyAllFixes_MultipleAttributes_RemovesEveryOccurrence(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Diagnostics.CodeAnalysis;
                              namespace MyApp {
                                  public class Foo {
                                      [SuppressMessage("Architecture", "ATXCS011")]
                                      [SuppressMessage("Architecture", "ATXCS021")]
                                      private static void Helper() { }
                                  }
                              }
                              """;

        var analyzer = new SuppressMessageAnalyzer();
        var provider = new SuppressMessageCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyAllFixesAsync(request, cancellationToken);

        await Assert.That(fixedSource).DoesNotContain("SuppressMessage");
    }

    /// <summary>Verifies a fully qualified attribute name is also removed.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_QualifiedAttributeName_RemovesAttribute(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      [System.Diagnostics.CodeAnalysis.SuppressMessage("Architecture", "ATXCS011")]
                                      private static void Helper() { }
                                  }
                              }
                              """;

        var analyzer = new SuppressMessageAnalyzer();
        var provider = new SuppressMessageCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).DoesNotContain("SuppressMessage");
    }

    /// <summary>Verifies only the flagged attribute is removed when the list holds several.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_SharedAttributeList_RemovesOnlyFlaggedAttribute(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              using System.Diagnostics.CodeAnalysis;
                              namespace MyApp {
                                  public class Foo {
                                      [Obsolete("x"), SuppressMessage("Architecture", "ATXCS011")]
                                      private static void Helper() { }
                                  }
                              }
                              """;

        var analyzer = new SuppressMessageAnalyzer();
        var provider = new SuppressMessageCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).DoesNotContain("SuppressMessage");
        await Assert.That(fixedSource).Contains("Obsolete(\"x\")");
    }

    /// <summary>Verifies the whole attribute list goes when it holds only the flagged attribute.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_SoleAttributeInList_RemovesWholeAttributeList(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Diagnostics.CodeAnalysis;
                              namespace MyApp {
                                  public class Foo {
                                      [SuppressMessage("Architecture", "ATXCS011")]
                                      private static void Helper() { }
                                  }
                              }
                              """;

        var analyzer = new SuppressMessageAnalyzer();
        var provider = new SuppressMessageCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).DoesNotContain("SuppressMessage");
        await Assert.That(fixedSource).Contains("private static void Helper() { }");
    }

    /// <summary>Verifies clean source offers no fix at all.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountFixable_CleanSource_ReportsZero(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() { }
                                  }
                              }
                              """;

        var analyzer = new SuppressMessageAnalyzer();
        var provider = new SuppressMessageCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var count = await CodeFixTestRunner.CountFixableAsync(request, cancellationToken);

        await Assert.That(count).IsEqualTo(0);
    }

    /// <summary>Verifies GetFixAllProvider returns a non-null batch fixer.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetFixAllProvider_Called_ReturnsBatchFixer(CancellationToken cancellationToken)
    {
        var provider = new SuppressMessageCodeFixProvider();
        var fixAllProvider = provider.GetFixAllProvider();

        await Assert.That(fixAllProvider).IsNotNull();
    }

    /// <summary>Verifies a diagnostic with no enclosing attribute registers no fix.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RegisterCodeFixes_DiagnosticWithoutEnclosingAttribute_RegistersNoFix(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() { }
                                  }
                              }
                              """;

        var analyzer = new SuppressMessageAtMethodAnalyzer();
        var provider = new SuppressMessageCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var count = await CodeFixTestRunner.CountRegisteredActionsAsync(request, cancellationToken);

        await Assert.That(count).IsEqualTo(0);
    }
}
