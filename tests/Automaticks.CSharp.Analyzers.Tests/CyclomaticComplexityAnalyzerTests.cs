using Automaticks.CSharp;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

/// <summary>
///     Tests for CyclomaticComplexityAnalyzer.
/// </summary>
public partial class CyclomaticComplexityAnalyzerTests
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
                                      public abstract void Method(int a);
                                  }
                              }
                              """;

        var analyzer = new CyclomaticComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS028")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_CatchClause_IncrementsComplexity.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CatchClause_IncrementsComplexity(CancellationToken cancellationToken)
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

        var analyzer = new CyclomaticComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS028")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_DiagnosticMessage_ContainsMethodNameAndComplexity.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_DiagnosticMessage_ContainsMethodNameAndComplexity(CancellationToken cancellationToken)
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

        var analyzer = new CyclomaticComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);
        var message = DiagnosticCollectionAssertions.GetSingleById(diagnostics, "ATXCS028")
                                 .GetMessage(CultureInfo.InvariantCulture);

        await Assert.That(message).IsEqualTo(
            "Method 'ComplexMethod' has a cyclomatic complexity of 16, which exceeds the maximum of 15");
    }

    /// <summary>
    ///     Tests that Analyze_DoWhileLoop_IncrementsComplexity.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_DoWhileLoop_IncrementsComplexity(CancellationToken cancellationToken)
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

        var analyzer = new CyclomaticComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS028")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ElseIfChain_IncrementsComplexityPerBranch.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ElseIfChain_IncrementsComplexityPerBranch(CancellationToken cancellationToken)
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

        var analyzer = new CyclomaticComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS028")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ExpressionBodiedMethodAboveThreshold_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExpressionBodiedMethodAboveThreshold_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new CyclomaticComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS028")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ForeachLoop_IncrementsComplexity.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ForeachLoop_IncrementsComplexity(CancellationToken cancellationToken)
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

        var analyzer = new CyclomaticComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS028")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ForLoop_IncrementsComplexity.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ForLoop_IncrementsComplexity(CancellationToken cancellationToken)
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

        var analyzer = new CyclomaticComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS028")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_IfStatement_IncrementsComplexity.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_IfStatement_IncrementsComplexity(CancellationToken cancellationToken)
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

        var analyzer = new CyclomaticComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS028")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_LambdaComplexityRollsUpToEnclosingMethod_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LambdaComplexityRollsUpToEnclosingMethod_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new CyclomaticComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS028")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_LocalFunctionComplexityRollsUpToEnclosingMethod_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LocalFunctionComplexityRollsUpToEnclosingMethod_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new CyclomaticComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS028")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_LogicalAnd_IncrementsComplexity.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LogicalAnd_IncrementsComplexity(CancellationToken cancellationToken)
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

        var analyzer = new CyclomaticComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS028")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_LogicalOr_IncrementsComplexity.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LogicalOr_IncrementsComplexity(CancellationToken cancellationToken)
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

        var analyzer = new CyclomaticComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS028")).IsTrue();
    }

}
