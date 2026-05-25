using Automaticks.Linq;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.Linq.Analyzers.Tests;

public class LinqUsageAnalyzerTests
{
    [Test]
    public async Task Analyze_LinqUsingDirective_ReportsDiagnostic()
    {
        const string source = """
                              using System.Linq;
                              namespace MyApp {
                                  public class Foo {}
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new LinqUsageAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXLQ002")).IsTrue();
    }

    [Test]
    public async Task Analyze_NoLinqUsing_NoDiagnostic()
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Foo {}
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new LinqUsageAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXLQ002")).IsFalse();
    }
}
