using Automaticks.CSharp;
using Automaticks.CSharp.Complexity;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Complexity;

/// <summary>
///     Tests that ConfigurableLimit.Read honors .editorconfig overrides and falls back to defaults.
/// </summary>
public class ConfigurableLimitTests
{
    private const string HighComplexitySource = """
                                                 using System;
                                                 namespace MyApp {
                                                     public class Foo {
                                                         public void Method(int x) {
                                                             if (x == 1) { }
                                                             if (x == 2) { }
                                                             if (x == 3) { }
                                                             if (x == 4) { }
                                                             if (x == 5) { }
                                                             if (x == 6) { }
                                                             if (x == 7) { }
                                                             if (x == 8) { }
                                                             if (x == 9) { }
                                                             if (x == 10) { }
                                                             if (x == 11) { }
                                                             if (x == 12) { }
                                                             if (x == 13) { }
                                                             if (x == 14) { }
                                                             if (x == 15) { }
                                                             if (x == 16) { }
                                                         }
                                                     }
                                                 }
                                                 """;
    private const string LongMethodBodySource = """
                                                 using System;
                                                 namespace MyApp {
                                                     public class Foo {
                                                         public void Method() {
                                                             var x1 = 1;
                                                             var x2 = 2;
                                                             var x3 = 3;
                                                             var x4 = 4;
                                                             var x5 = 5;
                                                             var x6 = 6;
                                                             var x7 = 7;
                                                             var x8 = 8;
                                                             var x9 = 9;
                                                             var x10 = 10;
                                                             var x11 = 11;
                                                             var x12 = 12;
                                                             var x13 = 13;
                                                             var x14 = 14;
                                                             var x15 = 15;
                                                             var x16 = 16;
                                                             var x17 = 17;
                                                             var x18 = 18;
                                                             var x19 = 19;
                                                             var x20 = 20;
                                                             var x21 = 21;
                                                             var x22 = 22;
                                                             var x23 = 23;
                                                             var x24 = 24;
                                                             var x25 = 25;
                                                             var x26 = 26;
                                                             var x27 = 27;
                                                             var x28 = 28;
                                                             var x29 = 29;
                                                             var x30 = 30;
                                                             var x31 = 31;
                                                             var x32 = 32;
                                                             var x33 = 33;
                                                             var x34 = 34;
                                                             var x35 = 35;
                                                             var x36 = 36;
                                                             var x37 = 37;
                                                             var x38 = 38;
                                                             var x39 = 39;
                                                             var x40 = 40;
                                                             var x41 = 41;
                                                             var x42 = 42;
                                                             var x43 = 43;
                                                             var x44 = 44;
                                                             var x45 = 45;
                                                             var x46 = 46;
                                                             var x47 = 47;
                                                             var x48 = 48;
                                                             var x49 = 49;
                                                             var x50 = 50;
                                                             var x51 = 51;
                                                             var x52 = 52;
                                                             var x53 = 53;
                                                             var x54 = 54;
                                                             var x55 = 55;
                                                             var x56 = 56;
                                                             var x57 = 57;
                                                             var x58 = 58;
                                                             var x59 = 59;
                                                             var x60 = 60;
                                                         }
                                                     }
                                                 }
                                                 """;
    private const string LowComplexitySource = """
                                                using System;
                                                namespace MyApp {
                                                    public class Foo {
                                                        public void Method(int x) {
                                                            if (x == 1) { }
                                                            if (x == 2) { }
                                                            if (x == 3) { }
                                                            if (x == 4) { }
                                                        }
                                                    }
                                                }
                                                """;

