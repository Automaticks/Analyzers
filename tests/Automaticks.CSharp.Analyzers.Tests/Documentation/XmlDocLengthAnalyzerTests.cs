using Automaticks.CSharp.Documentation;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Documentation;

/// <summary>
///     Tests for XmlDocLengthAnalyzer.
/// </summary>
public class XmlDocLengthAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_ConfiguredLimitRaised_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConfiguredLimitRaised_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        var body = new string('a', 200);
        var source = BuildSource("summary", body);
        var options = new Dictionary<string, string>
        {
            ["automaticks.xml_doc_max_length"] = "500",
        };

        var diagnostics = await AnalyzeAsync(source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS071")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_LongParamDocumentation_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LongParamDocumentation_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp;
                              public class Foo {
                                  /// <summary>Short.</summary>
                                  /// <param name="value">aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa</param>
                                  public void Run(int value) { }
                              }
                              """;

        var diagnostics = await AnalyzeAsync(source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS071")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_LongSummaryOnOneLine_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LongSummaryOnOneLine_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        var body = new string('a', 200);
        var source = BuildSource("summary", body);

        var diagnostics = await AnalyzeAsync(source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS071")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_LongSummarySplitOverManyLines_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LongSummarySplitOverManyLines_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp;
                              /// <summary>
                              ///     aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
                              ///     bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb
                              ///     cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc
                              /// </summary>
                              public class Foo { }
                              """;

        var diagnostics = await AnalyzeAsync(source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS071")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_MalformedLimit_UsesDefault.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MalformedLimit_UsesDefault(CancellationToken cancellationToken)
    {
        var body = new string('a', 200);
        var source = BuildSource("summary", body);
        var options = new Dictionary<string, string>
        {
            ["automaticks.xml_doc_max_length"] = "not-a-number",
        };

        var diagnostics = await AnalyzeAsync(source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS071")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_NestedTagMarkupNotCounted_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NestedTagMarkupNotCounted_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp;
                              /// <summary>Uses <see cref="System.String" /> and <c>Foo</c> here.</summary>
                              public class Foo { }
                              """;

        var diagnostics = await AnalyzeAsync(source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS071")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_NoDocumentation_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NoDocumentation_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp;
                              public class Foo { }
                              """;

        var diagnostics = await AnalyzeAsync(source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS071")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ShortSummary_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ShortSummary_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        var source = BuildSource("summary", "A short description.");

        var diagnostics = await AnalyzeAsync(source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS071")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_WhitespaceOnlyDocumentation_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_WhitespaceOnlyDocumentation_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp;
                              /// <summary>   </summary>
                              public class Foo { }
                              """;

        var diagnostics = await AnalyzeAsync(source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS071")).IsFalse();
    }

    private async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(
        string source,
        CancellationToken cancellationToken)
    {
        var analyzer = new XmlDocLengthAnalyzer();
        return await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);
    }

    private async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(
        string source,
        IReadOnlyDictionary<string, string> configOptions,
        CancellationToken cancellationToken)
    {
        var analyzer = new XmlDocLengthAnalyzer();
        var options = new AnalysisOptions
        {
            ConfigOptions = configOptions,
        };
        return await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);
    }
    private string BuildSource(string tag, string body)
    {
        return $"namespace MyApp;\r\n/// <{tag}>{body}</{tag}>\r\npublic class Foo {{ }}";
    }

}
