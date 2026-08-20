using Automaticks.Testing.Testability;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Testing.Analyzers.Tests.Testability;

/// <summary>
///     Tests for ExcessiveDecisionConditionsAnalyzer.
/// </summary>
public class ExcessiveDecisionConditionsAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_ConfiguredThresholdLowered_FlagsConditionBelowDefaultThreshold.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConfiguredThresholdLowered_FlagsConditionBelowDefaultThreshold(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Bar(bool a, bool b, bool c) { if (a && b && c) { return 1; } return 0; }
                                  }
                              }
                              """;

        var analyzer = new ExcessiveDecisionConditionsAnalyzer();
        var configOptions = new Dictionary<string, string>
        {
            ["automaticks.max_decision_conditions"] = "2",
        };
        var options = new AnalysisOptions
        {
            ConfigOptions = configOptions,
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST005")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ConfiguredThresholdRaised_SuppressesConditionAboveDefaultThreshold.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConfiguredThresholdRaised_SuppressesConditionAboveDefaultThreshold(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Bar(bool a, bool b, bool c, bool d) { if (a && b && c && d) { return 1; } return 0; }
                                  }
                              }
                              """;

        var analyzer = new ExcessiveDecisionConditionsAnalyzer();
        var configOptions = new Dictionary<string, string>
        {
            ["automaticks.max_decision_conditions"] = "5",
        };
        var options = new AnalysisOptions
        {
            ConfigOptions = configOptions,
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST005")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_DoWhileConditionAboveThreshold_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_DoWhileConditionAboveThreshold_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(bool a, bool b, bool c, bool d) { do { } while (a && b && c && d); }
                                  }
                              }
                              """;

        var analyzer = new ExcessiveDecisionConditionsAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST005")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_MalformedThresholdConfiguration_FallsBackToDefault.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MalformedThresholdConfiguration_FallsBackToDefault(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Bar(bool a, bool b, bool c, bool d) { if (a && b && c && d) { return 1; } return 0; }
                                  }
                              }
                              """;

        var analyzer = new ExcessiveDecisionConditionsAnalyzer();
        var configOptions = new Dictionary<string, string>
        {
            ["automaticks.max_decision_conditions"] = "not-a-number",
        };
        var options = new AnalysisOptions
        {
            ConfigOptions = configOptions,
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(
            diagnostics,
            "ATXTST005",
            "at least 5 test cases")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_NakedReturnStatement_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NakedReturnStatement_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() { return; }
                                  }
                              }
                              """;

        var analyzer = new ExcessiveDecisionConditionsAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST005")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_NonBooleanReturnExpression_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NonBooleanReturnExpression_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Bar() { return Compute(); }
                                      private int Compute() { return 1; }
                                  }
                              }
                              """;

        var analyzer = new ExcessiveDecisionConditionsAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST005")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ParenthesizedMixedChainAboveThreshold_CountsFlattenedLeaves.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ParenthesizedMixedChainAboveThreshold_CountsFlattenedLeaves(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool Bar(bool a, bool b, bool c, bool d) { return (a && b) || (c && d); }
                                  }
                              }
                              """;

        var analyzer = new ExcessiveDecisionConditionsAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(
            diagnostics,
            "ATXTST005",
            "combines 4 leaf conditions")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ReturnBooleanAboveThreshold_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ReturnBooleanAboveThreshold_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool Bar(bool a, bool b, bool c, bool d) { return a && b && c && d; }
                                  }
                              }
                              """;

        var analyzer = new ExcessiveDecisionConditionsAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(
            diagnostics,
            "ATXTST005",
            "at least 5 test cases")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_TernaryConditionAboveThreshold_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TernaryConditionAboveThreshold_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Bar(bool a, bool b, bool c, bool d) { return (a && b && c && d) ? 1 : 0; }
                                  }
                              }
                              """;

        var analyzer = new ExcessiveDecisionConditionsAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST005")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ThreeConditionsAtDefaultThreshold_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ThreeConditionsAtDefaultThreshold_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(bool a, bool b, bool c) { if (a && b && c) { } }
                                  }
                              }
                              """;

        var analyzer = new ExcessiveDecisionConditionsAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST005")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_WhileConditionAboveThreshold_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_WhileConditionAboveThreshold_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(bool a, bool b, bool c, bool d) { while (a && b && c && d) { break; } }
                                  }
                              }
                              """;

        var analyzer = new ExcessiveDecisionConditionsAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST005")).IsTrue();
    }
}
