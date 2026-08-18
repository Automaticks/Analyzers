using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Formatting;

/// <summary>
///     Tests for UnsortedUsingDirectivesAnalyzer.
/// </summary>
public class UnsortedUsingDirectivesAnalyzerTests
{

    /// <summary>
    ///     Tests that Analyze_AliasUsingIgnoredInSortCheck_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AliasUsingIgnoredInSortCheck_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using Inferno.Core;
                              using X = System.Text.StringBuilder;
                              using System;

                              namespace MyApp;
                              public class Foo { }
                              """;

        var analyzer = new UnsortedUsingDirectivesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS047")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_AlphabeticalOrder_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AlphabeticalOrder_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using Inferno.Core;
                              using System;

                              namespace MyApp;
                              public class Foo { }
                              """;

        var analyzer = new UnsortedUsingDirectivesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS047")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_CaseInsensitiveOrder_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CaseInsensitiveOrder_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using inferno.core;
                              using System;

                              namespace MyApp;
                              public class Foo { }
                              """;

        var analyzer = new UnsortedUsingDirectivesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS047")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ConditionalUsingInCorrectAlphabeticalPosition_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConditionalUsingInCorrectAlphabeticalPosition_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using Avalonia;
                              #if true
                              using Avalonia.Diagnostics;
                              #endif
                              using System;

                              namespace MyApp;
                              public class Foo { }
                              """;

        var analyzer = new UnsortedUsingDirectivesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS047")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ConditionalUsingOutOfAlphabeticalOrder_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConditionalUsingOutOfAlphabeticalOrder_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using Inferno.Core;
                              using System;
                              #if true
                              using Avalonia.Diagnostics;
                              #endif

                              namespace MyApp;
                              public class Foo { }
                              """;

        var analyzer = new UnsortedUsingDirectivesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS047")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_OutOfAlphabeticalOrder_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_OutOfAlphabeticalOrder_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              using Inferno.Core;

                              namespace MyApp;
                              public class Foo { }
                              """;

        var analyzer = new UnsortedUsingDirectivesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS047")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_SingleUsing_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SingleUsing_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;

                              namespace MyApp;
                              public class Foo { }
                              """;

        var analyzer = new UnsortedUsingDirectivesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS047")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_StaticUsingIgnoredInSortCheck_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StaticUsingIgnoredInSortCheck_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using Inferno.Core;
                              using static System.Math;
                              using System;

                              namespace MyApp;
                              public class Foo { }
                              """;

        var analyzer = new UnsortedUsingDirectivesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS047")).IsFalse();
    }
}
