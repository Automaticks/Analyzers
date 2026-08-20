using Automaticks.CSharp.Complexity;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Complexity;

/// <summary>
///     Tests that MethodLineLimitAnalyzer handles every member kind and skip condition.
/// </summary>
public class MethodLineLimitAnalyzerMemberKindsTests
{
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

        var analyzer = new MethodLineLimitAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.MethodLineLimit)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ConversionOperatorOverLimit_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConversionOperatorOverLimit_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        var source = $$"""
                       namespace MyApp {
                           public class Foo {
                               public static explicit operator int(Foo value) {
                                   var a = 0;
                       {{BuildLongBody()}}
                                   return a;
                               }
                           }
                       }
                       """;

        var analyzer = new MethodLineLimitAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.MethodLineLimit)).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ExpressionBodiedMembers_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExpressionBodiedMembers_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Value => 1;
                                      public int Compute() => 2;
                                      public static Foo operator +(Foo left, Foo right) => left;
                                      public static explicit operator int(Foo value) => 3;
                                  }
                              }
                              """;

        var analyzer = new MethodLineLimitAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.MethodLineLimit)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_GetAccessorOverLimit_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_GetAccessorOverLimit_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        var source = $$"""
                       namespace MyApp {
                           public class Foo {
                               public int Value {
                                   get {
                                       var a = 0;
                       {{BuildLongBody()}}
                                       return a;
                                   }
                               }
                           }
                       }
                       """;

        var analyzer = new MethodLineLimitAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.MethodLineLimit)).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_LocalFunctionOverLimit_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LocalFunctionOverLimit_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        var source = $$"""
                       namespace MyApp {
                           public class Foo {
                               public int Bar() {
                                   int Inner() {
                                       var a = 0;
                       {{BuildLongBody()}}
                                       return a;
                                   }
                                   return Inner();
                               }
                           }
                       }
                       """;

        var analyzer = new MethodLineLimitAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.MethodLineLimit)).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_LongMethodReturningObjectInitializer_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LongMethodReturningObjectInitializer_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        var builder = new StringBuilder();
        for (var index = 0; index < 60; index++)
        {
            builder.AppendLine($"                Name{index} = {index},");
        }

        var source = $$"""
                       namespace MyApp {
                           public class Data {
                       {{BuildFields()}}
                           }
                           public class Foo {
                               public Data Build() {
                                   return new Data {
                       {{builder.ToString().TrimEnd()}}
                                   };
                               }
                           }
                       }
                       """;

        var analyzer = new MethodLineLimitAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.MethodLineLimit)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_LongMethodWithSingleSwitch_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LongMethodWithSingleSwitch_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        var builder = new StringBuilder();
        for (var index = 0; index < 60; index++)
        {
            builder.AppendLine($"                case {index}: return {index};");
        }

        var source = $$"""
                       namespace MyApp {
                           public class Foo {
                               public int Bar(int value) {
                                   switch (value) {
                       {{builder.ToString().TrimEnd()}}
                                       default: return -1;
                                   }
                               }
                           }
                       }
                       """;

        var analyzer = new MethodLineLimitAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.MethodLineLimit)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_OperatorOverLimit_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_OperatorOverLimit_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        var source = $$"""
                       namespace MyApp {
                           public class Foo {
                               public static Foo operator +(Foo left, Foo right) {
                                   var a = 0;
                       {{BuildLongBody()}}
                                   return left;
                               }
                           }
                       }
                       """;

        var analyzer = new MethodLineLimitAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.MethodLineLimit)).IsTrue();
    }

    private string BuildFields()
    {
        var builder = new StringBuilder();
        for (var index = 0; index < 60; index++)
        {
            builder.AppendLine($"        public int Name{index} {{ get; set; }}");
        }

        return builder.ToString().TrimEnd();
    }

    private string BuildLongBody()
    {
        var builder = new StringBuilder();
        for (var index = 0; index < 60; index++)
        {
            builder.AppendLine($"            a = a + {index};");
        }

        return builder.ToString().TrimEnd();
    }
}
