using Automaticks.CSharp;
using Automaticks.CSharp.Complexity;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Complexity;

/// <summary>
///     Tests that CyclomaticComplexityAnalyzer counts every decision construct it recognises.
/// </summary>
public class CyclomaticComplexityAnalyzerConstructsTests
{
    /// <summary>
    ///     Tests that Analyze_AllDecisionConstructsBelowLimit_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AllDecisionConstructsBelowLimit_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Foo {
                                      public string Method(IEnumerable<KeyValuePair<int, int>> pairs, int value, string? text) {
                                          while (value > 0) { value--; }
                                          foreach (var (key, item) in pairs) { value += key + item; }
                                          text ??= "fallback";
                                          switch (value) { case 1: return "one"; default: break; }
                                          return value switch { 2 => "two", _ => text };
                                      }
                                  }
                              }
                              """;

        var analyzer = new CyclomaticComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.CyclomaticComplexity)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_CatchWhenClause_CountsGuard.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CatchWhenClause_CountsGuard(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public int Method(int value) {
                                          try { return value; }
                                          catch (Exception ex) when (ex.Message.Length > 0) { return -1; }
                                      }
                                  }
                              }
                              """;

        var analyzer = new CyclomaticComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.CyclomaticComplexity)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_CoalesceExpression_IncrementsComplexity.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CoalesceExpression_IncrementsComplexity(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public string Method(string? a) {
                                          return a ?? a ?? a ?? a ?? a ?? a ?? a ?? a ?? a ?? a ?? a ?? a ?? a ?? a ?? a ?? a ?? "fallback";
                                      }
                                  }
                              }
                              """;

        var analyzer = new CyclomaticComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.CyclomaticComplexity)).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ManyDecisionConstructs_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ManyDecisionConstructs_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Foo {
                                      public string Method(IEnumerable<KeyValuePair<int, int>> pairs, int value, string? text) {
                                          while (value > 20) { value--; }
                                          while (value > 19) { value--; }
                                          while (value > 18) { value--; }
                                          foreach (var (key, item) in pairs) { value += key + item; }
                                          text ??= "fallback";
                                          switch (value) {
                                              case 1: return "one";
                                              case 2: return "two";
                                              case 3 when value > 2: return "three";
                                              case 4: return "four";
                                              default: break;
                                          }
                                          return value switch {
                                              5 => "five",
                                              6 => "six",
                                              7 => "seven",
                                              8 when value > 7 => "eight",
                                              9 => "nine",
                                              _ => text
                                          };
                                      }
                                  }
                              }
                              """;

        var analyzer = new CyclomaticComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.CyclomaticComplexity)).IsTrue();
    }
}
