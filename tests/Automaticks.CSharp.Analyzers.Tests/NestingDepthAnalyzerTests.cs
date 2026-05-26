
using Automaticks.CSharp;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

/// <summary>
///     Tests for NestingDepthAnalyzer.
/// </summary>
public partial class NestingDepthAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_AbstractMethod_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AbstractMethod_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public abstract class Foo {
                                      public abstract void Method();
                                  }
                              }
                              """;

        var analyzer = new NestingDepthAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.NestingDepth)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_AsyncMethod_CorrectlyAnalyzed.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AsyncMethod_CorrectlyAnalyzed(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading;
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

        var analyzer = new NestingDepthAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.NestingDepth)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_DiagnosticMessage_ContainsMethodNameAndDepth.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_DiagnosticMessage_ContainsMethodNameAndDepth(CancellationToken cancellationToken)
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

        var analyzer = new NestingDepthAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);
        var message = DiagnosticCollectionAssertions.GetSingleById(diagnostics, DiagnosticIds.CSharp.NestingDepth)
                                 .GetMessage(CultureInfo.InvariantCulture);

        await Assert.That(message).IsEqualTo(
            "Method 'DeepMethod' has a nesting depth of 6, which exceeds the maximum of 5");
    }

    /// <summary>
    ///     Tests that Analyze_DoWhileLoop_IncrementsDepth.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_DoWhileLoop_IncrementsDepth(CancellationToken cancellationToken)
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

        var analyzer = new NestingDepthAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.NestingDepth)).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ElseBlock_IncrementsDepth.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ElseBlock_IncrementsDepth(CancellationToken cancellationToken)
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

        var analyzer = new NestingDepthAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.NestingDepth)).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ElseIfChain_DoesNotIncrementDepthBeyondIf.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ElseIfChain_DoesNotIncrementDepthBeyondIf(CancellationToken cancellationToken)
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

        var analyzer = new NestingDepthAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.NestingDepth)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_EmptyMethod_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_EmptyMethod_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method() { }
                                  }
                              }
                              """;

        var analyzer = new NestingDepthAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.NestingDepth)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExpressionBodiedMethod_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExpressionBodiedMethod_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Method() => 42;
                                  }
                              }
                              """;

        var analyzer = new NestingDepthAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.NestingDepth)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExpressionBodiedMethodWithConditional_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExpressionBodiedMethodWithConditional_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Method(bool flag) => flag ? 1 : 0;
                                  }
                              }
                              """;

        var analyzer = new NestingDepthAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.NestingDepth)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_FinallyBlock_IncrementsDepth.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FinallyBlock_IncrementsDepth(CancellationToken cancellationToken)
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

        var analyzer = new NestingDepthAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.NestingDepth)).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_FiveDeepNesting_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FiveDeepNesting_ReportsNoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new NestingDepthAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.NestingDepth)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ForEachLoop_IncrementsDepth.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ForEachLoop_IncrementsDepth(CancellationToken cancellationToken)
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

        var analyzer = new NestingDepthAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.NestingDepth)).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ForLoop_IncrementsDepth.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ForLoop_IncrementsDepth(CancellationToken cancellationToken)
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

        var analyzer = new NestingDepthAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.NestingDepth)).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_Lambda_IncrementsDepth.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_Lambda_IncrementsDepth(CancellationToken cancellationToken)
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

        var analyzer = new NestingDepthAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.NestingDepth)).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_LocalFunction_IncrementsDepth.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LocalFunction_IncrementsDepth(CancellationToken cancellationToken)
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

        var analyzer = new NestingDepthAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.NestingDepth)).IsTrue();
    }

}
