using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Linq.Analyzers.Tests;

/// <summary>
///     Tests for LinqUsageAnalyzer.
/// </summary>
public class LinqUsageAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_LinqSubNamespaceUsingDirective_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LinqSubNamespaceUsingDirective_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Linq.SomeSubNamespace;
                              namespace MyApp {
                                  public class Foo {}
                              }
                              """;

        var analyzer = new LinqUsageAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXLQ002")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_LinqUsingDirective_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LinqUsingDirective_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Linq;
                              namespace MyApp {
                                  public class Foo {}
                              }
                              """;

        var analyzer = new LinqUsageAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXLQ002")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_NoLinqUsing_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NoLinqUsing_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Foo {}
                              }
                              """;

        var analyzer = new LinqUsageAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXLQ002")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_SystemLinqExpressionsUsingDirective_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SystemLinqExpressionsUsingDirective_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Linq.Expressions;
                              namespace MyApp {
                                  public class Foo {}
                              }
                              """;

        var analyzer = new LinqUsageAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXLQ002")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_TypeAliasUsingDirective_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TypeAliasUsingDirective_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using MyIntArrayAlias = int[];
                              namespace MyApp {
                                  public class Foo {}
                              }
                              """;

        var analyzer = new LinqUsageAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXLQ002")).IsFalse();
    }
}
