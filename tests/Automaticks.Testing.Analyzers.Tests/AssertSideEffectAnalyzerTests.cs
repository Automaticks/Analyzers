using Automaticks.Testing.Testability;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Testing.Analyzers.Tests;

/// <summary>
///     Tests for AssertSideEffectAnalyzer.
/// </summary>
public class AssertSideEffectAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_AssertWithAssignment_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AssertWithAssignment_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Diagnostics;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() {
                                          var count = 0;
                                          Debug.Assert((count = Compute()) > 0);
                                      }
                                      private int Compute() { return 1; }
                                  }
                              }
                              """;

        var analyzer = new AssertSideEffectAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(
            diagnostics,
            "ATXTST009",
            "an assignment")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_AssertWithIncrement_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AssertWithIncrement_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Diagnostics;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() {
                                          var count = 0;
                                          Debug.Assert(++count > 0);
                                      }
                                  }
                              }
                              """;

        var analyzer = new AssertSideEffectAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(
            diagnostics,
            "ATXTST009",
            "an increment or decrement")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_AssertWithNonMutatingPostfixOperator_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AssertWithNonMutatingPostfixOperator_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Diagnostics;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(int? value) {
                                          Debug.Assert(value!.Value > 0);
                                      }
                                  }
                              }
                              """;

        var analyzer = new AssertSideEffectAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST009")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_AssertWithNonMutatingPrefixOperator_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AssertWithNonMutatingPrefixOperator_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Diagnostics;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(bool flag) {
                                          Debug.Assert(!flag);
                                      }
                                  }
                              }
                              """;

        var analyzer = new AssertSideEffectAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST009")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_AssertWithOutArgument_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AssertWithOutArgument_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections.Generic;
                              using System.Diagnostics;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(Dictionary<string, int> map) {
                                          Debug.Assert(map.TryGetValue("k", out var value));
                                      }
                                  }
                              }
                              """;

        var analyzer = new AssertSideEffectAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(
            diagnostics,
            "ATXTST009",
            "an out or ref argument")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_AssertWithPostfixDecrement_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AssertWithPostfixDecrement_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Diagnostics;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() {
                                          var count = 5;
                                          Debug.Assert(count-- > 0);
                                      }
                                  }
                              }
                              """;

        var analyzer = new AssertSideEffectAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST009")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_AssertWithPureCondition_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AssertWithPureCondition_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Diagnostics;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(string text) {
                                          Debug.Assert(text != null && text.Length > 0);
                                      }
                                  }
                              }
                              """;

        var analyzer = new AssertSideEffectAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST009")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_UnrelatedAssertMethodWithAssignment_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_UnrelatedAssertMethodWithAssignment_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public static class Check {
                                      public static void Assert(bool condition) { }
                                  }
                                  public class Foo {
                                      public void Bar() {
                                          var count = 0;
                                          Check.Assert((count = 1) > 0);
                                      }
                                  }
                              }
                              """;

        var analyzer = new AssertSideEffectAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST009")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_UnrelatedInvocationWithoutArguments_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_UnrelatedInvocationWithoutArguments_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() { Reset(); }
                                      private void Reset() { }
                                  }
                              }
                              """;

        var analyzer = new AssertSideEffectAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST009")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_UnresolvedInvocationWithArgument_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_UnresolvedInvocationWithArgument_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() { Undefined(true); }
                                  }
                              }
                              """;

        var analyzer = new AssertSideEffectAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST009")).IsFalse();
    }
}