    /// <summary>
    ///     Tests that Read_CognitiveComplexityKeyLowered_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Read_CognitiveComplexityKeyLowered_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        var analyzer = new CognitiveComplexityAnalyzer();
        var configOptions = new Dictionary<string, string>
        {
            ["automaticks.cognitive_complexity"] = "3"
        };
        var options = new AnalysisOptions
        {
            ConfigOptions = configOptions
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, LowComplexitySource, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.CognitiveComplexity)).IsTrue();
    }

    /// <summary>
    ///     Tests that Read_CognitiveComplexityKeyRaised_SuppressesDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Read_CognitiveComplexityKeyRaised_SuppressesDiagnostic(CancellationToken cancellationToken)
    {
        var analyzer = new CognitiveComplexityAnalyzer();
        var configOptions = new Dictionary<string, string>
        {
            ["automaticks.cognitive_complexity"] = "20"
        };
        var options = new AnalysisOptions
        {
            ConfigOptions = configOptions
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, HighComplexitySource, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.CognitiveComplexity)).IsFalse();
    }

    /// <summary>
    ///     Tests that Read_CyclomaticComplexityKeyLowered_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Read_CyclomaticComplexityKeyLowered_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        var analyzer = new CyclomaticComplexityAnalyzer();
        var configOptions = new Dictionary<string, string>
        {
            ["automaticks.cyclomatic_complexity"] = "3"
        };
        var options = new AnalysisOptions
        {
            ConfigOptions = configOptions
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, LowComplexitySource, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.CyclomaticComplexity)).IsTrue();
    }

    /// <summary>
    ///     Tests that Read_CyclomaticComplexityKeyRaised_SuppressesDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Read_CyclomaticComplexityKeyRaised_SuppressesDiagnostic(CancellationToken cancellationToken)
    {
        var analyzer = new CyclomaticComplexityAnalyzer();
        var configOptions = new Dictionary<string, string>
        {
            ["automaticks.cyclomatic_complexity"] = "30"
        };
        var options = new AnalysisOptions
        {
            ConfigOptions = configOptions
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, HighComplexitySource, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.CyclomaticComplexity)).IsFalse();
    }

    /// <summary>
    ///     Tests that Read_KeyAbsent_FallsBackToDefaultLimit.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Read_KeyAbsent_FallsBackToDefaultLimit(CancellationToken cancellationToken)
    {
        var analyzer = new CyclomaticComplexityAnalyzer();
        var configOptions = new Dictionary<string, string>
        {
            ["unrelated.editorconfig.key"] = "3"
        };
        var options = new AnalysisOptions
        {
            ConfigOptions = configOptions
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, HighComplexitySource, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(
            diagnostics, DiagnosticIds.CSharp.CyclomaticComplexity, "maximum of 15")).IsTrue();
    }

    /// <summary>
    ///     Tests that Read_KeyNonNumeric_FallsBackToDefaultLimit.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Read_KeyNonNumeric_FallsBackToDefaultLimit(CancellationToken cancellationToken)
    {
        var analyzer = new CyclomaticComplexityAnalyzer();
        var configOptions = new Dictionary<string, string>
        {
            ["automaticks.cyclomatic_complexity"] = "abc"
        };
        var options = new AnalysisOptions
        {
            ConfigOptions = configOptions
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, HighComplexitySource, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(
            diagnostics, DiagnosticIds.CSharp.CyclomaticComplexity, "maximum of 15")).IsTrue();
    }

    /// <summary>
    ///     Tests that Read_KeyZeroOrNegative_FallsBackToDefaultLimit.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Read_KeyZeroOrNegative_FallsBackToDefaultLimit(CancellationToken cancellationToken)
    {
        var analyzer = new CyclomaticComplexityAnalyzer();
        var configOptions = new Dictionary<string, string>
        {
            ["automaticks.cyclomatic_complexity"] = "0"
        };
        var options = new AnalysisOptions
        {
            ConfigOptions = configOptions
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, HighComplexitySource, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(
            diagnostics, DiagnosticIds.CSharp.CyclomaticComplexity, "maximum of 15")).IsTrue();
    }

    /// <summary>
    ///     Tests that Read_MethodLineLimitKeyLowered_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Read_MethodLineLimitKeyLowered_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public void Method() {
                                          var a = 1;
                                      }
                                  }
                              }
                              """;

        var analyzer = new MethodLineLimitAnalyzer();
        var configOptions = new Dictionary<string, string>
        {
            ["automaticks.method_line_limit"] = "2"
        };
        var options = new AnalysisOptions
        {
            ConfigOptions = configOptions
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.MethodLineLimit)).IsTrue();
    }

    /// <summary>
    ///     Tests that Read_MethodLineLimitKeyRaised_SuppressesDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Read_MethodLineLimitKeyRaised_SuppressesDiagnostic(CancellationToken cancellationToken)
    {
        var analyzer = new MethodLineLimitAnalyzer();
        var configOptions = new Dictionary<string, string>
        {
            ["automaticks.method_line_limit"] = "100"
        };
        var options = new AnalysisOptions
        {
            ConfigOptions = configOptions
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, LongMethodBodySource, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.MethodLineLimit)).IsFalse();
    }

    /// <summary>
    ///     Tests that Read_NestingDepthKeyLowered_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Read_NestingDepthKeyLowered_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public void Method(int x) {
                                          if (x > 0) {
                                              if (x > 1) {
                                                  if (x > 2) { }
                                              }
                                          }
                                      }
                                  }
                              }
                              """;

        var analyzer = new NestingDepthAnalyzer();
        var configOptions = new Dictionary<string, string>
        {
            ["automaticks.nesting_depth"] = "2"
        };
        var options = new AnalysisOptions
        {
            ConfigOptions = configOptions
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.NestingDepth)).IsTrue();
    }

    /// <summary>
    ///     Tests that Read_NestingDepthKeyRaised_SuppressesDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Read_NestingDepthKeyRaised_SuppressesDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public void Method(int x) {
                                          if (x > 0) {
                                              if (x > 1) {
                                                  if (x > 2) {
                                                      if (x > 3) {
                                                          if (x > 4) {
                                                              if (x > 5) { }
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
        var configOptions = new Dictionary<string, string>
        {
            ["automaticks.nesting_depth"] = "10"
        };
        var options = new AnalysisOptions
        {
            ConfigOptions = configOptions
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.NestingDepth)).IsFalse();
    }
}
