using Automaticks.Testing;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.Testing.Analyzers.Tests;

public class TestClassNameAnalyzerTests
{
    [Test]
    public async Task Analyze_ClassEndingWithTestsButNoTestMethods_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp.Tests {
                                  public class SomeHelperTests {
                                      public void NotATest() {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TestClassNameAnalyzer(), source, true);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXTST002")).IsFalse();
    }

    [Test]
    public async Task Analyze_TestClassMatchingExistingType_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TestClassNameAnalyzer(), source, true);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXTST002")).IsFalse();
    }

    [Test]
    public async Task Analyze_TestClassWithNoMatchingType_ReportsDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TestClassNameAnalyzer(), source, true);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXTST002")).IsTrue();
    }

    [Test]
    public async Task Analyze_TestClassWithQualifierMatchingBaseType_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TestClassNameAnalyzer(), source, true);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXTST002")).IsFalse();
    }

    [Test]
    public async Task Analyze_TestClassMatchingTypeInReferencedAssembly_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(
            new TestClassNameAnalyzer(), source, [reference], isTestProject: true);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXTST002")).IsFalse();
    }
}
