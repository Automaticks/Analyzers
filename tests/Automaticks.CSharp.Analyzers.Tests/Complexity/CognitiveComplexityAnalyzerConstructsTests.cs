using Automaticks.CSharp;
using Automaticks.CSharp.Complexity;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Complexity;

/// <summary>
///     Tests that CognitiveComplexityAnalyzer walks every construct it scores.
/// </summary>
public class CognitiveComplexityAnalyzerConstructsTests
{
    /// <summary>
    ///     Tests that Analyze_AllScoredConstructsBelowLimit_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AllScoredConstructsBelowLimit_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Foo {
                                      public string Method(IEnumerable<KeyValuePair<int, int>> pairs, int value) {
                                          for (var index = 0; index < 3; index++) { value += index; }
                                          foreach (var (key, item) in pairs) { value += key + item; }
                                          Func<int, int, int> add = (left, right) => left + right;
                                          Func<int, int> legacy = delegate(int x) { return x + 1; };
                                          var mapped = value switch { 0 => "zero", _ => "other" };
                                          return mapped + add(1, 2) + legacy(3);
                                      }
                                  }
                              }
                              """;

        var analyzer = new CognitiveComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.CognitiveComplexity)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_NestedForLoopsAndSwitchExpression_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NestedForLoopsAndSwitchExpression_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Foo {
                                      public int Method(IEnumerable<KeyValuePair<int, int>> pairs, int value) {
                                          for (var a = 0; a < 3; a++) {
                                              for (var b = 0; b < 3; b++) {
                                                  for (var c = 0; c < 3; c++) {
                                                      if (a > b) {
                                                          if (b > c) {
                                                              value += a switch { 0 => 1, 1 => 2, _ => 3 };
                                                          }
                                                      }
                                                  }
                                              }
                                          }
                                          foreach (var (key, item) in pairs) {
                                              if (key > item) {
                                                  if (item > 0) {
                                                      value += key;
                                                  }
                                              }
                                          }
                                          return value;
                                      }
                                  }
                              }
                              """;

        var analyzer = new CognitiveComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.CognitiveComplexity)).IsTrue();
    }
}
