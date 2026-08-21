using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Testing.Analyzers.Tests;

/// <summary>
///     Tests for TestClassNameAnalyzer.
/// </summary>
public class TestClassNameAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_ClassEndingWithTestsButNoTestMethods_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ClassEndingWithTestsButNoTestMethods_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp.Tests {
                                  public class SomeHelperTests {
                                      public void NotATest() {}
                                  }
                              }
                              """;

        var analyzer = new TestClassNameAnalyzer();
        var options = new AnalysisOptions
{
    IsTestProject = true
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST002")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ClassNamedTests_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ClassNamedTests_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace TUnit.Core { public class TestAttribute : System.Attribute {} }
                              namespace MyApp.Tests {
                                  public class Tests {
                                      [TUnit.Core.Test]
                                      public void Method_Scenario_Result() {}
                                  }
                              }
                              """;

        var analyzer = new TestClassNameAnalyzer();
        var options = new AnalysisOptions
{
    IsTestProject = true
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST002")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ClassNameWithNoPascalWords_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ClassNameWithNoPascalWords_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace TUnit.Core { public class TestAttribute : System.Attribute {} }
                              namespace MyApp.Tests {
                                  public class _Tests {
                                      [TUnit.Core.Test]
                                      public void Method_Scenario_Result() {}
                                  }
                              }
                              """;

        var analyzer = new TestClassNameAnalyzer();
        var options = new AnalysisOptions
{
    IsTestProject = true
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST002")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ClassWithNonMethodMember_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ClassWithNonMethodMember_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace TUnit.Core { public class TestAttribute : System.Attribute {} }
                              namespace MyApp { public class Widget {} }
                              namespace MyApp.Tests {
                                  public class WidgetTests {
                                      public int Value { get; set; }
                                      [TUnit.Core.Test]
                                      public void Method_Scenario_Result() {}
                                  }
                              }
                              """;

        var analyzer = new TestClassNameAnalyzer();
        var options = new AnalysisOptions
{
    IsTestProject = true
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST002")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ClassWithUnrelatedAttributeBeforeTestAttribute_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ClassWithUnrelatedAttributeBeforeTestAttribute_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace TUnit.Core {
                                  public class TestAttribute : System.Attribute {}
                                  public class CategoryAttribute : System.Attribute {}
                              }
                              namespace MyApp { public class Gadget {} }
                              namespace MyApp.Tests {
                                  public class GadgetTests {
                                      [TUnit.Core.Category]
                                      [TUnit.Core.Test]
                                      public void Method_Scenario_Result() {}
                                  }
                              }
                              """;

        var analyzer = new TestClassNameAnalyzer();
        var options = new AnalysisOptions
{
    IsTestProject = true
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST002")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_TestClassMatchingExistingType_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TestClassMatchingExistingType_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace TUnit.Core { public class TestAttribute : System.Attribute {} }
                              namespace MyApp { public class SpawnViewModel {} }
                              namespace MyApp.Tests {
                                  public class SpawnViewModelTests {
                                      [TUnit.Core.Test]
                                      public void Method_Scenario_Result() {}
                                  }
                              }
                              """;

        var analyzer = new TestClassNameAnalyzer();
        var options = new AnalysisOptions
{
    IsTestProject = true
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST002")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_TestClassMatchingTypeInReferencedAssembly_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TestClassMatchingTypeInReferencedAssembly_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        var reference = AnalyzerTestRunner.CompileToReference(
            "namespace App.Logging { public class LogPathExpander {} }");
        const string source = """
                              namespace TUnit.Core { public class TestAttribute : System.Attribute {} }
                              namespace App.Logging.Tests {
                                  public class LogPathExpanderTests {
                                      [TUnit.Core.Test]
                                      public void Method_Scenario_Result() {}
                                  }
                              }
                              """;

        var analyzer = new TestClassNameAnalyzer();
        var options = new AnalysisOptions
        {
            AdditionalReferences = [reference],
            IsTestProject = true,
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST002")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_TestClassWithNoMatchingType_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TestClassWithNoMatchingType_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace TUnit.Core { public class TestAttribute : System.Attribute {} }
                              namespace MyApp.Tests {
                                  public class TextureDrawOrderTests {
                                      [TUnit.Core.Test]
                                      public void Method_Scenario_Result() {}
                                  }
                              }
                              """;

        var analyzer = new TestClassNameAnalyzer();
        var options = new AnalysisOptions
{
    IsTestProject = true
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST002")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_TestClassWithQualifierMatchingBaseType_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TestClassWithQualifierMatchingBaseType_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace TUnit.Core { public class TestAttribute : System.Attribute {} }
                              namespace MyApp { public class DrawSortKey {} }
                              namespace MyApp.Tests {
                                  public class DrawSortKeyLargeViewportTests {
                                      [TUnit.Core.Test]
                                      public void Method_Scenario_Result() {}
                                  }
                              }
                              """;

        var analyzer = new TestClassNameAnalyzer();
        var options = new AnalysisOptions
{
    IsTestProject = true
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST002")).IsFalse();
    }
}
