using Automaticks.CSharp;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class CyclomaticComplexityAnalyzerTests
{
    [Test]
    public async Task Analyze_AbstractMethod_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public abstract class Foo {
                                      public abstract void Method(int a);
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CyclomaticComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS028")).IsFalse();
    }

    [Test]
    public async Task Analyze_CatchClause_IncrementsComplexity()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method(int a) {
                                          if (a == 1) {}
                                          if (a == 2) {}
                                          if (a == 3) {}
                                          if (a == 4) {}
                                          if (a == 5) {}
                                          if (a == 6) {}
                                          if (a == 7) {}
                                          if (a == 8) {}
                                          if (a == 9) {}
                                          if (a == 10) {}
                                          if (a == 11) {}
                                          if (a == 12) {}
                                          if (a == 13) {}
                                          if (a == 14) {}
                                          try { }
                                          catch (System.Exception) { }
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CyclomaticComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS028")).IsTrue();
    }

    [Test]
    public async Task Analyze_DiagnosticMessage_ContainsMethodNameAndComplexity()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void ComplexMethod(int a) {
                                          if (a == 1) {}
                                          if (a == 2) {}
                                          if (a == 3) {}
                                          if (a == 4) {}
                                          if (a == 5) {}
                                          if (a == 6) {}
                                          if (a == 7) {}
                                          if (a == 8) {}
                                          if (a == 9) {}
                                          if (a == 10) {}
                                          if (a == 11) {}
                                          if (a == 12) {}
                                          if (a == 13) {}
                                          if (a == 14) {}
                                          if (a == 15) {}
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CyclomaticComplexityAnalyzer(), source);
        var message = diagnostics.Single(d => d.Id == "ATXCS028")
                                 .GetMessage(CultureInfo.InvariantCulture);

        await Assert.That(message).IsEqualTo(
            "Method 'ComplexMethod' has a cyclomatic complexity of 16, which exceeds the maximum of 15");
    }

    [Test]
    public async Task Analyze_DoWhileLoop_IncrementsComplexity()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method() {
                                          do { } while (false);
                                          do { } while (false);
                                          do { } while (false);
                                          do { } while (false);
                                          do { } while (false);
                                          do { } while (false);
                                          do { } while (false);
                                          do { } while (false);
                                          do { } while (false);
                                          do { } while (false);
                                          do { } while (false);
                                          do { } while (false);
                                          do { } while (false);
                                          do { } while (false);
                                          do { } while (false);
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CyclomaticComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS028")).IsTrue();
    }

    [Test]
    public async Task Analyze_ElseIfChain_IncrementsComplexityPerBranch()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method(int a) {
                                          if (a == 1) {}
                                          else if (a == 2) {}
                                          else if (a == 3) {}
                                          else if (a == 4) {}
                                          else if (a == 5) {}
                                          else if (a == 6) {}
                                          else if (a == 7) {}
                                          else if (a == 8) {}
                                          else if (a == 9) {}
                                          else if (a == 10) {}
                                          else if (a == 11) {}
                                          else if (a == 12) {}
                                          else if (a == 13) {}
                                          else if (a == 14) {}
                                          else if (a == 15) {}
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CyclomaticComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS028")).IsTrue();
    }

    [Test]
    public async Task Analyze_ExpressionBodiedMethodAboveThreshold_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Method(bool a1, bool a2, bool a3, bool a4, bool a5,
                                                        bool a6, bool a7, bool a8, bool a9, bool a10,
                                                        bool a11, bool a12, bool a13, bool a14, bool a15) =>
                                          a1 ? 1 : a2 ? 2 : a3 ? 3 : a4 ? 4 : a5 ? 5 :
                                          a6 ? 6 : a7 ? 7 : a8 ? 8 : a9 ? 9 : a10 ? 10 :
                                          a11 ? 11 : a12 ? 12 : a13 ? 13 : a14 ? 14 : a15 ? 15 : 0;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CyclomaticComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS028")).IsTrue();
    }

    [Test]
    public async Task Analyze_ForeachLoop_IncrementsComplexity()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method(int[] items) {
                                          foreach (var item in items) {}
                                          foreach (var item in items) {}
                                          foreach (var item in items) {}
                                          foreach (var item in items) {}
                                          foreach (var item in items) {}
                                          foreach (var item in items) {}
                                          foreach (var item in items) {}
                                          foreach (var item in items) {}
                                          foreach (var item in items) {}
                                          foreach (var item in items) {}
                                          foreach (var item in items) {}
                                          foreach (var item in items) {}
                                          foreach (var item in items) {}
                                          foreach (var item in items) {}
                                          foreach (var item in items) {}
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CyclomaticComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS028")).IsTrue();
    }

    [Test]
    public async Task Analyze_ForLoop_IncrementsComplexity()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method() {
                                          for (var index = 0; index < 1; index++) {}
                                          for (var index = 0; index < 1; index++) {}
                                          for (var index = 0; index < 1; index++) {}
                                          for (var index = 0; index < 1; index++) {}
                                          for (var index = 0; index < 1; index++) {}
                                          for (var index = 0; index < 1; index++) {}
                                          for (var index = 0; index < 1; index++) {}
                                          for (var index = 0; index < 1; index++) {}
                                          for (var index = 0; index < 1; index++) {}
                                          for (var index = 0; index < 1; index++) {}
                                          for (var index = 0; index < 1; index++) {}
                                          for (var index = 0; index < 1; index++) {}
                                          for (var index = 0; index < 1; index++) {}
                                          for (var index = 0; index < 1; index++) {}
                                          for (var index = 0; index < 1; index++) {}
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CyclomaticComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS028")).IsTrue();
    }

    [Test]
    public async Task Analyze_IfStatement_IncrementsComplexity()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method(int a) {
                                          if (a == 1) {}
                                          if (a == 2) {}
                                          if (a == 3) {}
                                          if (a == 4) {}
                                          if (a == 5) {}
                                          if (a == 6) {}
                                          if (a == 7) {}
                                          if (a == 8) {}
                                          if (a == 9) {}
                                          if (a == 10) {}
                                          if (a == 11) {}
                                          if (a == 12) {}
                                          if (a == 13) {}
                                          if (a == 14) {}
                                          if (a == 15) {}
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CyclomaticComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS028")).IsTrue();
    }

    [Test]
    public async Task Analyze_LambdaComplexityRollsUpToEnclosingMethod_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method(int a) {
                                          if (a == 1) {}
                                          if (a == 2) {}
                                          if (a == 3) {}
                                          if (a == 4) {}
                                          if (a == 5) {}
                                          if (a == 6) {}
                                          if (a == 7) {}
                                          if (a == 8) {}
                                          System.Action action = () => {
                                              if (a == 9) {}
                                              if (a == 10) {}
                                              if (a == 11) {}
                                              if (a == 12) {}
                                              if (a == 13) {}
                                              if (a == 14) {}
                                              if (a == 15) {}
                                          };
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CyclomaticComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS028")).IsTrue();
    }

    [Test]
    public async Task Analyze_LocalFunctionComplexityRollsUpToEnclosingMethod_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method(int a) {
                                          if (a == 1) {}
                                          if (a == 2) {}
                                          if (a == 3) {}
                                          if (a == 4) {}
                                          if (a == 5) {}
                                          if (a == 6) {}
                                          if (a == 7) {}
                                          if (a == 8) {}
                                          void LocalHelper(int b) {
                                              if (b == 9) {}
                                              if (b == 10) {}
                                              if (b == 11) {}
                                              if (b == 12) {}
                                              if (b == 13) {}
                                              if (b == 14) {}
                                              if (b == 15) {}
                                          }
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CyclomaticComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS028")).IsTrue();
    }

    [Test]
    public async Task Analyze_LogicalAnd_IncrementsComplexity()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool Method(bool a1, bool a2, bool a3, bool a4, bool a5,
                                                         bool a6, bool a7, bool a8, bool a9, bool a10,
                                                         bool a11, bool a12, bool a13, bool a14, bool a15, bool a16) =>
                                          a1 && a2 && a3 && a4 && a5 && a6 && a7 && a8 &&
                                          a9 && a10 && a11 && a12 && a13 && a14 && a15 && a16;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CyclomaticComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS028")).IsTrue();
    }

    [Test]
    public async Task Analyze_LogicalOr_IncrementsComplexity()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool Method(bool a1, bool a2, bool a3, bool a4, bool a5,
                                                         bool a6, bool a7, bool a8, bool a9, bool a10,
                                                         bool a11, bool a12, bool a13, bool a14, bool a15, bool a16) =>
                                          a1 || a2 || a3 || a4 || a5 || a6 || a7 || a8 ||
                                          a9 || a10 || a11 || a12 || a13 || a14 || a15 || a16;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CyclomaticComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS028")).IsTrue();
    }

    [Test]
    public async Task Analyze_MethodWithComplexityAboveThreshold_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method(int a) {
                                          if (a == 1) {}
                                          if (a == 2) {}
                                          if (a == 3) {}
                                          if (a == 4) {}
                                          if (a == 5) {}
                                          if (a == 6) {}
                                          if (a == 7) {}
                                          if (a == 8) {}
                                          if (a == 9) {}
                                          if (a == 10) {}
                                          if (a == 11) {}
                                          if (a == 12) {}
                                          if (a == 13) {}
                                          if (a == 14) {}
                                          if (a == 15) {}
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CyclomaticComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS028")).IsTrue();
    }

    [Test]
    public async Task Analyze_MethodWithComplexityAtThreshold_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method(int a) {
                                          if (a == 1) {}
                                          if (a == 2) {}
                                          if (a == 3) {}
                                          if (a == 4) {}
                                          if (a == 5) {}
                                          if (a == 6) {}
                                          if (a == 7) {}
                                          if (a == 8) {}
                                          if (a == 9) {}
                                          if (a == 10) {}
                                          if (a == 11) {}
                                          if (a == 12) {}
                                          if (a == 13) {}
                                          if (a == 14) {}
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CyclomaticComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS028")).IsFalse();
    }

    [Test]
    public async Task Analyze_MethodWithLowComplexity_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method(int a) {
                                          if (a == 1) {}
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CyclomaticComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS028")).IsFalse();
    }

    [Test]
    public async Task Analyze_NullCoalescing_IncrementsComplexity()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public string Method(string? s1, string? s2, string? s3, string? s4, string? s5,
                                                           string? s6, string? s7, string? s8, string? s9, string? s10,
                                                           string? s11, string? s12, string? s13, string? s14, string? s15, string? s16) =>
                                          s1 ?? s2 ?? s3 ?? s4 ?? s5 ?? s6 ?? s7 ?? s8 ??
                                          s9 ?? s10 ?? s11 ?? s12 ?? s13 ?? s14 ?? s15 ?? s16 ?? string.Empty;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CyclomaticComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS028")).IsTrue();
    }

    [Test]
    public async Task Analyze_NullCoalescingAssignment_IncrementsComplexity()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method() {
                                          string? value1 = null;
                                          string? value2 = null;
                                          string? value3 = null;
                                          string? value4 = null;
                                          string? value5 = null;
                                          string? value6 = null;
                                          string? value7 = null;
                                          string? value8 = null;
                                          string? value9 = null;
                                          string? value10 = null;
                                          string? value11 = null;
                                          string? value12 = null;
                                          string? value13 = null;
                                          string? value14 = null;
                                          string? value15 = null;
                                          value1 ??= "a";
                                          value2 ??= "b";
                                          value3 ??= "c";
                                          value4 ??= "d";
                                          value5 ??= "e";
                                          value6 ??= "f";
                                          value7 ??= "g";
                                          value8 ??= "h";
                                          value9 ??= "i";
                                          value10 ??= "j";
                                          value11 ??= "k";
                                          value12 ??= "l";
                                          value13 ??= "m";
                                          value14 ??= "n";
                                          value15 ??= "o";
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CyclomaticComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS028")).IsTrue();
    }

    [Test]
    public async Task Analyze_OperatorAboveThreshold_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Vec {
                                      public int Value { get; set; }
                                      public static Vec operator +(Vec left, Vec right) {
                                          if (left.Value == 1) {}
                                          if (left.Value == 2) {}
                                          if (left.Value == 3) {}
                                          if (left.Value == 4) {}
                                          if (left.Value == 5) {}
                                          if (left.Value == 6) {}
                                          if (left.Value == 7) {}
                                          if (left.Value == 8) {}
                                          if (left.Value == 9) {}
                                          if (left.Value == 10) {}
                                          if (left.Value == 11) {}
                                          if (left.Value == 12) {}
                                          if (left.Value == 13) {}
                                          if (left.Value == 14) {}
                                          if (left.Value == 15) {}
                                          return new Vec();
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CyclomaticComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS028")).IsTrue();
    }

    [Test]
    public async Task Analyze_PropertyGetterAboveThreshold_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private int _value;
                                      public int Value {
                                          get {
                                              if (_value == 1) {}
                                              if (_value == 2) {}
                                              if (_value == 3) {}
                                              if (_value == 4) {}
                                              if (_value == 5) {}
                                              if (_value == 6) {}
                                              if (_value == 7) {}
                                              if (_value == 8) {}
                                              if (_value == 9) {}
                                              if (_value == 10) {}
                                              if (_value == 11) {}
                                              if (_value == 12) {}
                                              if (_value == 13) {}
                                              if (_value == 14) {}
                                              if (_value == 15) {}
                                              return _value;
                                          }
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CyclomaticComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS028")).IsTrue();
    }

    [Test]
    public async Task Analyze_PropertySetterAboveThreshold_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private int _value;
                                      public int Value {
                                          set {
                                              if (value == 1) {}
                                              if (value == 2) {}
                                              if (value == 3) {}
                                              if (value == 4) {}
                                              if (value == 5) {}
                                              if (value == 6) {}
                                              if (value == 7) {}
                                              if (value == 8) {}
                                              if (value == 9) {}
                                              if (value == 10) {}
                                              if (value == 11) {}
                                              if (value == 12) {}
                                              if (value == 13) {}
                                              if (value == 14) {}
                                              if (value == 15) {}
                                              _value = value;
                                          }
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CyclomaticComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS028")).IsTrue();
    }

    [Test]
    public async Task Analyze_SwitchCaseLabel_IncrementsComplexity()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method(int x) {
                                          switch (x) {
                                              case 1: break;
                                              case 2: break;
                                              case 3: break;
                                              case 4: break;
                                              case 5: break;
                                              case 6: break;
                                              case 7: break;
                                              case 8: break;
                                              case 9: break;
                                              case 10: break;
                                              case 11: break;
                                              case 12: break;
                                              case 13: break;
                                              case 14: break;
                                              case 15: break;
                                          }
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CyclomaticComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS028")).IsTrue();
    }

    [Test]
    public async Task Analyze_SwitchDefaultLabel_DoesNotIncrementComplexity()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method(int x) {
                                          switch (x) {
                                              case 1: break;
                                              case 2: break;
                                              case 3: break;
                                              case 4: break;
                                              case 5: break;
                                              case 6: break;
                                              case 7: break;
                                              case 8: break;
                                              case 9: break;
                                              default: break;
                                          }
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CyclomaticComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS028")).IsFalse();
    }

    [Test]
    public async Task Analyze_SwitchExpressionArm_IncrementsComplexity()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Method(int x) =>
                                          x switch {
                                              1 => 1,
                                              2 => 2,
                                              3 => 3,
                                              4 => 4,
                                              5 => 5,
                                              6 => 6,
                                              7 => 7,
                                              8 => 8,
                                              9 => 9,
                                              10 => 10,
                                              11 => 11,
                                              12 => 12,
                                              13 => 13,
                                              14 => 14,
                                              _ => 0,
                                          };
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CyclomaticComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS028")).IsTrue();
    }

    [Test]
    public async Task Analyze_TernaryExpression_IncrementsComplexity()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Method(bool a1, bool a2, bool a3, bool a4, bool a5,
                                                        bool a6, bool a7, bool a8, bool a9, bool a10,
                                                        bool a11, bool a12, bool a13, bool a14, bool a15) =>
                                          a1 ? 1 : a2 ? 2 : a3 ? 3 : a4 ? 4 : a5 ? 5 :
                                          a6 ? 6 : a7 ? 7 : a8 ? 8 : a9 ? 9 : a10 ? 10 :
                                          a11 ? 11 : a12 ? 12 : a13 ? 13 : a14 ? 14 : a15 ? 15 : 0;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CyclomaticComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS028")).IsTrue();
    }

    [Test]
    public async Task Analyze_WhenClause_IncrementsComplexity()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Method(int a) {
                                          if (a == 1) {}
                                          if (a == 2) {}
                                          if (a == 3) {}
                                          if (a == 4) {}
                                          if (a == 5) {}
                                          if (a == 6) {}
                                          if (a == 7) {}
                                          if (a == 8) {}
                                          if (a == 9) {}
                                          if (a == 10) {}
                                          if (a == 11) {}
                                          if (a == 12) {}
                                          if (a == 13) {}
                                          if (a == 14) {}
                                          switch (a) {
                                              case int n when n == 15: return n;
                                          }
                                          return 0;
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CyclomaticComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS028")).IsTrue();
    }

    [Test]
    public async Task Analyze_WhileLoop_IncrementsComplexity()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method() {
                                          while (false) {}
                                          while (false) {}
                                          while (false) {}
                                          while (false) {}
                                          while (false) {}
                                          while (false) {}
                                          while (false) {}
                                          while (false) {}
                                          while (false) {}
                                          while (false) {}
                                          while (false) {}
                                          while (false) {}
                                          while (false) {}
                                          while (false) {}
                                          while (false) {}
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new CyclomaticComplexityAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS028")).IsTrue();
    }
}
