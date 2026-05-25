using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class GenericDelegateAnalyzerTests
{
    [Test]
    public async Task Analyze_ActionAsGenericTypeArgument_ReportsDiagnostic()
    {
        const string source = """
                              using System;
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Foo {
                                      private List<Action<int>> _callbacks = new();
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new GenericDelegateAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS020")).IsTrue();
    }

    [Test]
    public async Task Analyze_ActionParameter_ReportsDiagnostic()
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(Action callback)
                                      {
                                          callback();
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new GenericDelegateAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS020")).IsTrue();
    }

    [Test]
    public async Task Analyze_ActionReturnType_ReportsDiagnostic()
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public Action<int> GetCallback()
                                      {
                                          return _ => { };
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new GenericDelegateAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS020")).IsTrue();
    }

    [Test]
    public async Task Analyze_AliasUsageWhereAliasWrapsAction_ReportsDiagnostic()
    {
        const string source = """
                              using MyFunc = System.Func<int, bool>;
                              namespace MyApp {
                                  public class Foo {
                                      private MyFunc _filter;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new GenericDelegateAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS020")).IsTrue();
    }

    [Test]
    public async Task Analyze_ComparisonParameter_ReportsDiagnostic()
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public void Sort(Comparison<int> comparison)
                                      {
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new GenericDelegateAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS020")).IsTrue();
    }

    [Test]
    public async Task Analyze_ConverterLocalVariable_ReportsDiagnostic()
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar()
                                      {
                                          Converter<int, string> conv = x => x.ToString();
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new GenericDelegateAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS020")).IsTrue();
    }

    [Test]
    public async Task Analyze_CustomDelegateField_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public delegate void MyCallback(int x);
                                  public class Foo {
                                      private MyCallback _callback;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new GenericDelegateAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS020")).IsFalse();
    }

    [Test]
    public async Task Analyze_CustomDelegateParameter_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public delegate bool MyPredicate(int x);
                                  public class Foo {
                                      public void Filter(MyPredicate predicate)
                                      {
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new GenericDelegateAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS020")).IsFalse();
    }

    [Test]
    public async Task Analyze_EventHandlerOnEvent_ReportsNoDiagnostic()
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public event EventHandler Closed;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new GenericDelegateAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS020")).IsFalse();
    }

    [Test]
    public async Task Analyze_FuncField_ReportsDiagnostic()
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      private Func<int> _provider;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new GenericDelegateAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS020")).IsTrue();
    }

    [Test]
    public async Task Analyze_FuncLocalVariable_ReportsDiagnostic()
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar()
                                      {
                                          Func<int, string> converter = x => x.ToString();
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new GenericDelegateAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS020")).IsTrue();
    }

    [Test]
    public async Task Analyze_FuncProperty_ReportsDiagnostic()
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public Func<int, bool> Filter { get; set; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new GenericDelegateAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS020")).IsTrue();
    }

    [Test]
    public async Task Analyze_GenericActionParameter_ReportsDiagnostic()
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(Action<int> callback)
                                      {
                                          callback(1);
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new GenericDelegateAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS020")).IsTrue();
    }

    [Test]
    public async Task Analyze_GenericEventHandlerOnEvent_ReportsNoDiagnostic()
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public event EventHandler<EventArgs> Changed;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new GenericDelegateAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS020")).IsFalse();
    }

    [Test]
    public async Task Analyze_LambdaPassedToExternalLinqMethod_ReportsNoDiagnostic()
    {
        const string source = """
                              using System.Collections.Generic;
                              using System.Linq;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(List<int> list)
                                      {
                                          var result = list.Where(x => x > 0);
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new GenericDelegateAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS020")).IsFalse();
    }

    [Test]
    public async Task Analyze_MultipleViolationsInSameFile_ReportsMultipleDiagnostics()
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      private Func<int> _provider;
                                      public void Bar(Action<string> callback)
                                      {
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new GenericDelegateAnalyzer(), source);

        await Assert.That(diagnostics.Count(d => d.Id == "ATXCS020")).IsEqualTo(2);
    }

    [Test]
    public async Task Analyze_PredicateParameter_ReportsDiagnostic()
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public void Filter(Predicate<int> predicate)
                                      {
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new GenericDelegateAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS020")).IsTrue();
    }

    [Test]
    public async Task Analyze_QualifiedActionName_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(System.Action<int> callback)
                                      {
                                          callback(1);
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new GenericDelegateAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS020")).IsTrue();
    }

    [Test]
    public async Task Analyze_UsingAliasDeclaration_ReportsDiagnostic()
    {
        const string source = """
                              using MyAction = System.Action<int>;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(MyAction callback)
                                      {
                                          callback(1);
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new GenericDelegateAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS020")).IsTrue();
    }
}
