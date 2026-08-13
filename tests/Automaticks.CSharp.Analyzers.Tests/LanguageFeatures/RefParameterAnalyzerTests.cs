using Automaticks.CSharp;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures;

/// <summary>
///     Tests for RefParameterAnalyzer.
/// </summary>
public class RefParameterAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_MethodWithRefNotFirst_ReportsAtxCs026.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodWithRefNotFirst_ReportsAtxCs026(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void DoWork(int a, ref int value) { value = 0; }
                                  }
                              }
                              """;

        var analyzer = new RefParameterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS026")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_MethodWithTwoRefs_ReportsAtxCs027.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodWithTwoRefs_ReportsAtxCs027(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void DoWork(ref int a, ref int b) { a = 0; b = 0; }
                                  }
                              }
                              """;

        var analyzer = new RefParameterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS027")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_NonSetPropertyWithRef_ReportsAtxCs025.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NonSetPropertyWithRef_ReportsAtxCs025(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void DoWork(ref int value) { value = 0; }
                                  }
                              }
                              """;

        var analyzer = new RefParameterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS025")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_SetPropertyWithOneRefFirst_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SetPropertyWithOneRefFirst_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      protected bool SetProperty(ref int field, int value) { field = value; return true; }
                                  }
                              }
                              """;

        var analyzer = new RefParameterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasAnyId(diagnostics, ["ATXCS025", "ATXCS026", "ATXCS027"])).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_SetPropertyWithRefNotFirst_ReportsAtxCs026.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SetPropertyWithRefNotFirst_ReportsAtxCs026(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      protected bool SetProperty(int value, ref int field) { field = value; return true; }
                                  }
                              }
                              """;

        var analyzer = new RefParameterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS025")).IsFalse();
        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS026")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_SetPropertyWithTwoRefs_ReportsAtxCs027.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SetPropertyWithTwoRefs_ReportsAtxCs027(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      protected bool SetProperty(ref int field, ref int extra) { field = 0; extra = 0; return true; }
                                  }
                              }
                              """;

        var analyzer = new RefParameterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS025")).IsFalse();
        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS027")).IsTrue();
    }
}
