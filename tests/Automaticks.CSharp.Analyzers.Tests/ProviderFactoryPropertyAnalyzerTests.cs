using Automaticks.CSharp;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

/// <summary>
///     Tests for ProviderFactoryPropertyAnalyzer.
/// </summary>
public class ProviderFactoryPropertyAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_MethodOnProvider_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodOnProvider_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFooProvider {
                                      string GetFoo();
                                  }
                              }
                              """;

        var analyzer = new ProviderFactoryPropertyAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS004")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_MethodOnSession_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodOnSession_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class FooSession {
                                      public string GetName() => string.Empty;
                                  }
                              }
                              """;

        var analyzer = new ProviderFactoryPropertyAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS004")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_PropertyOnNonProvider_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PropertyOnNonProvider_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class FooService {
                                      public string Name { get; set; } = string.Empty;
                                  }
                              }
                              """;

        var analyzer = new ProviderFactoryPropertyAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS004")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_PropertyOnProvider_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PropertyOnProvider_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFooProvider {
                                      string Name { get; }
                                  }
                              }
                              """;

        var analyzer = new ProviderFactoryPropertyAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS004")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_PropertyOnSession_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PropertyOnSession_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class FooSession {
                                      public string Name { get; } = string.Empty;
                                  }
                              }
                              """;

        var analyzer = new ProviderFactoryPropertyAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS004")).IsTrue();
    }
}
