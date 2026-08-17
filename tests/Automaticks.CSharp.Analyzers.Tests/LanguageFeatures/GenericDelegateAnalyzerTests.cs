using Automaticks.CSharp;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures;

/// <summary>
///     Tests for GenericDelegateAnalyzer.
/// </summary>
public class GenericDelegateAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_ActionAsGenericTypeArgument_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ActionAsGenericTypeArgument_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new GenericDelegateAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS020")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ActionParameter_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ActionParameter_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new GenericDelegateAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS020")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ActionReturnType_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ActionReturnType_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new GenericDelegateAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS020")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_AliasedFuncType_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AliasedFuncType_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using Mapper = System.Func<int, int>;
                              namespace MyApp {
                                  public class Foo {
                                      public Mapper? Bar() { return null; }
                                  }
                              }
                              """;

        var analyzer = new GenericDelegateAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS020")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_AliasUsageWhereAliasWrapsAction_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AliasUsageWhereAliasWrapsAction_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using MyFunc = System.Func<int, bool>;
                              namespace MyApp {
                                  public class Foo {
                                      private MyFunc _filter;
                                  }
                              }
                              """;

        var analyzer = new GenericDelegateAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS020")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ComparisonParameter_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ComparisonParameter_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new GenericDelegateAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS020")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ConverterLocalVariable_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConverterLocalVariable_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new GenericDelegateAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS020")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_CustomDelegateField_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CustomDelegateField_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public delegate void MyCallback(int x);
                                  public class Foo {
                                      private MyCallback _callback;
                                  }
                              }
                              """;

        var analyzer = new GenericDelegateAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS020")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_CustomDelegateParameter_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CustomDelegateParameter_ReportsNoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new GenericDelegateAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS020")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_EventHandlerOnEvent_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_EventHandlerOnEvent_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public event EventHandler Closed;
                                  }
                              }
                              """;

        var analyzer = new GenericDelegateAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS020")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_FuncField_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FuncField_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      private Func<int> _provider;
                                  }
                              }
                              """;

        var analyzer = new GenericDelegateAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS020")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_FuncLocalVariable_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FuncLocalVariable_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new GenericDelegateAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS020")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_FuncProperty_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FuncProperty_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public Func<int, bool> Filter { get; set; }
                                  }
                              }
                              """;

        var analyzer = new GenericDelegateAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS020")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_GenericActionParameter_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_GenericActionParameter_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new GenericDelegateAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS020")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_GenericEventHandlerOnEvent_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_GenericEventHandlerOnEvent_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public event EventHandler<EventArgs> Changed;
                                  }
                              }
                              """;

        var analyzer = new GenericDelegateAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS020")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_LambdaPassedToExternalLinqMethod_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LambdaPassedToExternalLinqMethod_ReportsNoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new GenericDelegateAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS020")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_MultipleViolationsInSameFile_ReportsMultipleDiagnostics.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MultipleViolationsInSameFile_ReportsMultipleDiagnostics(CancellationToken cancellationToken)
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

        var analyzer = new GenericDelegateAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.CountId(diagnostics, "ATXCS020")).IsEqualTo(2);
    }

    /// <summary>
    ///     Tests that Analyze_PredicateParameter_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PredicateParameter_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new GenericDelegateAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS020")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_QualifiedActionName_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_QualifiedActionName_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new GenericDelegateAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS020")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_UsingAliasDeclaration_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_UsingAliasDeclaration_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new GenericDelegateAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS020")).IsTrue();
    }
}
