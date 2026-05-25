using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class InlineFieldInitializerAnalyzerTests
{

    [Test]
    public async Task Analyze_InstanceFieldWithNewExpression_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Service { }
                                  public class Consumer {
                                      private readonly Service _service = new();
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineFieldInitializerAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS036")).IsTrue();
    }

    [Test]
    public async Task Analyze_InstanceFieldWithPrimitiveLiteral_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Counter {
                                      private int _count = 0;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineFieldInitializerAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS036")).IsTrue();
    }

    [Test]
    public async Task Analyze_InstanceFieldWithStringLiteral_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Greeter {
                                      private string _name = "default";
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineFieldInitializerAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS036")).IsTrue();
    }

    [Test]
    public async Task Analyze_InstanceFieldWithNullLiteral_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Service { }
                                  public class Consumer {
                                      private Service? _service = null;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineFieldInitializerAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS036")).IsTrue();
    }

    [Test]
    public async Task Analyze_InstanceFieldWithStaticMethodCall_ReportsDiagnostic()
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Repository {
                                      private readonly List<int> _items = new List<int>();
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineFieldInitializerAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS036")).IsTrue();
    }

    [Test]
    public async Task Analyze_StaticFieldWithInitializer_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Registry {
                                      private static int _count = 0;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineFieldInitializerAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS036")).IsTrue();
    }

    [Test]
    public async Task Analyze_AutoPropertyWithInitializer_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Config {
                                      public int MaxRetries { get; set; } = 3;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineFieldInitializerAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS036")).IsTrue();
    }

    [Test]
    public async Task Analyze_MultipleVariablesWithInitializers_ReportsMultipleDiagnostics()
    {
        const string source = """
                              namespace MyApp {
                                  public class Counters {
                                      private int _a = 1, _b = 2;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineFieldInitializerAnalyzer(), source);

        await Assert.That(diagnostics.Count(d => d.Id == "ATXCS036")).IsEqualTo(2);
    }

    [Test]
    public async Task Analyze_RecordFieldWithInitializer_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public record Config {
                                      public int Timeout { get; init; } = 30;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineFieldInitializerAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS036")).IsTrue();
    }

    [Test]
    public async Task Analyze_StructFieldWithInitializer_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public struct Point {
                                      public int X = 0;
                                      public int Y = 0;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineFieldInitializerAnalyzer(), source);

        await Assert.That(diagnostics.Count(d => d.Id == "ATXCS036")).IsEqualTo(2);
    }

    [Test]
    public async Task Analyze_ConstField_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Constants {
                                      private const int MaxValue = 100;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineFieldInitializerAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS036")).IsFalse();
    }

    [Test]
    public async Task Analyze_FieldWithoutInitializer_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Service { }
                                  public class Consumer {
                                      private Service? _service;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineFieldInitializerAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS036")).IsFalse();
    }

    [Test]
    public async Task Analyze_StaticFieldWithoutInitializer_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Registry {
                                      private static int _count;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineFieldInitializerAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS036")).IsFalse();
    }

    [Test]
    public async Task Analyze_AutoPropertyWithoutInitializer_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Config {
                                      public int MaxRetries { get; set; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineFieldInitializerAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS036")).IsFalse();
    }

    [Test]
    public async Task Analyze_FullBodyProperty_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Config {
                                      private int _retries;
                                      public int MaxRetries
                                      {
                                          get { return _retries; }
                                          set { _retries = value; }
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineFieldInitializerAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS036")).IsFalse();
    }

    [Test]
    public async Task Analyze_MultipleVariablesSomeWithoutInitializers_ReportsOnlyInitialized()
    {
        const string source = """
                              namespace MyApp {
                                  public class Counters {
                                      private int _a = 1, _b;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineFieldInitializerAnalyzer(), source);

        await Assert.That(diagnostics.Count(d => d.Id == "ATXCS036")).IsEqualTo(1);
    }

    [Test]
    public async Task Analyze_PrimaryConstructorParameterCapture_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Point(int x, int y) {
                                      public int X { get; } = x;
                                      public int Y { get; } = y;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineFieldInitializerAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS036")).IsFalse();
    }

    [Test]
    public async Task Analyze_PrimaryConstructorFieldCapture_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Container(int capacity) {
                                      private int _capacity = capacity;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineFieldInitializerAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS036")).IsFalse();
    }

    [Test]
    public async Task Analyze_PrimaryConstructorWithNonParamInitializer_ReportsDiagnostic()
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Container(int capacity) {
                                      private int _capacity = capacity;
                                      private readonly List<int> _items = [];
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineFieldInitializerAnalyzer(), source);

        await Assert.That(diagnostics.Count(d => d.Id == "ATXCS036")).IsEqualTo(1);
    }
}
