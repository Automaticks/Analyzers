using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures.Qualification;

/// <summary>
///     Tests for GlobalAliasQualifierAnalyzer.
/// </summary>
public class GlobalAliasQualifierAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_ExternAliasQualifier_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExternAliasQualifier_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              extern alias LegacyLib;

                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                      public void Bar()
                                      {
                                          var t = typeof(LegacyLib::SomeType);
                                      }
                                  }
                              }
                              """;

        var analyzer = new GlobalAliasQualifierAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS073")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_GlobalAliasQualifier_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_GlobalAliasQualifier_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                      public void Bar(string path)
                                      {
                                          var f = new global::System.IO.FileInfo(path);
                                      }
                                  }
                              }
                              """;

        var analyzer = new GlobalAliasQualifierAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS073")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_NoAliasQualifier_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NoAliasQualifier_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                      public void Bar(string path)
                                      {
                                          var f = new System.IO.FileInfo(path);
                                      }
                                  }
                              }
                              """;

        var analyzer = new GlobalAliasQualifierAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS073")).IsFalse();
    }
}
