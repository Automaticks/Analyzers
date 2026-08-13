using Automaticks.CSharp;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Formatting;

/// <summary>
///     Tests for InlineNewExpressionAnalyzer.
/// </summary>
public partial class InlineNewExpressionAnalyzerTests
{

    /// <summary>
    ///     Tests that Analyze_AnonymousObjectCreationAsMethodArgument_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AnonymousObjectCreationAsMethodArgument_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new InlineNewExpressionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS058")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ArrayCreationAsMethodArgument_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ArrayCreationAsMethodArgument_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new InlineNewExpressionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS058")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ArrayCreationInLocalVar_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ArrayCreationInLocalVar_ReportsNoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new InlineNewExpressionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS058")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExplicitTypeLocalVarDeclaration_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExplicitTypeLocalVarDeclaration_ReportsNoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new InlineNewExpressionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS058")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ImplicitArrayCreationAsMethodArgument_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ImplicitArrayCreationAsMethodArgument_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new InlineNewExpressionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS058")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ImplicitObjectCreationAsMethodArgument_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ImplicitObjectCreationAsMethodArgument_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new InlineNewExpressionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS058")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ImplicitObjectCreationInLocalVarDeclaration_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ImplicitObjectCreationInLocalVarDeclaration_ReportsNoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new InlineNewExpressionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS058")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_LocalVarInsideLambda_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LocalVarInsideLambda_ReportsNoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new InlineNewExpressionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS058")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ObjectCreationAsCollectionInitializerElement_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ObjectCreationAsCollectionInitializerElement_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new InlineNewExpressionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS058")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ObjectCreationAsConstructorArgument_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ObjectCreationAsConstructorArgument_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new InlineNewExpressionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS058")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ObjectCreationAsMethodArgument_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ObjectCreationAsMethodArgument_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new InlineNewExpressionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS058")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ObjectCreationAsObjectInitializerValue_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ObjectCreationAsObjectInitializerValue_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new InlineNewExpressionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS058")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ObjectCreationInConditionalExpression_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ObjectCreationInConditionalExpression_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new InlineNewExpressionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.CountId(diagnostics, "ATXCS058")).IsEqualTo(2);
    }

    /// <summary>
    ///     Tests that Analyze_ObjectCreationInExpressionBodiedMethod_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ObjectCreationInExpressionBodiedMethod_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { }
                                  public class Service {
                                      public Foo Create() => new Foo();
                                  }
                              }
                              """;

        var analyzer = new InlineNewExpressionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS058")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ObjectCreationInFieldInitializer_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ObjectCreationInFieldInitializer_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Health { }
                                  public class Player {
                                      private Health _health = new Health();
                                  }
                              }
                              """;

        var analyzer = new InlineNewExpressionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS058")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ObjectCreationInForLoopVariable_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ObjectCreationInForLoopVariable_ReportsNoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new InlineNewExpressionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS058")).IsFalse();
    }

}
