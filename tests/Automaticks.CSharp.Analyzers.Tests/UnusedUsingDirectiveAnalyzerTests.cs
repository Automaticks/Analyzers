using Automaticks.CSharp;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

/// <summary>
///     Tests for UnusedUsingDirectiveAnalyzer.
/// </summary>
public class UnusedUsingDirectiveAnalyzerTests
{

    /// <summary>
    ///     Tests that Analyze_AliasUsingNotChecked_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AliasUsingNotChecked_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using X = System.Text.StringBuilder;

                              namespace MyApp;
                              public class Foo { }
                              """;

        var analyzer = new UnusedUsingDirectiveAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS048")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_GlobalUsingNotChecked_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_GlobalUsingNotChecked_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = "global using System.Text;\nnamespace MyApp;\npublic class Foo { }";

        var analyzer = new UnusedUsingDirectiveAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS048")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_NoUsings_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NoUsings_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp;
                              public class Foo { }
                              """;

        var analyzer = new UnusedUsingDirectiveAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS048")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_StaticUsingNotChecked_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StaticUsingNotChecked_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using static System.Math;

                              namespace MyApp;
                              public class Foo { }
                              """;

        var analyzer = new UnusedUsingDirectiveAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS048")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_UnusedRegularUsing_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_UnusedRegularUsing_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Text;

                              namespace MyApp;
                              public class Foo { }
                              """;

        var analyzer = new UnusedUsingDirectiveAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS048")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_UsedRegularUsing_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_UsedRegularUsing_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Text;

                              namespace MyApp;
                              public class Foo {
                                  public System.Text.StringBuilder Builder { get; set; }
                              }
                              """;

        var analyzer = new UnusedUsingDirectiveAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS048")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_UsedViaSimpleTypeName_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_UsedViaSimpleTypeName_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections.Generic;

                              namespace MyApp;
                              public class Foo {
                                  public List<int> Items { get; set; }
                              }
                              """;

        var analyzer = new UnusedUsingDirectiveAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS048")).IsFalse();
    }
}
