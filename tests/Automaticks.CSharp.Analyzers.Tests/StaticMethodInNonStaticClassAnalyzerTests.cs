using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class StaticMethodInNonStaticClassAnalyzerTests
{
    [Test]
    public async Task Analyze_ExtensionMethodInStaticClass_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public static class FooExtensions {
                                      public static void Bar(this object obj) {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new StaticMethodInNonStaticClassAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS011")).IsFalse();
    }

    [Test]
    public async Task Analyze_InstanceMethodInNonStaticClass_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new StaticMethodInNonStaticClassAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS011")).IsFalse();
    }

    [Test]
    public async Task Analyze_StaticMethodInNonStaticClass_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public static void Bar() {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new StaticMethodInNonStaticClassAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS011")).IsTrue();
    }

    [Test]
    public async Task Analyze_StaticMethodInStaticClass_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public static class Foo {
                                      public static void Bar() {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new StaticMethodInNonStaticClassAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS011")).IsFalse();
    }
}
