using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class ProviderFactoryPropertyAnalyzerTests
{
    [Test]
    public async Task Analyze_MethodOnProvider_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFooProvider {
                                      string GetFoo();
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ProviderFactoryPropertyAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS004")).IsFalse();
    }

    [Test]
    public async Task Analyze_PropertyOnNonProvider_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class FooService {
                                      public string Name { get; set; } = string.Empty;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ProviderFactoryPropertyAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS004")).IsFalse();
    }

    [Test]
    public async Task Analyze_PropertyOnProvider_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFooProvider {
                                      string Name { get; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ProviderFactoryPropertyAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS004")).IsTrue();
    }

    [Test]
    public async Task Analyze_MethodOnSession_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class FooSession {
                                      public string GetName() => string.Empty;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ProviderFactoryPropertyAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS004")).IsFalse();
    }

    [Test]
    public async Task Analyze_PropertyOnSession_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class FooSession {
                                      public string Name { get; } = string.Empty;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ProviderFactoryPropertyAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS004")).IsTrue();
    }
}
