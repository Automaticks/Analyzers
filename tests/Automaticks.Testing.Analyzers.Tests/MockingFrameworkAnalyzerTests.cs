using Automaticks.Testing;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.Testing.Analyzers.Tests;

public class MockingFrameworkAnalyzerTests
{
    [Test]
    public async Task Analyze_TestProjectUsingMoq_ReportsDiagnostic()
    {
        const string source = """
                              using Moq;
                              namespace MyApp.Tests {
                                  public class FooTests {}
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MockingFrameworkAnalyzer(), source, true);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXTST001")).IsTrue();
    }

    [Test]
    public async Task Analyze_TestProjectUsingNSubstitute_ReportsDiagnostic()
    {
        const string source = """
                              using NSubstitute;
                              namespace MyApp.Tests {
                                  public class FooTests {}
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MockingFrameworkAnalyzer(), source, true);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXTST001")).IsTrue();
    }

    [Test]
    public async Task Analyze_TestProjectUsingRegularNamespace_ReportsNoDiagnostic()
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp.Tests {
                                  public class FooTests {}
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MockingFrameworkAnalyzer(), source, true);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXTST001")).IsFalse();
    }
}
