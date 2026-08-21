using Automaticks.CSharp.LanguageFeatures;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures.Qualification;

/// <summary>
///     Tests QualifiedTypeReferenceAnalyzer when another type holds the simple name.
/// </summary>
public class QualifiedTypeReferenceAnalyzerShadowedNameTests
{
    /// <summary>
    ///     Tests that a namespace sharing the simple name is not mistaken for the type.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SimpleNameHeldByNamespace_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp.Console { }
                              namespace MyApp {
                                  public class Shape {
                                      public void Draw() {
                                          System.Console.WriteLine();
                                      }
                                  }
                              }
                              """;

        var analyzer = new QualifiedTypeReferenceAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.NamespaceQualifiedTypeReference)).IsTrue();
    }

    /// <summary>
    ///     Tests that a different type sharing the simple name is not mistaken for it.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SimpleNameHeldByOtherType_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Shape {
                                      public void Draw() {
                                          var Console = 1;
                                          System.Console.WriteLine(Console);
                                      }
                                  }
                              }
                              """;

        var analyzer = new QualifiedTypeReferenceAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.NamespaceQualifiedTypeReference)).IsTrue();
    }

    /// <summary>
    ///     Tests a qualified type whose simple name is not visible at all.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SimpleNameNotVisible_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Shape {
                                      public void Draw() {
                                          var builder = new System.Text.StringBuilder();
                                      }
                                  }
                              }
                              """;

        var analyzer = new QualifiedTypeReferenceAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.NamespaceQualifiedTypeReference)).IsTrue();
    }
}
