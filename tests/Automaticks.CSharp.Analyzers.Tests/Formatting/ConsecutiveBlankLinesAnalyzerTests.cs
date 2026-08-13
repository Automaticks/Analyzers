using Automaticks.CSharp;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Formatting;

/// <summary>
///     Tests for ConsecutiveBlankLinesAnalyzer.
/// </summary>
public class ConsecutiveBlankLinesAnalyzerTests
{

    /// <summary>
    ///     Tests that Analyze_NoBlankLinesBetweenMembers_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NoBlankLinesBetweenMembers_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Service {
                                      public void Foo() { }
                                      public void Bar() { }
                                  }
                              }
                              """;

        var analyzer = new ConsecutiveBlankLinesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS044")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_SingleBlankLineBetweenMethods_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SingleBlankLineBetweenMethods_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Service {
                                      public void Foo() { }

                                      public void Bar() { }
                                  }
                              }
                              """;

        var analyzer = new ConsecutiveBlankLinesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS044")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_SingleBlankLineBetweenUsings_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SingleBlankLineBetweenUsings_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;

                              using System.Collections.Generic;

                              namespace MyApp {
                                  public class Foo { }
                              }
                              """;

        var analyzer = new ConsecutiveBlankLinesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS044")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ThreeConsecutiveBlankLines_ReportsOneDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ThreeConsecutiveBlankLines_ReportsOneDiagnostic(CancellationToken cancellationToken)
    {
        const string source = "namespace MyApp {\n    public class Service {\n        public void Foo() { }\n\n\n\n        public void Bar() { }\n    }\n}";

        var analyzer = new ConsecutiveBlankLinesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.CountId(diagnostics, "ATXCS044")).IsEqualTo(1);
    }

    /// <summary>
    ///     Tests that Analyze_ThreeConsecutiveBlankLinesBetweenMethods_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ThreeConsecutiveBlankLinesBetweenMethods_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = "namespace MyApp {\n    public class Service {\n        public void Foo() { }\n\n\n\n        public void Bar() { }\n    }\n}";

        var analyzer = new ConsecutiveBlankLinesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS044")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_TwoConsecutiveBlankLinesAtTopOfFile_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TwoConsecutiveBlankLinesAtTopOfFile_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = "\n\nnamespace MyApp {\n    public class Foo { }\n}";

        var analyzer = new ConsecutiveBlankLinesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS044")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_TwoConsecutiveBlankLinesBetweenMethods_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TwoConsecutiveBlankLinesBetweenMethods_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = "namespace MyApp {\n    public class Service {\n        public void Foo() { }\n\n\n        public void Bar() { }\n    }\n}";

        var analyzer = new ConsecutiveBlankLinesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS044")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_TwoConsecutiveBlankLinesBetweenProperties_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TwoConsecutiveBlankLinesBetweenProperties_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = "namespace MyApp {\n    public class Model {\n        public int Foo { get; set; }\n\n\n        public int Bar { get; set; }\n    }\n}";

        var analyzer = new ConsecutiveBlankLinesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS044")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_TwoConsecutiveBlankLinesBetweenUsingDirectives_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TwoConsecutiveBlankLinesBetweenUsingDirectives_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = "using System;\n\n\nusing System.Collections.Generic;\n\nnamespace MyApp {\n    public class Foo { }\n}";

        var analyzer = new ConsecutiveBlankLinesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS044")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_TwoConsecutiveBlankLinesInsideMethodBody_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TwoConsecutiveBlankLinesInsideMethodBody_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = "namespace MyApp {\n    public class Service {\n        public void Foo()\n        {\n            var x = 1;\n\n\n            var y = 2;\n        }\n    }\n}";

        var analyzer = new ConsecutiveBlankLinesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS044")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_TwoRunsOfConsecutiveBlankLines_ReportsTwoDiagnostics.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TwoRunsOfConsecutiveBlankLines_ReportsTwoDiagnostics(CancellationToken cancellationToken)
    {
        const string source = "namespace MyApp {\n    public class Service {\n        public void Foo() { }\n\n\n        public void Bar() { }\n\n\n        public void Baz() { }\n    }\n}";

        var analyzer = new ConsecutiveBlankLinesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.CountId(diagnostics, "ATXCS044")).IsEqualTo(2);
    }

    /// <summary>
    ///     Tests that Analyze_WhitespaceOnlyLineCountsAsBlankLine_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_WhitespaceOnlyLineCountsAsBlankLine_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = "namespace MyApp {\n    public class Service {\n        public void Foo() { }\n   \n\n        public void Bar() { }\n    }\n}";

        var analyzer = new ConsecutiveBlankLinesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS044")).IsTrue();
    }
}
