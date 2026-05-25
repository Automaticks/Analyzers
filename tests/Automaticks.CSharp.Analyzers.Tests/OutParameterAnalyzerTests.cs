using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class OutParameterAnalyzerTests
{
    [Test]
    public async Task Analyze_MethodWithNoOutParams_NoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void DoWork(int a, int b) {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new OutParameterAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS023" || d.Id == "ATXCS024")).IsFalse();
    }

    [Test]
    public async Task Analyze_MethodWithOneOutParamLast_NoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool TryGet(int id, out string result) { result = ""; return true; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new OutParameterAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS023" || d.Id == "ATXCS024")).IsFalse();
    }

    [Test]
    public async Task Analyze_MethodWithOutParamNotLast_ReportsAtxCs024()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void DoWork(out int result, int extra) { result = 0; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new OutParameterAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS024")).IsTrue();
    }

    [Test]
    public async Task Analyze_MethodWithTwoOutParams_ReportsAtxCs023()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void GetValues(out int a, out int b) { a = 0; b = 0; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new OutParameterAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS023")).IsTrue();
    }

    [Test]
    public async Task Analyze_MethodWithTwoOutParamsFirstNotLast_ReportsBothDiagnostics()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void DoWork(out int a, out int b, int extra) { a = 0; b = 0; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new OutParameterAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS023")).IsTrue();
        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS024")).IsTrue();
    }
}
