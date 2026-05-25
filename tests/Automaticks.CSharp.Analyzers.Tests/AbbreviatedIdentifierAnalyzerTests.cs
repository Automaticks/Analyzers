using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class AbbreviatedIdentifierAnalyzerTests
{
    [Test]
    public async Task Analyze_AxisVariableX_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(int positionX) {
                                          var x = positionX;
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AbbreviatedIdentifierAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS017")).IsFalse();
    }

    [Test]
    public async Task Analyze_AxisVariableY_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(int positionY) {
                                          var y = positionY;
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AbbreviatedIdentifierAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS017")).IsFalse();
    }

    [Test]
    public async Task Analyze_AxisVariableZ_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(int positionZ) {
                                          var z = positionZ;
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AbbreviatedIdentifierAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS017")).IsFalse();
    }

    [Test]
    public async Task Analyze_CamelCaseWithAbbreviatedSegment_ReportsDiagnostic()
    {
        const string source = """
                              using System.Threading;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(CancellationTokenSource source) {
                                          var walkCts = source;
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AbbreviatedIdentifierAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS017")).IsTrue();
    }

    [Test]
    public async Task Analyze_CamelCaseWithFullWords_ReportsNoDiagnostic()
    {
        const string source = """
                              using System.Threading;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(CancellationTokenSource source) {
                                          var walkCancellationTokenSource = source;
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AbbreviatedIdentifierAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS017")).IsFalse();
    }

    [Test]
    public async Task Analyze_ClassWithAbbreviatedSegment_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class CtsManager { }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AbbreviatedIdentifierAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS017")).IsTrue();
    }

    [Test]
    public async Task Analyze_FieldWithAbbreviatedName_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private int _sb;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AbbreviatedIdentifierAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS017")).IsTrue();
    }

    [Test]
    public async Task Analyze_FieldWithFullName_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private int _stringBuilder;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AbbreviatedIdentifierAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS017")).IsFalse();
    }

    [Test]
    public async Task Analyze_ForEachVariableWithAbbreviatedName_ReportsDiagnostic()
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(List<string> spawns) {
                                          foreach (var s in spawns) { }
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AbbreviatedIdentifierAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS017")).IsTrue();
    }

    [Test]
    public async Task Analyze_ForEachVariableWithFullName_ReportsNoDiagnostic()
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(List<string> items) {
                                          foreach (var spawn in items) { }
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AbbreviatedIdentifierAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS017")).IsFalse();
    }

    [Test]
    public async Task Analyze_InterfaceWithAbbreviatedSegment_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public interface ICtxProvider { }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AbbreviatedIdentifierAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS017")).IsTrue();
    }

    [Test]
    public async Task Analyze_LocalVariableWithAbbreviatedName_ReportsDiagnostic()
    {
        const string source = """
                              using System.Threading;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(CancellationToken token) {
                                          var ct = token;
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AbbreviatedIdentifierAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS017")).IsTrue();
    }

    [Test]
    public async Task Analyze_LocalVariableWithFullName_ReportsNoDiagnostic()
    {
        const string source = """
                              using System.Threading;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(CancellationToken token) {
                                          var cancellationToken = token;
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AbbreviatedIdentifierAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS017")).IsFalse();
    }

    [Test]
    public async Task Analyze_MethodWithAbbreviatedName_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void GetVm() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AbbreviatedIdentifierAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS017")).IsTrue();
    }

    [Test]
    public async Task Analyze_MethodWithFullName_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void GetViewModel() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AbbreviatedIdentifierAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS017")).IsFalse();
    }

    [Test]
    public async Task Analyze_ParameterWithAbbreviatedName_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(int ct) { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AbbreviatedIdentifierAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS017")).IsTrue();
    }

    [Test]
    public async Task Analyze_ParameterWithFullName_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(int count) { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AbbreviatedIdentifierAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS017")).IsFalse();
    }

    [Test]
    public async Task Analyze_PatternMatchVariableWithAbbreviatedName_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(object obj) {
                                          if (obj is string str) { }
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AbbreviatedIdentifierAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS017")).IsTrue();
    }

    [Test]
    public async Task Analyze_ShortWordWithVowels_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() {
                                          var key = 0;
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AbbreviatedIdentifierAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS017")).IsFalse();
    }

    [Test]
    public async Task Analyze_SingleLetterVariable_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() {
                                          var i = 0;
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AbbreviatedIdentifierAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS017")).IsTrue();
    }

    [Test]
    public async Task Analyze_PropertyWithAbbreviatedName_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public string Vm { get; set; } = string.Empty;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AbbreviatedIdentifierAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS017")).IsTrue();
    }

    [Test]
    public async Task Analyze_PropertyWithFullName_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public string ViewModel { get; set; } = string.Empty;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AbbreviatedIdentifierAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS017")).IsFalse();
    }

    [Test]
    public async Task Analyze_ExternalPropertyOverride_ReportsNoDiagnostic()
    {
        const string externalSource = """
                                      public abstract class Base {
                                          public abstract string Vm { get; }
                                      }
                                      """;

        const string source = """
                              namespace MyApp {
                                  public class Derived : Base {
                                      public override string Vm { get; } = string.Empty;
                                  }
                              }
                              """;

        var externalRef = AnalyzerTestRunner.CompileToReference(externalSource);
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AbbreviatedIdentifierAnalyzer(), source, [externalRef]);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS017")).IsFalse();
    }
}
