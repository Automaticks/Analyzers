using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Testing.Analyzers.Tests;

/// <summary>
///     Tests for TestMethodNamingAnalyzer.
/// </summary>
public class TestMethodNamingAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_MethodWithArgumentsAttributeAndValidName_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodWithArgumentsAttributeAndValidName_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace TUnit.Core {
                                  public class TestAttribute : System.Attribute {}
                                  public class ArgumentsAttribute : System.Attribute {
                                      public ArgumentsAttribute(params object[] args) {}
                                  }
                              }
                              namespace MyApp.Tests {
                                  public class FooTests {
                                      [TUnit.Core.Test]
                                      [TUnit.Core.Arguments(1, 2)]
                                      public void Parse_ValidInput_ReturnsExpected(int a, int b) {}
                                  }
                              }
                              """;

        var analyzer = new TestMethodNamingAnalyzer();
        var options = new AnalysisOptions
{
    IsTestProject = true
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST003")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_MethodWithNoUnderscores_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodWithNoUnderscores_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace TUnit.Core { public class TestAttribute : System.Attribute {} }
                              namespace MyApp.Tests {
                                  public class FooTests {
                                      [TUnit.Core.Test]
                                      public void ParseReturnsExpected() {}
                                  }
                              }
                              """;

        var analyzer = new TestMethodNamingAnalyzer();
        var options = new AnalysisOptions
{
    IsTestProject = true
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST003")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_MethodWithThreePartName_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodWithThreePartName_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace TUnit.Core { public class TestAttribute : System.Attribute {} }
                              namespace MyApp.Tests {
                                  public class FooTests {
                                      [TUnit.Core.Test]
                                      public void Parse_ValidInput_ReturnsExpected() {}
                                  }
                              }
                              """;

        var analyzer = new TestMethodNamingAnalyzer();
        var options = new AnalysisOptions
{
    IsTestProject = true
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST003")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_MethodWithTwoPartName_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodWithTwoPartName_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace TUnit.Core { public class TestAttribute : System.Attribute {} }
                              namespace MyApp.Tests {
                                  public class FooTests {
                                      [TUnit.Core.Test]
                                      public void Parse_ReturnsExpected() {}
                                  }
                              }
                              """;

        var analyzer = new TestMethodNamingAnalyzer();
        var options = new AnalysisOptions
{
    IsTestProject = true
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST003")).IsTrue();
    }
}
