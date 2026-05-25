using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class InternalModifierAnalyzerTests
{
    [Test]
    public async Task Analyze_InternalClass_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  internal class Foo {}
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InternalModifierAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS013")).IsTrue();
    }

    [Test]
    public async Task Analyze_InternalClassInTestProject_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp.Tests {
                                  internal class FooTests {}
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InternalModifierAnalyzer(), source, true);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS013")).IsTrue();
    }

    [Test]
    public async Task Analyze_InternalEnum_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  internal enum Foo { A, B }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InternalModifierAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS013")).IsTrue();
    }

    [Test]
    public async Task Analyze_InternalEvent_ReportsDiagnostic()
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      internal event Action Bar;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InternalModifierAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS013")).IsTrue();
    }

    [Test]
    public async Task Analyze_InternalField_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      internal int _bar;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InternalModifierAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS013")).IsTrue();
    }

    [Test]
    public async Task Analyze_InternalInterface_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  internal interface IFoo {}
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InternalModifierAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS013")).IsTrue();
    }

    [Test]
    public async Task Analyze_InternalMethod_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      internal void Bar() {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InternalModifierAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS013")).IsTrue();
    }

    [Test]
    public async Task Analyze_InternalProperty_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      internal int Bar { get; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InternalModifierAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS013")).IsTrue();
    }

    [Test]
    public async Task Analyze_InternalStruct_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  internal struct Foo {}
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InternalModifierAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS013")).IsTrue();
    }

    [Test]
    public async Task Analyze_PrivateMethod_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private void Bar() {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InternalModifierAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS013")).IsFalse();
    }

    [Test]
    public async Task Analyze_PublicClass_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {}
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InternalModifierAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS013")).IsFalse();
    }
}
