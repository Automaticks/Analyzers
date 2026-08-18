using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Formatting;

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
    ///     Tests that Analyze_ExtensionMethodUsage_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExtensionMethodUsage_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections.Generic;
                              using System.Linq;

                              namespace MyApp;
                              public class Foo {
                                  public IEnumerable<int> Filter(List<int> values) => values.Where(value => value > 0);
                              }
                              """;

        var analyzer = new UnusedUsingDirectiveAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS048")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_FullyQualifiedNameOnly_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FullyQualifiedNameOnly_ReportsDiagnostic(CancellationToken cancellationToken)
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

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS048")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_FullyQualifiedStaticMemberAccess_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FullyQualifiedStaticMemberAccess_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Text;

                              namespace MyApp;
                              public class Foo {
                                  public string Value() => System.Text.Encoding.UTF8.EncodingName;
                              }
                              """;

        var analyzer = new UnusedUsingDirectiveAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS048")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_GlobalNamespaceTypeReference_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_GlobalNamespaceTypeReference_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Text;

                              public class Helper { }

                              namespace MyApp;
                              public class Foo { public Helper Make() { return new Helper(); } }
                              """;

        var analyzer = new UnusedUsingDirectiveAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS048")).IsTrue();
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
    ///     Tests that Analyze_OverloadResolutionFailure_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_OverloadResolutionFailure_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Text;

                              namespace MyApp;
                              public class Foo
                              {
                                  public void Take(int value) { }
                                  public void Bar() { Take(); }
                              }
                              """;

        var analyzer = new UnusedUsingDirectiveAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS048")).IsTrue();
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
    ///     Tests that Analyze_UnresolvableTypeReference_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_UnresolvableTypeReference_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Text;

                              namespace MyApp;
                              public class Foo { public Missing Make() { return null; } }
                              """;

        var analyzer = new UnusedUsingDirectiveAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS048")).IsTrue();
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
    ///     Tests that Analyze_UsedAndUnusedUsings_ReportsOneDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_UsedAndUnusedUsings_ReportsOneDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Text;
                              using System.Threading;

                              namespace MyApp;
                              public class Foo { public StringBuilder Make() { return new StringBuilder(); } }
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
                                  public StringBuilder Builder { get; set; }
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
