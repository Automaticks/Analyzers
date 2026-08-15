using Automaticks.Testing.Testability;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Testing.Analyzers.Tests;

/// <summary>
///     Tests for AmbientDependencyAnalyzer.
/// </summary>
public class AmbientDependencyAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_DateTimeNow_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_DateTimeNow_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public DateTime Bar() { return DateTime.Now; }
                                  }
                              }
                              """;

        var analyzer = new AmbientDependencyAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(
            diagnostics,
            "ATXTST010",
            "TimeProvider")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_FileRead_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FileRead_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.IO;
                              namespace MyApp {
                                  public class Foo {
                                      public string Bar(string path) { return File.ReadAllText(path); }
                                  }
                              }
                              """;

        var analyzer = new AmbientDependencyAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(
            diagnostics,
            "ATXTST010",
            "file-system abstraction")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_GuidNewGuid_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_GuidNewGuid_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public Guid Bar() { return Guid.NewGuid(); }
                                  }
                              }
                              """;

        var analyzer = new AmbientDependencyAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST010")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_InjectedTimeProvider_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_InjectedTimeProvider_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      private readonly TimeProvider _clock;
                                      public Foo(TimeProvider clock) { _clock = clock; }
                                      public DateTimeOffset Bar() { return _clock.GetUtcNow(); }
                                  }
                              }
                              """;

        var analyzer = new AmbientDependencyAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST010")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_SeededRandom_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SeededRandom_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public Random Bar() { return new Random(1234); }
                                  }
                              }
                              """;

        var analyzer = new AmbientDependencyAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST010")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_UnseededRandom_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_UnseededRandom_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public Random Bar() { return new Random(); }
                                  }
                              }
                              """;

        var analyzer = new AmbientDependencyAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(
            diagnostics,
            "ATXTST010",
            "explicit seed")).IsTrue();
    }
}
