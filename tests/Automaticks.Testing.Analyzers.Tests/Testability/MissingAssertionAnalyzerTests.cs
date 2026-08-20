using Automaticks.Testing.Testability;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Testing.Analyzers.Tests.Testability;

/// <summary>
///     Tests for MissingAssertionAnalyzer.
/// </summary>
public class MissingAssertionAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_AbstractTestMethod_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AbstractTestMethod_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace TUnit.Core { public class TestAttribute : System.Attribute {} }
                              namespace MyApp {
                                  public abstract class FooTests {
                                      [TUnit.Core.Test]
                                      public abstract void Method_Scenario_Result();
                                  }
                              }
                              """;

        var analyzer = new MissingAssertionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST011")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExpressionBodiedTestMethodWithAssertion_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExpressionBodiedTestMethodWithAssertion_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace TUnit.Core { public class TestAttribute : System.Attribute {} }
                              namespace MyApp {
                                  public static class Assert { public static bool That(bool value) => value; }
                                  public class FooTests {
                                      [TUnit.Core.Test]
                                      public void Method_Scenario_Result() => Assert.That(true);
                                  }
                              }
                              """;

        var analyzer = new MissingAssertionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST011")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_NonTestMethodWithoutAssertion_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NonTestMethodWithoutAssertion_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace TUnit.Core { public class TestAttribute : System.Attribute {} }
                              namespace MyApp {
                                  public class FooTests {
                                      public void HelperMethod() { var x = 1 + 1; }
                                  }
                              }
                              """;

        var analyzer = new MissingAssertionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST011")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_TestMethodWithAssertion_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TestMethodWithAssertion_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace TUnit.Core { public class TestAttribute : System.Attribute {} }
                              namespace MyApp {
                                  public static class Assert { public static bool That(bool value) => value; }
                                  public class FooTests {
                                      [TUnit.Core.Test]
                                      public void Method_Scenario_Result() { Assert.That(true); }
                                  }
                              }
                              """;

        var analyzer = new MissingAssertionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST011")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_TestMethodWithoutAssertion_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TestMethodWithoutAssertion_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace TUnit.Core { public class TestAttribute : System.Attribute {} }
                              namespace MyApp {
                                  public class FooTests {
                                      [TUnit.Core.Test]
                                      public void Method_Scenario_Result() { var x = 1 + 1; }
                                  }
                              }
                              """;

        var analyzer = new MissingAssertionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(
            diagnostics,
            "ATXTST011",
            "'Method_Scenario_Result'")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_TestMethodWithThrowsAssertion_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TestMethodWithThrowsAssertion_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace TUnit.Core { public class TestAttribute : System.Attribute {} }
                              namespace MyApp {
                                  public static class Assert { public static void Throws(System.Action action) {} }
                                  public class FooTests {
                                      [TUnit.Core.Test]
                                      public void Method_Scenario_Result() { Assert.Throws(() => throw new System.Exception()); }
                                  }
                              }
                              """;

        var analyzer = new MissingAssertionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST011")).IsFalse();
    }
}
