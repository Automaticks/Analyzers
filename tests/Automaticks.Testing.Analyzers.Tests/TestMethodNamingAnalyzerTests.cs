using Automaticks.Testing;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.Testing.Analyzers.Tests;

public class TestMethodNamingAnalyzerTests
{
    [Test]
    public async Task Analyze_MethodWithArgumentsAttributeAndValidName_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TestMethodNamingAnalyzer(), source, true);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXTST003")).IsFalse();
    }

    [Test]
    public async Task Analyze_MethodWithNoUnderscores_ReportsDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TestMethodNamingAnalyzer(), source, true);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXTST003")).IsTrue();
    }

    [Test]
    public async Task Analyze_MethodWithThreePartName_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TestMethodNamingAnalyzer(), source, true);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXTST003")).IsFalse();
    }

    [Test]
    public async Task Analyze_MethodWithTwoPartName_ReportsDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TestMethodNamingAnalyzer(), source, true);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXTST003")).IsTrue();
    }
}
