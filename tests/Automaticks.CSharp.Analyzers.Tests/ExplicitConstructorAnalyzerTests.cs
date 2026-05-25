using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class ExplicitConstructorAnalyzerTests
{

    [Test]
    public async Task Analyze_ClassWithPrimaryConstructor_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Point(int x, int y) { }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ExplicitConstructorAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS037")).IsTrue();
    }

    [Test]
    public async Task Analyze_StructWithPrimaryConstructor_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public struct Vector(float x, float y) { }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ExplicitConstructorAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS037")).IsTrue();
    }

    [Test]
    public async Task Analyze_RecordWithPrimaryConstructor_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public record Person(string Name, int Age);
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ExplicitConstructorAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS037")).IsTrue();
    }

    [Test]
    public async Task Analyze_RecordStructWithPrimaryConstructor_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public record struct Coordinate(double Latitude, double Longitude);
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ExplicitConstructorAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS037")).IsTrue();
    }

    [Test]
    public async Task Analyze_ClassWithPrimaryConstructorAndBody_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Service(string name) {
                                      public string Name { get; } = name;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ExplicitConstructorAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS037")).IsTrue();
    }

    [Test]
    public async Task Analyze_MultiplePrimaryConstructorTypes_ReportsOneDiagnosticEach()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo(int x) { }
                                  public struct Bar(int y) { }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ExplicitConstructorAnalyzer(), source);

        await Assert.That(diagnostics.Count(d => d.Id == "ATXCS037")).IsEqualTo(2);
    }

    [Test]
    public async Task Analyze_ClassWithExplicitConstructor_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Point {
                                      public Point(int x, int y) { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ExplicitConstructorAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS037")).IsFalse();
    }

    [Test]
    public async Task Analyze_StructWithExplicitConstructor_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public struct Vector {
                                      public Vector(float x, float y) { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ExplicitConstructorAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS037")).IsFalse();
    }

    [Test]
    public async Task Analyze_ClassWithNoConstructor_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class SimpleService { }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ExplicitConstructorAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS037")).IsFalse();
    }

    [Test]
    public async Task Analyze_RecordWithNoParameterList_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public record Config {
                                      public int Timeout { get; init; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ExplicitConstructorAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS037")).IsFalse();
    }

    [Test]
    public async Task Analyze_StaticClass_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public static class MathHelper {
                                      public static int Add(int a, int b)
                                      {
                                          return a + b;
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ExplicitConstructorAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS037")).IsFalse();
    }
}
