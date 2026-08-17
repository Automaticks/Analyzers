using Automaticks.CSharp.Naming;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

/// <summary>
///     Tests for StaticMethodInRecordOrStructAnalyzer.
/// </summary>
public class StaticMethodInRecordOrStructAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_InstanceMethodInRecord_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_InstanceMethodInRecord_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public record Foo {
                                      public int Bar() { return 1; }
                                  }
                              }
                              """;

        var analyzer = new StaticMethodInRecordOrStructAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS069")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_StaticMethodInClass_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StaticMethodInClass_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public static class Foo {
                                      public static int Bar() { return 1; }
                                  }
                              }
                              """;

        var analyzer = new StaticMethodInRecordOrStructAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS069")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_StaticMethodInRecord_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StaticMethodInRecord_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public record Foo {
                                      public static int Bar() { return 1; }
                                  }
                              }
                              """;

        var analyzer = new StaticMethodInRecordOrStructAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS069")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_StaticMethodInRecordStruct_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StaticMethodInRecordStruct_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public record struct Foo {
                                      public static int Bar() { return 1; }
                                  }
                              }
                              """;

        var analyzer = new StaticMethodInRecordOrStructAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS069")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_StaticMethodInStruct_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StaticMethodInStruct_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public struct Foo {
                                      public static int Bar() { return 1; }
                                  }
                              }
                              """;

        var analyzer = new StaticMethodInRecordOrStructAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS069")).IsTrue();
    }
}
