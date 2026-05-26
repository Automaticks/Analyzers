using Automaticks.CSharp;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

/// <summary>
///     Tests for DuplicateUsingDirectiveAnalyzer.
/// </summary>
public class DuplicateUsingDirectiveAnalyzerTests
{

    /// <summary>
    ///     Tests that Analyze_DuplicateAliasUsing_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_DuplicateAliasUsing_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using X = System.Text.StringBuilder;
                              using X = System.Text.StringBuilder;

                              namespace MyApp;
                              public class Foo { }
                              """;

        var analyzer = new DuplicateUsingDirectiveAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS046")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_DuplicateGlobalUsing_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_DuplicateGlobalUsing_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = "global using System;\nglobal using System;\nnamespace MyApp;\npublic class Foo { }";

        var analyzer = new DuplicateUsingDirectiveAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS046")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_DuplicateRegularUsing_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_DuplicateRegularUsing_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              using System;

                              namespace MyApp;
                              public class Foo { }
                              """;

        var analyzer = new DuplicateUsingDirectiveAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS046")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_DuplicateStaticUsing_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_DuplicateStaticUsing_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using static System.Math;
                              using static System.Math;

                              namespace MyApp;
                              public class Foo { }
                              """;

        var analyzer = new DuplicateUsingDirectiveAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS046")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_TriplicateUsing_ReportsDiagnosticTwice.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TriplicateUsing_ReportsDiagnosticTwice(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              using System;
                              using System;

                              namespace MyApp;
                              public class Foo { }
                              """;

        var analyzer = new DuplicateUsingDirectiveAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.CountId(diagnostics, "ATXCS046")).IsEqualTo(2);
    }

    /// <summary>
    ///     Tests that Analyze_UniqueUsings_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_UniqueUsings_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              using System.Collections.Generic;

                              namespace MyApp;
                              public class Foo { }
                              """;

        var analyzer = new DuplicateUsingDirectiveAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS046")).IsFalse();
    }
}
