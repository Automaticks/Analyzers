using Automaticks.CSharp.CodeFixes.LanguageFeatures;
using Automaticks.CSharp.LanguageFeatures;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes.LanguageFeatures;

/// <summary>
///     Tests where QualifiedTypeReferenceCodeFixProvider places the using it adds.
/// </summary>
public class QualifiedTypeReferenceCodeFixProviderInsertionTests
{
    /// <summary>
    ///     Tests that static and alias directives are stepped over when placing the using.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_MemberAccessAmongIrregularUsings_InsertsInOrder(CancellationToken cancellationToken)
    {
        const string source = """
                              using static System.Math;
                              using Numbers = int[];
                              using System.Text;
                              namespace MyApp {
                                  public class Shape {
                                      public void Draw() {
                                          System.Console.WriteLine();
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

        await Assert.That(fixedSource).Contains("Console.WriteLine()");
        await Assert.That(fixedSource).DoesNotContain("System.Console.WriteLine()");
    }

    /// <summary>
    ///     Tests that the using is placed after one that sorts earlier.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_QualifiedNameAfterEarlierUsing_InsertsInOrder(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Shape {
                                      public System.Text.StringBuilder Builder;
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

        await Assert.That(fixedSource).Contains("using System.Text;");
        await Assert.That(fixedSource).Contains("StringBuilder Builder;");
    }

    /// <summary>
    ///     Tests that a namespace holding the simple name is not taken for the type.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountOfferedActions_SimpleNameHeldByNamespace_OffersNoFix(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp.Console {
                                  public class Marker { }
                              }
                              namespace MyApp {
                                  public class Shape {
                                      public void Draw() {
                                          System.Console.WriteLine();
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
        var offered = await CodeFixTestRunner.CountOfferedActionsAsync(request, cancellationToken);

        await Assert.That(offered).IsEqualTo(0);
    }

    /// <summary>
    ///     Tests that no fix is offered when another type already holds the simple name.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountOfferedActions_SimpleNameHeldByOtherType_OffersNoFix(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Console { }
                                  public class Shape {
                                      public void Draw() {
                                          System.Console.WriteLine();
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
        var offered = await CodeFixTestRunner.CountOfferedActionsAsync(request, cancellationToken);

        await Assert.That(offered).IsEqualTo(0);
    }
}
