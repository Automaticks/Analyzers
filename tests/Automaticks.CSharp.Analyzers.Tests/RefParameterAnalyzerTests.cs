using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class RefParameterAnalyzerTests
{
    [Test]
    public async Task Analyze_MethodWithRefNotFirst_ReportsAtxCs026()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void DoWork(int a, ref int value) { value = 0; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new RefParameterAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS026")).IsTrue();
    }

    [Test]
    public async Task Analyze_MethodWithTwoRefs_ReportsAtxCs027()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void DoWork(ref int a, ref int b) { a = 0; b = 0; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new RefParameterAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS027")).IsTrue();
    }

    [Test]
    public async Task Analyze_NonSetPropertyWithRef_ReportsAtxCs025()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void DoWork(ref int value) { value = 0; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new RefParameterAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS025")).IsTrue();
    }

    [Test]
    public async Task Analyze_SetPropertyWithOneRefFirst_NoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      protected bool SetProperty(ref int field, int value) { field = value; return true; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new RefParameterAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id is "ATXCS025" or "ATXCS026" or "ATXCS027")).IsFalse();
    }

    [Test]
    public async Task Analyze_SetPropertyWithRefNotFirst_ReportsAtxCs026()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      protected bool SetProperty(int value, ref int field) { field = value; return true; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new RefParameterAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS025")).IsFalse();
        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS026")).IsTrue();
    }

    [Test]
    public async Task Analyze_SetPropertyWithTwoRefs_ReportsAtxCs027()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      protected bool SetProperty(ref int field, ref int extra) { field = 0; extra = 0; return true; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new RefParameterAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS025")).IsFalse();
        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS027")).IsTrue();
    }
}
