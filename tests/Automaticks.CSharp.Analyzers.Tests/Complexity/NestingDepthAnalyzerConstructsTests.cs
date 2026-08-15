using Automaticks.CSharp;
using Automaticks.CSharp.Complexity;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Complexity;

/// <summary>
///     Tests that NestingDepthAnalyzer walks every nesting construct it recognises.
/// </summary>
public class NestingDepthAnalyzerConstructsTests
{
    private const string AllConstructsSource = """
                                               using System;
                                               using System.Collections.Generic;
                                               namespace MyApp {
                                                   public class Foo {
                                                       private readonly object _gate = new object();
                                                       public int Method(IEnumerable<KeyValuePair<int, int>> pairs, IDisposable resource, int value) {
                                                           while (value > 0) { value--; }
                                                           lock (_gate) { value++; }
                                                           using (resource) { value++; }
                                                           switch (value) { case 1: value = 2; break; default: value = 3; break; }
                                                           var mapped = value switch { 1 => "a", _ => "b" };
                                                           foreach (var (key, item) in pairs) { value += key + item; }
                                                           Func<int, int> simple = x => x + 1;
                                                           Func<int, int> anon = delegate(int x) { return x + 2; };
                                                           return value + simple(1) + anon(2) + mapped.Length;
                                                       }
                                                   }
                                               }
                                               """;

    /// <summary>
    ///     Tests that Analyze_AllNestingConstructsAtShallowDepth_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AllNestingConstructsAtShallowDepth_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        var analyzer = new NestingDepthAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, AllConstructsSource, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.NestingDepth)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_DeeplyNestedMixedConstructs_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_DeeplyNestedMixedConstructs_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Foo {
                                      private readonly object _gate = new object();
                                      public int Method(IEnumerable<KeyValuePair<int, int>> pairs, IDisposable resource, int value) {
                                          while (value > 0) {
                                              lock (_gate) {
                                                  using (resource) {
                                                      foreach (var (key, item) in pairs) {
                                                          switch (key) {
                                                              case 1:
                                                                  if (item > 0) { value += item; }
                                                                  break;
                                                              default:
                                                                  break;
                                                          }
                                                      }
                                                  }
                                              }
                                              value--;
                                          }
                                          return value;
                                      }
                                  }
                              }
                              """;

        var analyzer = new NestingDepthAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.NestingDepth)).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_NestedSwitchExpressionAndLambdas_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NestedSwitchExpressionAndLambdas_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public string Method(int value) {
                                          Func<int, string> render = x => x switch { 0 => "zero", _ => "other" };
                                          Func<int, string> legacy = delegate(int x) { return x > 0 ? "pos" : "neg"; };
                                          return render(value) + legacy(value);
                                      }
                                  }
                              }
                              """;

        var analyzer = new NestingDepthAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.NestingDepth)).IsFalse();
    }
}
