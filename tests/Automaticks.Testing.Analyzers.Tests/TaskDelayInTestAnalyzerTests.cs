using Automaticks.Testing;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.Testing.Analyzers.Tests;

public class TaskDelayInTestAnalyzerTests
{
    [Test]
    public async Task Analyze_TestProjectWithCustomDelayMethod_ReportsNoDiagnostic()
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp.Tests {
                                  public class FooTests {
                                      private static Task Delay(int ms) { return Task.CompletedTask; }
                                      public async Task Bar_Something_ReturnsTrue() { await Delay(200); }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TaskDelayInTestAnalyzer(), source, true);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXTST004")).IsFalse();
    }

    [Test]
    public async Task Analyze_TestProjectWithFullyQualifiedTaskDelay_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp.Tests {
                                  public class FooTests {
                                      public async System.Threading.Tasks.Task Bar_Something_ReturnsTrue() {
                                          await System.Threading.Tasks.Task.Delay(200);
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TaskDelayInTestAnalyzer(), source, true);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXTST004")).IsTrue();
    }

    [Test]
    public async Task Analyze_TestProjectWithTaskDelay_ReportsDiagnostic()
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp.Tests {
                                  public class FooTests {
                                      public async Task Bar_Something_ReturnsTrue() { await Task.Delay(200); }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TaskDelayInTestAnalyzer(), source, true);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXTST004")).IsTrue();
    }
}
