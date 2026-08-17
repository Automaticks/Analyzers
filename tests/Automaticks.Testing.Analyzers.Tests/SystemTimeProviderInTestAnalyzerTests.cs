using Automaticks.Testing;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Testing.Analyzers.Tests;

/// <summary>
///     Tests for SystemTimeProviderInTestAnalyzer.
/// </summary>
public class SystemTimeProviderInTestAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_FakeTimeProviderInTestProject_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FakeTimeProviderInTestProject_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp.Tests {
                                  public sealed class FakeTimeProvider : TimeProvider { }
                                  public class FooTests {
                                      public TimeProvider Bar_Something_ReturnsClock() { return new FakeTimeProvider(); }
                                  }
                              }
                              """;

        var analyzer = new SystemTimeProviderInTestAnalyzer();
        var options = new AnalysisOptions
        {
            IsTestProject = true
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST014")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ForeignPropertyNamedSystem_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ForeignPropertyNamedSystem_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp.Tests {
                                  public static class Env { public static int System { get; set; } }
                                  public class FooTests {
                                      public int Bar_Something_ReturnsValue() { return Env.System; }
                                  }
                              }
                              """;

        var analyzer = new SystemTimeProviderInTestAnalyzer();
        var options = new AnalysisOptions
        {
            IsTestProject = true
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST014")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_MethodCallMemberAccess_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodCallMemberAccess_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp.Tests {
                                  public static class Helper { public static int Value() { return 1; } }
                                  public class FooTests {
                                      public int Bar_Something_ReturnsValue() { return Helper.Value(); }
                                  }
                              }
                              """;

        var analyzer = new SystemTimeProviderInTestAnalyzer();
        var options = new AnalysisOptions
        {
            IsTestProject = true
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST014")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_SystemTimeProviderInProductionProject_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SystemTimeProviderInProductionProject_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public TimeProvider Bar() { return TimeProvider.System; }
                                  }
                              }
                              """;

        var analyzer = new SystemTimeProviderInTestAnalyzer();
        var options = new AnalysisOptions
        {
            IsTestProject = false
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST014")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_SystemTimeProviderInTestProject_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SystemTimeProviderInTestProject_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              using System.Threading.Tasks;
                              namespace MyApp.Tests {
                                  public class FooTests {
                                      public Task Bar_Something_Waits() { return Task.Delay(200, TimeProvider.System); }
                                  }
                              }
                              """;

        var analyzer = new SystemTimeProviderInTestAnalyzer();
        var options = new AnalysisOptions
        {
            IsTestProject = true
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST014")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_UnrelatedStaticProperty_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_UnrelatedStaticProperty_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp.Tests {
                                  public class FooTests {
                                      public DateTime Bar_Something_ReturnsNow() { return DateTime.Now; }
                                  }
                              }
                              """;

        var analyzer = new SystemTimeProviderInTestAnalyzer();
        var options = new AnalysisOptions
        {
            IsTestProject = true
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST014")).IsFalse();
    }
}
