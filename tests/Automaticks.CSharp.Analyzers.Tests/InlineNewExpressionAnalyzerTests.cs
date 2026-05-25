using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class InlineNewExpressionAnalyzerTests
{

    [Test]
    public async Task Analyze_ObjectCreationAsMethodArgument_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class DamageInfo { }
                                  public class Player {
                                      public void TakeDamage(DamageInfo info) { }
                                      public void Run() {
                                          TakeDamage(new DamageInfo());
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineNewExpressionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS058")).IsTrue();
    }

    [Test]
    public async Task Analyze_ObjectCreationAsConstructorArgument_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Health { }
                                  public class Player {
                                      public Player(Health health) { }
                                  }
                                  public class Factory {
                                      public void Run() {
                                          var p = new Player(new Health());
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineNewExpressionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS058")).IsTrue();
    }

    [Test]
    public async Task Analyze_ArrayCreationAsMethodArgument_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Service {
                                      public void Process(int[] data) { }
                                      public void Run() {
                                          Process(new int[5]);
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineNewExpressionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS058")).IsTrue();
    }

    [Test]
    public async Task Analyze_ImplicitArrayCreationAsMethodArgument_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Service {
                                      public void Process(int[] data) { }
                                      public void Run() {
                                          Process(new[] { 1, 2, 3 });
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineNewExpressionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS058")).IsTrue();
    }

    [Test]
    public async Task Analyze_AnonymousObjectCreationAsMethodArgument_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Service {
                                      public void Process(object data) { }
                                      public void Run() {
                                          Process(new { X = 1 });
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineNewExpressionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS058")).IsTrue();
    }

    [Test]
    public async Task Analyze_ImplicitObjectCreationAsMethodArgument_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { }
                                  public class Service {
                                      public void Process(Foo f) { }
                                      public void Run() {
                                          Process(new());
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineNewExpressionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS058")).IsTrue();
    }

    [Test]
    public async Task Analyze_ObjectCreationInFieldInitializer_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Health { }
                                  public class Player {
                                      private Health _health = new Health();
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineNewExpressionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS058")).IsTrue();
    }

    [Test]
    public async Task Analyze_ObjectCreationInPropertyInitializer_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Health { }
                                  public class Player {
                                      public Health H { get; } = new Health();
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineNewExpressionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS058")).IsTrue();
    }

    [Test]
    public async Task Analyze_ObjectCreationInConditionalExpression_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { }
                                  public class Service {
                                      private Foo _field;
                                      public void Run(bool flag) {
                                          _field = flag ? new Foo() : new Foo();
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineNewExpressionAnalyzer(), source);

        await Assert.That(diagnostics.Count(d => d.Id == "ATXCS058")).IsEqualTo(2);
    }

    [Test]
    public async Task Analyze_ObjectCreationInThrowStatement_ReportsNoDiagnostic()
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Service {
                                      public void Run() {
                                          throw new InvalidOperationException();
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineNewExpressionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS058")).IsFalse();
    }

    [Test]
    public async Task Analyze_ObjectCreationInThrowExpression_ReportsNoDiagnostic()
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Service {
                                      private string _value = null!;
                                      public string Value => _value ?? throw new InvalidOperationException();
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineNewExpressionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS058")).IsFalse();
    }

    [Test]
    public async Task Analyze_ObjectCreationAsObjectInitializerValue_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Health { }
                                  public class Player {
                                      public Health H { get; set; }
                                  }
                                  public class Factory {
                                      public void Run() {
                                          var player = new Player { H = new Health() };
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineNewExpressionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS058")).IsTrue();
    }

    [Test]
    public async Task Analyze_ObjectCreationAsCollectionInitializerElement_ReportsDiagnostic()
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Foo { }
                                  public class Service {
                                      public void Run() {
                                          var list = new List<Foo> { new Foo() };
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineNewExpressionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS058")).IsTrue();
    }

    [Test]
    public async Task Analyze_ObjectCreationInnerNestedInLocalVarOuterConstructor_ReportsDiagnosticForInnerOnly()
    {
        const string source = """
                              namespace MyApp {
                                  public class Bar { }
                                  public class Foo {
                                      public Foo(Bar b) { }
                                  }
                                  public class Service {
                                      public void Run() {
                                          var x = new Foo(new Bar());
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineNewExpressionAnalyzer(), source);

        await Assert.That(diagnostics.Count(d => d.Id == "ATXCS058")).IsEqualTo(1);
    }

    [Test]
    public async Task Analyze_StandaloneLocalVarDeclaration_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { }
                                  public class Service {
                                      public void Run() {
                                          var x = new Foo();
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineNewExpressionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS058")).IsFalse();
    }

    [Test]
    public async Task Analyze_ExplicitTypeLocalVarDeclaration_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { }
                                  public class Service {
                                      public void Run() {
                                          Foo x = new Foo();
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineNewExpressionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS058")).IsFalse();
    }

    [Test]
    public async Task Analyze_ImplicitObjectCreationInLocalVarDeclaration_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { }
                                  public class Service {
                                      public void Run() {
                                          Foo x = new();
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineNewExpressionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS058")).IsFalse();
    }

    [Test]
    public async Task Analyze_ObjectInitializerOnLocalVar_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int X { get; set; }
                                  }
                                  public class Service {
                                      public void Run() {
                                          var x = new Foo { X = 1 };
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineNewExpressionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS058")).IsFalse();
    }

    [Test]
    public async Task Analyze_ObjectCreationInReturnStatement_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { }
                                  public class Service {
                                      public Foo Create() {
                                          return new Foo();
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineNewExpressionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS058")).IsFalse();
    }

    [Test]
    public async Task Analyze_ObjectCreationInExpressionBodiedMethod_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { }
                                  public class Service {
                                      public Foo Create() => new Foo();
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineNewExpressionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS058")).IsFalse();
    }

    [Test]
    public async Task Analyze_LocalVarInsideLambda_ReportsNoDiagnostic()
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo { }
                                  public class Service {
                                      public void Run() {
                                          Action a = () => {
                                              var x = new Foo();
                                          };
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineNewExpressionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS058")).IsFalse();
    }

    [Test]
    public async Task Analyze_ObjectCreationInsideAttributeArgument_ReportsNoDiagnostic()
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class MyAttr : Attribute {
                                      public MyAttr(Type[] types) { }
                                  }
                                  [MyAttr(new[] { typeof(int) })]
                                  public class Service { }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineNewExpressionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS058")).IsFalse();
    }

    [Test]
    public async Task Analyze_ObjectCreationInYieldReturn_ReportsNoDiagnostic()
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Foo { }
                                  public class Service {
                                      public IEnumerable<Foo> GetFoos() {
                                          yield return new Foo();
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineNewExpressionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS058")).IsFalse();
    }

    [Test]
    public async Task Analyze_ObjectCreationInForLoopVariable_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { }
                                  public class Service {
                                      public void Run() {
                                          for (var x = new Foo(); false; ) { }
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineNewExpressionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS058")).IsFalse();
    }

    [Test]
    public async Task Analyze_ObjectCreationInUsingStatementVariable_ReportsNoDiagnostic()
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo : IDisposable {
                                      public void Dispose() { }
                                  }
                                  public class Service {
                                      public void Run() {
                                          using (var x = new Foo()) { }
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineNewExpressionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS058")).IsFalse();
    }

    [Test]
    public async Task Analyze_ObjectCreationInUsingVarDeclaration_ReportsNoDiagnostic()
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo : IDisposable {
                                      public void Dispose() { }
                                  }
                                  public class Service {
                                      public void Run() {
                                          using var x = new Foo();
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineNewExpressionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS058")).IsFalse();
    }

    [Test]
    public async Task Analyze_ObjectCreationInParenthesizedLocalVar_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { }
                                  public class Service {
                                      public void Run() {
                                          var x = (new Foo());
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineNewExpressionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS058")).IsFalse();
    }

    [Test]
    public async Task Analyze_ObjectCreationInNullForgivingLocalVar_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { }
                                  public class Service {
                                      public void Run() {
                                          var x = new Foo()!;
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineNewExpressionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS058")).IsFalse();
    }

    [Test]
    public async Task Analyze_ArrayCreationInLocalVar_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Service {
                                      public void Run() {
                                          var arr = new int[5];
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineNewExpressionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS058")).IsFalse();
    }

    [Test]
    public async Task Analyze_ObjectCreationInSwitchExpressionArmInReturn_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Base { }
                                  public class TypeA : Base { }
                                  public class TypeB : Base { }
                                  public class Service {
                                      public Base Map(bool flag) {
                                          return flag switch {
                                              true => new TypeA(),
                                              false => new TypeB(),
                                          };
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineNewExpressionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS058")).IsFalse();
    }

    [Test]
    public async Task Analyze_ObjectCreationInSwitchExpressionArmInLocalVar_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Base { }
                                  public class TypeA : Base { }
                                  public class TypeB : Base { }
                                  public class Service {
                                      public void Run(bool flag) {
                                          var result = flag switch {
                                              true => new TypeA(),
                                              _ => new TypeB(),
                                          };
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineNewExpressionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS058")).IsFalse();
    }

    [Test]
    public async Task Analyze_ObjectCreationInSwitchExpressionArmAsMethodArgument_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Base { }
                                  public class TypeA : Base { }
                                  public class TypeB : Base { }
                                  public class Service {
                                      public void Accept(Base b) { }
                                      public void Run(bool flag) {
                                          Accept(flag switch {
                                              true => new TypeA(),
                                              _ => new TypeB(),
                                          });
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new InlineNewExpressionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS058")).IsFalse();
    }
}
