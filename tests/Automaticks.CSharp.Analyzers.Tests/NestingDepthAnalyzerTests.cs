
using Automaticks.CSharp;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class NestingDepthAnalyzerTests
{
    [Test]
    public async Task Analyze_AbstractMethod_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public abstract class Foo {
                                      public abstract void Method();
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new NestingDepthAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.NestingDepth)).IsFalse();
    }

    [Test]
    public async Task Analyze_AsyncMethod_CorrectlyAnalyzed()
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public async Task Method() {
                                          await System.Threading.Tasks.Task.CompletedTask;
                                          if (true) { }
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new NestingDepthAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.NestingDepth)).IsFalse();
    }

    [Test]
    public async Task Analyze_DiagnosticMessage_ContainsMethodNameAndDepth()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void DeepMethod() {
                                          if (true) {
                                              if (true) {
                                                  if (true) {
                                                      if (true) {
                                                          if (true) {
                                                              if (true) { }
                                                          }
                                                      }
                                                  }
                                              }
                                          }
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new NestingDepthAnalyzer(), source);
        var message = diagnostics.Single(d => d.Id == DiagnosticIds.CSharp.NestingDepth)
                                 .GetMessage(CultureInfo.InvariantCulture);

        await Assert.That(message).IsEqualTo(
            "Method 'DeepMethod' has a nesting depth of 6, which exceeds the maximum of 5");
    }

    [Test]
    public async Task Analyze_DoWhileLoop_IncrementsDepth()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method() {
                                          if (true) {
                                              if (true) {
                                                  if (true) {
                                                      if (true) {
                                                          if (true) {
                                                              do { } while (false);
                                                          }
                                                      }
                                                  }
                                              }
                                          }
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new NestingDepthAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.NestingDepth)).IsTrue();
    }

    [Test]
    public async Task Analyze_ElseBlock_IncrementsDepth()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method() {
                                          if (true) {
                                              if (true) {
                                                  if (true) {
                                                      if (true) {
                                                          if (true) {
                                                          } else { if (true) { } }
                                                      }
                                                  }
                                              }
                                          }
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new NestingDepthAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.NestingDepth)).IsTrue();
    }

    [Test]
    public async Task Analyze_ElseIfChain_DoesNotIncrementDepthBeyondIf()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method(int x) {
                                          if (x == 1) { }
                                          else if (x == 2) { }
                                          else if (x == 3) { }
                                          else if (x == 4) { }
                                          else if (x == 5) { }
                                          else if (x == 6) { }
                                          else if (x == 7) { }
                                          else if (x == 8) { }
                                          else { }
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new NestingDepthAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.NestingDepth)).IsFalse();
    }

    [Test]
    public async Task Analyze_EmptyMethod_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new NestingDepthAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.NestingDepth)).IsFalse();
    }

    [Test]
    public async Task Analyze_ExpressionBodiedMethod_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Method() => 42;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new NestingDepthAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.NestingDepth)).IsFalse();
    }

    [Test]
    public async Task Analyze_ExpressionBodiedMethodWithConditional_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Method(bool flag) => flag ? 1 : 0;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new NestingDepthAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.NestingDepth)).IsFalse();
    }

    [Test]
    public async Task Analyze_FinallyBlock_IncrementsDepth()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method() {
                                          if (true) {
                                              if (true) {
                                                  if (true) {
                                                      if (true) {
                                                          try { }
                                                          finally { if (true) { } }
                                                      }
                                                  }
                                              }
                                          }
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new NestingDepthAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.NestingDepth)).IsTrue();
    }

    [Test]
    public async Task Analyze_FiveDeepNesting_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method() {
                                          if (true) {
                                              if (true) {
                                                  if (true) {
                                                      if (true) {
                                                          if (true) { }
                                                      }
                                                  }
                                              }
                                          }
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new NestingDepthAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.NestingDepth)).IsFalse();
    }

    [Test]
    public async Task Analyze_ForEachLoop_IncrementsDepth()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method(int[] items) {
                                          if (true) {
                                              if (true) {
                                                  if (true) {
                                                      if (true) {
                                                          if (true) {
                                                              foreach (var x in items) { }
                                                          }
                                                      }
                                                  }
                                              }
                                          }
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new NestingDepthAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.NestingDepth)).IsTrue();
    }

    [Test]
    public async Task Analyze_ForLoop_IncrementsDepth()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method() {
                                          if (true) {
                                              if (true) {
                                                  if (true) {
                                                      if (true) {
                                                          if (true) {
                                                              for (var i = 0; i < 1; i++) { }
                                                          }
                                                      }
                                                  }
                                              }
                                          }
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new NestingDepthAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.NestingDepth)).IsTrue();
    }

    [Test]
    public async Task Analyze_Lambda_IncrementsDepth()
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public void Method() {
                                          if (true) {
                                              if (true) {
                                                  if (true) {
                                                      if (true) {
                                                          if (true) {
                                                              Action a = () => { if (true) { } };
                                                          }
                                                      }
                                                  }
                                              }
                                          }
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new NestingDepthAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.NestingDepth)).IsTrue();
    }

    [Test]
    public async Task Analyze_LocalFunction_IncrementsDepth()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method() {
                                          if (true) {
                                              if (true) {
                                                  if (true) {
                                                      if (true) {
                                                          if (true) {
                                                              void Local() { if (true) { } }
                                                              Local();
                                                          }
                                                      }
                                                  }
                                              }
                                          }
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new NestingDepthAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.NestingDepth)).IsTrue();
    }

    [Test]
    public async Task Analyze_LockStatement_IncrementsDepth()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private readonly object _lock = new object();
                                      public void Method() {
                                          if (true) {
                                              if (true) {
                                                  if (true) {
                                                      if (true) {
                                                          if (true) {
                                                              lock (_lock) { }
                                                          }
                                                      }
                                                  }
                                              }
                                          }
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new NestingDepthAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.NestingDepth)).IsTrue();
    }

    [Test]
    public async Task Analyze_PatternMatchingSwitchExpression_CorrectlyAnalyzed()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method(object x) {
                                          if (true) {
                                              if (true) {
                                                  if (true) {
                                                      if (true) {
                                                          if (true) {
                                                              var r = x switch { _ => 0 };
                                                          }
                                                      }
                                                  }
                                              }
                                          }
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new NestingDepthAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.NestingDepth)).IsTrue();
    }

    [Test]
    public async Task Analyze_SingleIfStatement_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method() {
                                          if (true) { }
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new NestingDepthAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.NestingDepth)).IsFalse();
    }

    [Test]
    public async Task Analyze_SixDeepNesting_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method() {
                                          if (true) {
                                              if (true) {
                                                  if (true) {
                                                      if (true) {
                                                          if (true) {
                                                              if (true) { }
                                                          }
                                                      }
                                                  }
                                              }
                                          }
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new NestingDepthAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.NestingDepth)).IsTrue();
    }

    [Test]
    public async Task Analyze_SwitchStatement_IncrementsDepth()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method(int x) {
                                          if (true) {
                                              if (true) {
                                                  if (true) {
                                                      if (true) {
                                                          if (true) {
                                                              switch (x) { default: break; }
                                                          }
                                                      }
                                                  }
                                              }
                                          }
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new NestingDepthAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.NestingDepth)).IsTrue();
    }

    [Test]
    public async Task Analyze_TryCatch_IncrementsDepth()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method() {
                                          if (true) {
                                              if (true) {
                                                  if (true) {
                                                      if (true) {
                                                          try { if (true) { } }
                                                          catch (System.Exception) { }
                                                      }
                                                  }
                                              }
                                          }
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new NestingDepthAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.NestingDepth)).IsTrue();
    }

    [Test]
    public async Task Analyze_UsingStatement_IncrementsDepth()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method() {
                                          if (true) {
                                              if (true) {
                                                  if (true) {
                                                      if (true) {
                                                          if (true) {
                                                              using (var r = new System.IO.MemoryStream()) { }
                                                          }
                                                      }
                                                  }
                                              }
                                          }
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new NestingDepthAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.NestingDepth)).IsTrue();
    }

    [Test]
    public async Task Analyze_WhileLoop_IncrementsDepth()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method() {
                                          if (true) {
                                              if (true) {
                                                  if (true) {
                                                      if (true) {
                                                          if (true) {
                                                              while (false) { }
                                                          }
                                                      }
                                                  }
                                              }
                                          }
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new NestingDepthAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.NestingDepth)).IsTrue();
    }
}
