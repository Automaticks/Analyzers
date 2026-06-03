using Automaticks.CSharp;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

/// <summary>
///     Tests for InlineNewExpressionAnalyzer covering simple-assignment-statement exemptions
///     and the boundary cases that intentionally remain reported.
/// </summary>
public class InlineNewExpressionAnalyzerAssignmentTests
{

    /// <summary>
    ///     Tests that Analyze_ArrayCreationAsCtorFieldAssignment_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ArrayCreationAsCtorFieldAssignment_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Service {
                                      private int[] _arr;
                                      public Service() {
                                          _arr = new int[5];
                                      }
                                  }
                              }
                              """;

        var analyzer = new InlineNewExpressionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS058")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_AssignmentUsedAsMethodArgument_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AssignmentUsedAsMethodArgument_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { }
                                  public class Service {
                                      private Foo _foo;
                                      public void Take(Foo f) { }
                                      public void Run() {
                                          Take(_foo = new Foo());
                                      }
                                  }
                              }
                              """;

        var analyzer = new InlineNewExpressionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS058")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ChainedFieldAssignment_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ChainedFieldAssignment_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { }
                                  public class Service {
                                      private Foo _a;
                                      private Foo _b;
                                      public Service() {
                                          _a = _b = new Foo();
                                      }
                                  }
                              }
                              """;

        var analyzer = new InlineNewExpressionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS058")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ExpressionBodiedConstructorAssignment_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExpressionBodiedConstructorAssignment_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { }
                                  public class Service {
                                      private Foo _foo;
                                      public Service() => _foo = new Foo();
                                  }
                              }
                              """;

        var analyzer = new InlineNewExpressionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS058")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ImplicitObjectCreationAsCtorFieldAssignment_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ImplicitObjectCreationAsCtorFieldAssignment_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { }
                                  public class Service {
                                      private Foo _foo;
                                      public Service() {
                                          _foo = new();
                                      }
                                  }
                              }
                              """;

        var analyzer = new InlineNewExpressionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS058")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_NestedObjectCreationInsideExemptedAssignment_ReportsOnlyInnerDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NestedObjectCreationInsideExemptedAssignment_ReportsOnlyInnerDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Bar { }
                                  public class Foo {
                                      public Foo(Bar b) { }
                                  }
                                  public class Service {
                                      private Foo _foo;
                                      public Service() {
                                          _foo = new Foo(new Bar());
                                      }
                                  }
                              }
                              """;

        var analyzer = new InlineNewExpressionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.CountId(diagnostics, "ATXCS058")).IsEqualTo(1);
    }

    /// <summary>
    ///     Tests that Analyze_NullCoalesceAssignment_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NullCoalesceAssignment_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              #nullable enable
                              namespace MyApp {
                                  public class Foo { }
                                  public class Service {
                                      private Foo? _foo;
                                      public void EnsureFoo() {
                                          _foo ??= new Foo();
                                      }
                                  }
                              }
                              """;

        var analyzer = new InlineNewExpressionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS058")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_NullForgivingObjectCreationAsCtorFieldAssignment_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NullForgivingObjectCreationAsCtorFieldAssignment_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              #nullable enable
                              namespace MyApp {
                                  public class Foo { }
                                  public class Service {
                                      private Foo _foo;
                                      public Service() {
                                          _foo = new Foo()!;
                                      }
                                  }
                              }
                              """;

        var analyzer = new InlineNewExpressionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS058")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ObjectCreationAsCtorFieldAssignment_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ObjectCreationAsCtorFieldAssignment_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { }
                                  public class Service {
                                      private Foo _foo;
                                      public Service() {
                                          _foo = new Foo();
                                      }
                                  }
                              }
                              """;

        var analyzer = new InlineNewExpressionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS058")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ObjectCreationAsCtorPropertyAssignment_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ObjectCreationAsCtorPropertyAssignment_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { }
                                  public class Service {
                                      public Foo Prop { get; set; }
                                      public Service() {
                                          Prop = new Foo();
                                      }
                                  }
                              }
                              """;

        var analyzer = new InlineNewExpressionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS058")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ObjectCreationAsIndexerAssignment_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ObjectCreationAsIndexerAssignment_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Foo { }
                                  public class Service {
                                      private Dictionary<int, Foo> _map;
                                      public Service() {
                                          _map = new Dictionary<int, Foo>();
                                          _map[1] = new Foo();
                                      }
                                  }
                              }
                              """;

        var analyzer = new InlineNewExpressionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS058")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ObjectCreationAsMethodBodyFieldAssignment_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ObjectCreationAsMethodBodyFieldAssignment_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { }
                                  public class Service {
                                      private Foo _foo;
                                      public void Reset() {
                                          _foo = new Foo();
                                      }
                                  }
                              }
                              """;

        var analyzer = new InlineNewExpressionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS058")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ObjectCreationAsThisQualifiedFieldAssignment_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ObjectCreationAsThisQualifiedFieldAssignment_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { }
                                  public class Service {
                                      private Foo _foo;
                                      public Service() {
                                          this._foo = new Foo();
                                      }
                                  }
                              }
                              """;

        var analyzer = new InlineNewExpressionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS058")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ParenthesizedObjectCreationAsCtorFieldAssignment_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ParenthesizedObjectCreationAsCtorFieldAssignment_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { }
                                  public class Service {
                                      private Foo _foo;
                                      public Service() {
                                          _foo = (new Foo());
                                      }
                                  }
                              }
                              """;

        var analyzer = new InlineNewExpressionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS058")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_TupleDeconstructionAssignment_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TupleDeconstructionAssignment_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { }
                                  public class Bar { }
                                  public class Service {
                                      private Foo _foo;
                                      private Bar _bar;
                                      public Service() {
                                          (_foo, _bar) = (new Foo(), new Bar());
                                      }
                                  }
                              }
                              """;

        var analyzer = new InlineNewExpressionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.CountId(diagnostics, "ATXCS058")).IsEqualTo(2);
    }

}
