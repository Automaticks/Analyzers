using Automaticks.CSharp.LanguageFeatures;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures.ExternalInterfaces;

/// <summary>
///     Tests how InlineFieldInitializerAnalyzer resolves primary constructor parameters.
/// </summary>
public class InlineFieldInitializerAnalyzerPrimaryConstructorTests
{
    /// <summary>
    ///     Tests that an identifier matching no primary constructor parameter is reported.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_IdentifierOutsidePrimaryConstructor_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Box(int width) {
                                      private static readonly int Fallback = 4;
                                      private int _size = Fallback;
                                  }
                              }
                              """;

        var analyzer = new InlineFieldInitializerAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.InlineFieldInitializer)).IsTrue();
    }

    /// <summary>
    ///     Tests that a primary constructor parameter keeps its initializer.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PrimaryConstructorParameter_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Box(int width) {
                                      private int _width = width;
                                  }
                              }
                              """;

        var analyzer = new InlineFieldInitializerAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.InlineFieldInitializer)).IsFalse();
    }

    /// <summary>
    ///     Tests that a type without a primary constructor still reports its initializers.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TypeWithoutPrimaryConstructor_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Shape {
                                      private static readonly int Fallback = 4;
                                      private int _size = Fallback;
                                  }
                              }
                              """;

        var analyzer = new InlineFieldInitializerAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.InlineFieldInitializer)).IsTrue();
    }
}
