using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class ParamsParameterAnalyzerTests
{
    [Test]
    public async Task Analyze_MethodWithParamsArray_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(params int[] values) { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ParamsParameterAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS055")).IsTrue();
    }

    [Test]
    public async Task Analyze_MethodWithoutParams_ReportsNoDiagnostic()
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(IReadOnlyList<int> values) { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ParamsParameterAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS055")).IsFalse();
    }
}
