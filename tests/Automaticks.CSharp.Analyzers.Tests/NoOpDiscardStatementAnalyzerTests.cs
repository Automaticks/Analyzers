using Automaticks.CSharp.LanguageFeatures;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

/// <summary>
///     Tests for NoOpDiscardStatementAnalyzer.
/// </summary>
public class NoOpDiscardStatementAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_CompoundAssignment_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CompoundAssignment_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Bar(int input) { var total = 0; total += input; return total; }
                                  }
                              }
                              """;

        var analyzer = new NoOpDiscardStatementAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS068")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_DiscardOfCancellationTokenParameter_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_DiscardOfCancellationTokenParameter_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(CancellationToken token) { _ = token; }
                                  }
                              }
                              """;

        var analyzer = new NoOpDiscardStatementAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS068")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_DiscardOfLocalVariable_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_DiscardOfLocalVariable_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() { var value = 1; _ = value; }
                                  }
                              }
                              """;

        var analyzer = new NoOpDiscardStatementAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS068")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_DiscardOfMethodCall_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_DiscardOfMethodCall_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Compute() { return 1; }
                                      public void Bar() { _ = Compute(); }
                                  }
                              }
                              """;

        var analyzer = new NoOpDiscardStatementAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS068")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_DiscardOfPropertyRead_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_DiscardOfPropertyRead_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Value { get; set; }
                                      public void Bar() { _ = Value; }
                                  }
                              }
                              """;

        var analyzer = new NoOpDiscardStatementAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS068")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_DiscardOfThis_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_DiscardOfThis_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Bar() { _ = this; return 1; }
                                  }
                              }
                              """;

        var analyzer = new NoOpDiscardStatementAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS068")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_InvocationStatement_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_InvocationStatement_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Compute() { }
                                      public void Bar() { Compute(); }
                                  }
                              }
                              """;

        var analyzer = new NoOpDiscardStatementAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS068")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_OutParameterDiscard_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_OutParameterDiscard_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool Bar(string text) { return int.TryParse(text, out _); }
                                  }
                              }
                              """;

        var analyzer = new NoOpDiscardStatementAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS068")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_RegularAssignment_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_RegularAssignment_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Bar(int input) { var value = 0; value = input; return value; }
                                  }
                              }
                              """;

        var analyzer = new NoOpDiscardStatementAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS068")).IsFalse();
    }
}
