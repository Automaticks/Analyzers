using Automaticks.CSharp.CodeFixes.LanguageFeatures;
using Microsoft.CodeAnalysis.CodeFixes;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes.LanguageFeatures;

/// <summary>
///     Tests for QualifiedTypeReferenceCodeFixProvider.
/// </summary>
public class QualifiedTypeReferenceCodeFixProviderTests
{
    /// <summary>
    ///     Tests that already-present usings keep their sorted order and no new using is duplicated.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_UsingAlreadyPresent_SimplifiesWithoutAddingUsing(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.IO;

                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                      public void Bar()
                                      {
                                          var exists = System.IO.File.Exists("p");
                                      }
                                  }
                              }
                              """;

        var analyzer = new QualifiedTypeReferenceAnalyzer();
        var provider = new QualifiedTypeReferenceCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);
        var usingCount = 0;
        var searchIndex = 0;
        while (true)
        {
            var found = fixedSource.IndexOf("using System.IO;", searchIndex, StringComparison.Ordinal);
            if (found < 0)
            {
                break;
            }

            usingCount++;
            searchIndex = found + 1;
        }

        await Assert.That(fixedSource).Contains("File.Exists(\"p\")");
        await Assert.That(fixedSource).DoesNotContain("System.IO.File");
        await Assert.That(usingCount).IsEqualTo(1);
    }

    /// <summary>
    ///     Tests that a missing using is added and the reference is simplified.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_UsingMissing_SimplifiesAndAddsUsingDirective(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                      public void Bar()
                                      {
                                          var exists = System.IO.File.Exists("p");
                                      }
                                  }
                              }
                              """;

        var analyzer = new QualifiedTypeReferenceAnalyzer();
        var provider = new QualifiedTypeReferenceCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("using System.IO;");
        await Assert.That(fixedSource).Contains("File.Exists(\"p\")");
        await Assert.That(fixedSource).DoesNotContain("System.IO.File");
    }

    /// <summary>
    ///     Tests that a missing using is inserted alphabetically among existing regular usings.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_UsingMissingAmongOtherUsings_InsertsInSortedPosition(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              using System.Text;

                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                      public void Bar()
                                      {
                                          var exists = System.IO.File.Exists("p");
                                      }
                                  }
                              }
                              """;

        var analyzer = new QualifiedTypeReferenceAnalyzer();
        var provider = new QualifiedTypeReferenceCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);
        var systemIndex = fixedSource.IndexOf("using System;", StringComparison.Ordinal);
        var ioIndex = fixedSource.IndexOf("using System.IO;", StringComparison.Ordinal);
        var textIndex = fixedSource.IndexOf("using System.Text;", StringComparison.Ordinal);

        await Assert.That(systemIndex).IsLessThan(ioIndex);
        await Assert.That(ioIndex).IsLessThan(textIndex);
    }

    /// <summary>
    ///     Tests that a collision diagnostic is never offered a fix.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountOfferedActions_ForCollidingTypeReference_ReportsZero(CancellationToken cancellationToken)
    {
        const string source = """
                              using MyApp;

                              namespace MyApp
                              {
                                  public class File
                                  {
                                  }
                              }

                              namespace Other
                              {
                                  public class Foo
                                  {
                                      public void Bar()
                                      {
                                          var x = System.IO.File.Exists("p");
                                      }
                                  }
                              }
                              """;

        var analyzer = new QualifiedTypeReferenceAnalyzer();
        var provider = new QualifiedTypeReferenceCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var offeredActions = await CodeFixTestRunner.CountOfferedActionsAsync(request, cancellationToken);

        await Assert.That(offeredActions).IsEqualTo(0);
    }

    /// <summary>
    ///     Tests that the provider always exposes the batch Fix All provider.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetFixAllProvider_Always_ReturnsBatchFixer(CancellationToken cancellationToken)
    {
        var provider = new QualifiedTypeReferenceCodeFixProvider();
        var fixAllProvider = provider.GetFixAllProvider();

        await Assert.That(fixAllProvider).IsEqualTo(WellKnownFixAllProviders.BatchFixer);
    }
}
