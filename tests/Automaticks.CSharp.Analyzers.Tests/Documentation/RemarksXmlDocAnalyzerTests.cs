using Automaticks.CSharp;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Documentation;

/// <summary>
///     Tests for RemarksXmlDocAnalyzer.
/// </summary>
public class RemarksXmlDocAnalyzerTests
{

    /// <summary>
    ///     Tests that Analyze_MultipleRemarksElements_ReportsOneDiagnosticPerElement.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MultipleRemarksElements_ReportsOneDiagnosticPerElement(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      /// <summary>
                                      ///     Foo.
                                      /// </summary>
                                      /// <remarks>First.</remarks>
                                      /// <remarks>Second.</remarks>
                                      public void Bar() {}
                                  }
                              }
                              """;

        var analyzer = new RemarksXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.CountId(diagnostics, "ATXCS038")).IsEqualTo(2);
    }

    /// <summary>
    ///     Tests that Analyze_NoDocComment_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NoDocComment_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {}
                              }
                              """;

        var analyzer = new RemarksXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS038")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_RemarksElementOnClass_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_RemarksElementOnClass_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///     A class.
                                  /// </summary>
                                  /// <remarks>Extra notes.</remarks>
                                  public class Foo {}
                              }
                              """;

        var analyzer = new RemarksXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS038")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_RemarksElementOnMethod_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_RemarksElementOnMethod_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      /// <summary>
                                      ///     Does something.
                                      /// </summary>
                                      /// <remarks>Additional detail.</remarks>
                                      public void Bar() {}
                                  }
                              }
                              """;

        var analyzer = new RemarksXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS038")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_SelfClosingRemarks_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SelfClosingRemarks_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///     A class.
                                  /// </summary>
                                  /// <remarks/>
                                  public class Foo {}
                              }
                              """;

        var analyzer = new RemarksXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS038")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_SummaryAndParamOnly_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SummaryAndParamOnly_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      /// <summary>
                                      ///     Does something.
                                      /// </summary>
                                      /// <param name="value">The value.</param>
                                      public void Bar(int value) {}
                                  }
                              }
                              """;

        var analyzer = new RemarksXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS038")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_SummaryOnly_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SummaryOnly_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///     A class.
                                  /// </summary>
                                  public class Foo {}
                              }
                              """;

        var analyzer = new RemarksXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS038")).IsFalse();
    }
}
