using Automaticks.CSharp;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Formatting;

/// <summary>
///     Tests for SingleBlankLineBetweenUsingsAndNamespaceAnalyzer.
/// </summary>
public class SingleBlankLineBetweenUsingsAndNamespaceAnalyzerTests
{

    /// <summary>
    ///     Tests that Analyze_BlankLineBetweenUsingsButNoBlankLineBeforeNamespace_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BlankLineBetweenUsingsButNoBlankLineBeforeNamespace_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = "using System;\n\nusing System.Collections.Generic;\nnamespace MyApp;\npublic class Foo { }";

        var analyzer = new SingleBlankLineBetweenUsingsAndNamespaceAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS043")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_MultipleUsingsNoBlankLineBeforeNamespace_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MultipleUsingsNoBlankLineBeforeNamespace_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Foo { }
                              }
                              """;

        var analyzer = new SingleBlankLineBetweenUsingsAndNamespaceAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS043")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_MultipleUsingsOneBlankLineBeforeNamespace_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MultipleUsingsOneBlankLineBeforeNamespace_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              using System.Collections.Generic;

                              namespace MyApp {
                                  public class Foo { }
                              }
                              """;

        var analyzer = new SingleBlankLineBetweenUsingsAndNamespaceAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS043")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_NoBlankLineBetweenLastUsingAndBlockNamespace_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NoBlankLineBetweenLastUsingAndBlockNamespace_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo { }
                              }
                              """;

        var analyzer = new SingleBlankLineBetweenUsingsAndNamespaceAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS043")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_NoBlankLineBetweenLastUsingAndFileScopedNamespace_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NoBlankLineBetweenLastUsingAndFileScopedNamespace_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp;
                              public class Foo { }
                              """;

        var analyzer = new SingleBlankLineBetweenUsingsAndNamespaceAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS043")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_NoNamespaceDeclaration_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NoNamespaceDeclaration_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;

                              public class Foo { }
                              """;

        var analyzer = new SingleBlankLineBetweenUsingsAndNamespaceAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS043")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_NoUsingDirectives_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NoUsingDirectives_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { }
                              }
                              """;

        var analyzer = new SingleBlankLineBetweenUsingsAndNamespaceAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS043")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_OneBlankLineBetweenLastUsingAndBlockNamespace_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_OneBlankLineBetweenLastUsingAndBlockNamespace_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;

                              namespace MyApp {
                                  public class Foo { }
                              }
                              """;

        var analyzer = new SingleBlankLineBetweenUsingsAndNamespaceAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS043")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_OneBlankLineBetweenLastUsingAndFileScopedNamespace_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_OneBlankLineBetweenLastUsingAndFileScopedNamespace_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;

                              namespace MyApp;
                              public class Foo { }
                              """;

        var analyzer = new SingleBlankLineBetweenUsingsAndNamespaceAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS043")).IsFalse();
    }
}
