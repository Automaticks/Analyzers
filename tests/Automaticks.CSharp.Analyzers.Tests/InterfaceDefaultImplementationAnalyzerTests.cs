using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class InterfaceDefaultImplementationAnalyzerTests
{
    [Test]
    public async Task Analyze_MethodWithBlockBody_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo {
                                      void Bar() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InterfaceDefaultImplementationAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS061")).IsTrue();
    }

    [Test]
    public async Task Analyze_MethodWithExpressionBody_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo {
                                      int Bar() => 42;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InterfaceDefaultImplementationAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS061")).IsTrue();
    }

    [Test]
    public async Task Analyze_StaticMethod_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo {
                                      static void Helper() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InterfaceDefaultImplementationAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS061")).IsTrue();
    }

    [Test]
    public async Task Analyze_StaticField_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo {
                                      static int Count = 0;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InterfaceDefaultImplementationAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS061")).IsTrue();
    }

    [Test]
    public async Task Analyze_PropertyWithAccessorBody_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo {
                                      int Value { get { return 1; } }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InterfaceDefaultImplementationAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS061")).IsTrue();
    }

    [Test]
    public async Task Analyze_PropertyWithExpressionBody_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo {
                                      int Value => 42;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InterfaceDefaultImplementationAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS061")).IsTrue();
    }

    [Test]
    public async Task Analyze_EventWithAccessorBodies_ReportsDiagnostic()
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public interface IFoo {
                                      event EventArgs Changed { add { } remove { } }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InterfaceDefaultImplementationAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS061")).IsTrue();
    }

    [Test]
    public async Task Analyze_AbstractMembersOnly_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo {
                                      int Value { get; set; }
                                      void Bar();
                                      string Name { get; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InterfaceDefaultImplementationAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS061")).IsFalse();
    }

    [Test]
    public async Task Analyze_ClassMethodWithBody_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InterfaceDefaultImplementationAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS061")).IsFalse();
    }

    [Test]
    public async Task Analyze_StaticMethodWithAbstractMethods_ReportsOnlyStaticMethod()
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo {
                                      void AbstractMethod();
                                      static void Helper() { }
                                      int Value { get; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InterfaceDefaultImplementationAnalyzer(), source);

        var matching = diagnostics.Where(d => d.Id == "ATXCS061").ToList();
        await Assert.That(matching.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Analyze_OperatorWithBody_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo {
                                      static IFoo operator +(IFoo a, IFoo b) { return a; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InterfaceDefaultImplementationAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS061")).IsTrue();
    }

    [Test]
    public async Task Analyze_OperatorWithExpressionBody_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo {
                                      static IFoo operator +(IFoo a, IFoo b) => a;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InterfaceDefaultImplementationAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS061")).IsTrue();
    }

    [Test]
    public async Task Analyze_ConversionOperatorWithExpressionBody_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo {
                                      static explicit operator int(IFoo foo) => 0;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InterfaceDefaultImplementationAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS061")).IsTrue();
    }

    [Test]
    public async Task Analyze_ConversionOperatorWithBlockBody_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo {
                                      static explicit operator int(IFoo foo) { return 0; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InterfaceDefaultImplementationAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS061")).IsTrue();
    }

    [Test]
    public async Task Analyze_IndexerWithExpressionBody_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo {
                                      int this[int i] => i;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InterfaceDefaultImplementationAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS061")).IsTrue();
    }

    [Test]
    public async Task Analyze_IndexerWithAccessorBody_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo {
                                      int this[int i] { get { return i; } }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InterfaceDefaultImplementationAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS061")).IsTrue();
    }

    [Test]
    public async Task Analyze_AbstractIndexer_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo {
                                      int this[int i] { get; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InterfaceDefaultImplementationAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS061")).IsFalse();
    }

    [Test]
    public async Task Analyze_StaticPropertyWithAccessorBody_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo {
                                      static int Value { get { return 42; } }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InterfaceDefaultImplementationAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS061")).IsTrue();
    }

    [Test]
    public async Task Analyze_MultipleMembers_ReportsOnePerOffendingMember()
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo {
                                      void Clean();
                                      void WithBody() { }
                                      int AlsoWithBody() => 42;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InterfaceDefaultImplementationAnalyzer(), source);

        var matching = diagnostics.Where(d => d.Id == "ATXCS061").ToList();
        await Assert.That(matching.Count).IsEqualTo(2);
    }

    // GAP 1 regression: static abstract members must NOT be flagged (they are abstract contracts, not implementations)
    [Test]
    public async Task Analyze_StaticAbstractMethod_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo<T> where T : IFoo<T> {
                                      static abstract T Create();
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InterfaceDefaultImplementationAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS061")).IsFalse();
    }

    [Test]
    public async Task Analyze_StaticAbstractProperty_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo<T> where T : IFoo<T> {
                                      static abstract T Zero { get; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InterfaceDefaultImplementationAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS061")).IsFalse();
    }

    [Test]
    public async Task Analyze_StaticAbstractAndStaticImplementation_ReportsOnlyImplementation()
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo<T> where T : IFoo<T> {
                                      static abstract T Zero { get; }
                                      static abstract T Create();
                                      static T Default() => default;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InterfaceDefaultImplementationAnalyzer(), source);

        var matching = diagnostics.Where(d => d.Id == "ATXCS061").ToList();
        await Assert.That(matching.Count).IsEqualTo(1);
    }

    // GAP 2 regression: static field-like events must be flagged
    [Test]
    public async Task Analyze_StaticFieldLikeEvent_ReportsDiagnostic()
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public interface IFoo {
                                      static event EventHandler Updated;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InterfaceDefaultImplementationAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS061")).IsTrue();
    }

    [Test]
    public async Task Analyze_AbstractFieldLikeEvent_ReportsNoDiagnostic()
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public interface IFoo {
                                      event EventHandler Changed;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InterfaceDefaultImplementationAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS061")).IsFalse();
    }
}
