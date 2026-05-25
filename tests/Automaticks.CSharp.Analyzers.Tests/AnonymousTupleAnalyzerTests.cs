using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class AnonymousTupleAnalyzerTests
{
    [Test]
    public async Task Analyze_NamedTupleTypeDeclaration_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() {
                                          (string Name, int Age) p = ("Bob", 30);
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AnonymousTupleAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS012")).IsTrue();
    }

    [Test]
    public async Task Analyze_StronglyTypedRecord_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public record MyType(string A, int B);
                                  public class Foo {
                                      public void Bar()
                                      {
                                          var x = new MyType("hello", 42);
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AnonymousTupleAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS012")).IsFalse();
    }

    [Test]
    public async Task Analyze_TupleDeconstruction_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private (string, int) GetTuple()
                                      {
                                          return ("a", 1);
                                      }
                                      public void Bar()
                                      {
                                          var (x, y) = GetTuple();
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AnonymousTupleAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS012")).IsTrue();
    }

    [Test]
    public async Task Analyze_TupleLiteral_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar()
                                      {
                                          var x = ("hello", 42);
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AnonymousTupleAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS012")).IsTrue();
    }

    [Test]
    public async Task Analyze_TupleParameter_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar((string, int) p)
                                      {
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AnonymousTupleAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS012")).IsTrue();
    }

    [Test]
    public async Task Analyze_TupleReturnType_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public (string, int) Bar()
                                      {
                                          return ("a", 1);
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AnonymousTupleAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS012")).IsTrue();
    }

    [Test]
    public async Task Analyze_TupleTypeDeclaration_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() {
                                          (string, int) x = ("a", 1);
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AnonymousTupleAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS012")).IsTrue();
    }
}
