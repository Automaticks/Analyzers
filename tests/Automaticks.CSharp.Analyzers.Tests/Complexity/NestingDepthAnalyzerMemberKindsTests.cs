using Automaticks.CSharp.Complexity;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Complexity;

/// <summary>
///     Tests that NestingDepthAnalyzer handles every member kind and body form it registers for.
/// </summary>
public class NestingDepthAnalyzerMemberKindsTests
{
    private const string DeepBody = """
                                    if (a > 0) {
                                        if (a > 1) {
                                            if (a > 2) {
                                                if (a > 3) {
                                                    if (a > 4) {
                                                        if (a > 5) {
                                                            a = 42;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    """;

    /// <summary>
    ///     Tests that Analyze_AbstractInterfaceOperators_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AbstractInterfaceOperators_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface IAddable<T> where T : IAddable<T> {
                                      static abstract T operator +(T left, T right);
                                      static abstract explicit operator int(T value);
                                  }
                              }
                              """;

        var analyzer = new NestingDepthAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.NestingDepth)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_AutoPropertyAccessor_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AutoPropertyAccessor_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Value { get; set; }
                                      public int Other { get; init; }
                                  }
                              }
                              """;

        var analyzer = new NestingDepthAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.NestingDepth)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ConversionOperatorWithDeepNesting_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConversionOperatorWithDeepNesting_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        var source = $$"""
                       namespace MyApp {
                           public class Foo {
                               public static explicit operator int(Foo value) {
                                   var a = 0;
                                   {{DeepBody}}
                                   return a;
                               }
                           }
                       }
                       """;

        var analyzer = new NestingDepthAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.NestingDepth)).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ForLoopWithInitializers_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ForLoopWithInitializers_ReportsNoDiagnostic(CancellationToken cancellationToken)    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() {
                                          var index = 0;
                                          var total = 0;
                                          for (index = 0, total = 0; index < 3; index++) { total += index; }
                                      }
                                  }
                              }
                              """;

        var analyzer = new NestingDepthAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.NestingDepth)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_GetAccessorWithDeepNesting_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_GetAccessorWithDeepNesting_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        var source = $$"""
                       namespace MyApp {
                           public class Foo {
                               public int Value {
                                   get {
                                       var a = 0;
                                       {{DeepBody}}
                                       return a;
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
    ///     Tests that Analyze_LocalFunctionWithExpressionBody_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LocalFunctionWithExpressionBody_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Bar() {
                                          int Double(int value) => value * 2;
                                          return Double(21);
                                      }
                                  }
                              }
                              """;

        var analyzer = new NestingDepthAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.NestingDepth)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_OperatorWithDeepNesting_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_OperatorWithDeepNesting_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        var source = $$"""
                       namespace MyApp {
                           public class Foo {
                               public static Foo operator +(Foo left, Foo right) {
                                   var a = 0;
                                   {{DeepBody}}
                                   return left;
                               }
                           }
                       }
                       """;

        var analyzer = new NestingDepthAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.NestingDepth)).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_TryWithCatchClauses_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TryWithCatchClauses_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() {
                                          try { Console.WriteLine("a"); }
                                          catch (InvalidOperationException) { Console.WriteLine("b"); }
                                          catch (Exception) { Console.WriteLine("c"); }
                                      }
                                  }
                              }
                              """;

        var analyzer = new NestingDepthAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.NestingDepth)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_UsingStatementWithDeclaration_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_UsingStatementWithDeclaration_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              using System.IO;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() {
                                          using (var stream = new MemoryStream()) { Console.WriteLine(stream.Length); }
                                      }
                                  }
                              }
                              """;

        var analyzer = new NestingDepthAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.NestingDepth)).IsFalse();
    }
}
