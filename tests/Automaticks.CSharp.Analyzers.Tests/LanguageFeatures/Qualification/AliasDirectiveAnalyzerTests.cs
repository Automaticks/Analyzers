using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures.Qualification;

/// <summary>
///     Tests for AliasDirectiveAnalyzer.
/// </summary>
public class AliasDirectiveAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_ExternAlias_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExternAlias_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              extern alias LegacyLib;

                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                  }
                              }
                              """;

        var analyzer = new AliasDirectiveAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(diagnostics, "ATXCS074", "LegacyLib")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_RegularUsingDirective_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_RegularUsingDirective_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.IO;

                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                  }
                              }
                              """;

        var analyzer = new AliasDirectiveAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS074")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_UsingNamespaceAlias_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_UsingNamespaceAlias_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using IO = System.IO;

                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                  }
                              }
                              """;

        var analyzer = new AliasDirectiveAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(diagnostics, "ATXCS074", "IO")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_UsingStaticDirective_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_UsingStaticDirective_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using static System.Math;

                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                  }
                              }
                              """;

        var analyzer = new AliasDirectiveAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS074")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_UsingTypeAlias_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_UsingTypeAlias_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using MyFile = System.IO.File;

                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                  }
                              }
                              """;

        var analyzer = new AliasDirectiveAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(diagnostics, "ATXCS074", "MyFile")).IsTrue();
    }
}
