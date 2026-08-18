using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Complexity;

/// <summary>
///     Tests that ExcessiveParameterCountAnalyzer covers its guard, override, and interface branches.
/// </summary>
public class ExcessiveParameterCountAnalyzerBranchesTests
{
    /// <summary>
    ///     Tests that Analyze_AnonymousMethodWithFourParams_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AnonymousMethodWithFourParams_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public void DoWork() {
                                          Action<int, int, int, int> action = delegate(int a, int b, int c, int d) {};
                                          action(1, 2, 3, 4);
                                      }
                                  }
                              }
                              """;

        var analyzer = new ExcessiveParameterCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS022")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_AnonymousMethodWithNullParameterList_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AnonymousMethodWithNullParameterList_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public void DoWork() {
                                          Action action = delegate {};
                                          action();
                                      }
                                  }
                              }
                              """;

        var analyzer = new ExcessiveParameterCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS022")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExternalIndexerOverrideWithFiveParams_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExternalIndexerOverrideWithFiveParams_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string externalSource = """
                                      namespace External {
                                          public abstract class Base {
                                              public abstract int this[int a, int b, int c, int d, int e] { get; }
                                          }
                                      }
                                      """;
        const string source = """
                              using External;
                              namespace MyApp {
                                  public class Derived : Base {
                                      public override int this[int a, int b, int c, int d, int e] => a;
                                  }
                              }
                              """;

        var externalRef = AnalyzerTestRunner.CompileToReference(externalSource);
        var analyzer = new ExcessiveParameterCountAnalyzer();
        var options = new AnalysisOptions
        {
            AdditionalReferences = [externalRef]
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS022")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ImplicitExternalInterfaceIndexerImplementation_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ImplicitExternalInterfaceIndexerImplementation_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections;
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Foo : IReadOnlyList<int> {
                                      private readonly List<int> _items = new List<int>();
                                      public int Count => _items.Count;
                                      public int this[int index] => _items[index];
                                      public IEnumerator<int> GetEnumerator() => _items.GetEnumerator();
                                      IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
                                  }
                              }
                              """;

        var analyzer = new ExcessiveParameterCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS022")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_LambdaWithFourParams_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LambdaWithFourParams_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public void DoWork() {
                                          Action<int, int, int, int> action = (a, b, c, d) => {};
                                          action(1, 2, 3, 4);
                                      }
                                  }
                              }
                              """;

        var analyzer = new ExcessiveParameterCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS022")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_LocalFunctionWithFourParams_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LocalFunctionWithFourParams_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void DoWork() {
                                          void Inner(int a, int b, int c, int d) {}
                                          Inner(1, 2, 3, 4);
                                      }
                                  }
                              }
                              """;

        var analyzer = new ExcessiveParameterCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS022")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_LocalIndexerOverrideWithFiveParams_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LocalIndexerOverrideWithFiveParams_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public abstract class Base {
                                      public abstract int this[int a, int b, int c, int d, int e] { get; }
                                  }

                                  public class Derived : Base {
                                      public override int this[int a, int b, int c, int d, int e] => a;
                                  }
                              }
                              """;

        var analyzer = new ExcessiveParameterCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS022")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_LocalMethodOverrideWithFiveParams_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LocalMethodOverrideWithFiveParams_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public abstract class Base {
                                      public abstract void DoWork(int a, int b, int c, int d, int e);
                                  }

                                  public class Derived : Base {
                                      public override void DoWork(int a, int b, int c, int d, int e) {}
                                  }
                              }
                              """;

        var analyzer = new ExcessiveParameterCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS022")).IsTrue();
    }
}
