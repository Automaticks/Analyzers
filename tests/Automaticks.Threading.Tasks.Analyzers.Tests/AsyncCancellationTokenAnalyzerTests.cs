using Automaticks.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Threading.Tasks.Analyzers.Tests;

/// <summary>
///     Tests for AsyncCancellationTokenAnalyzer.
/// </summary>
public class AsyncCancellationTokenAnalyzerTests
{

    /// <summary>
    ///     Tests that Analyze_IAsyncEnumerableMethodWithCancellationToken_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_IAsyncEnumerableMethodWithCancellationToken_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections.Generic;
                              using System.Threading;
                              namespace MyApp {
                                  public class Foo {
                                      public IAsyncEnumerable<int> GetItemsAsync(CancellationToken ct) => throw new System.NotImplementedException();
                                  }
                              }
                              """;

        var analyzer = new AsyncCancellationTokenAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA008")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_IAsyncEnumerableMethodWithoutCancellationToken_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_IAsyncEnumerableMethodWithoutCancellationToken_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Foo {
                                      public IAsyncEnumerable<int> GetItemsAsync() => throw new System.NotImplementedException();
                                  }
                              }
                              """;

        var analyzer = new AsyncCancellationTokenAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA008")).IsTrue();
    }
    /// <summary>
    ///     Tests that Analyze_OverrideOfExternalTaskMethod_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_OverrideOfExternalTaskMethod_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class MyStream : System.IO.Stream {
                                      public override bool CanRead => false;
                                      public override bool CanSeek => false;
                                      public override bool CanWrite => false;
                                      public override long Length => 0;
                                      public override long Position { get; set; }
                                      public override void Flush() {}
                                      public override int Read(byte[] buffer, int offset, int count) => 0;
                                      public override long Seek(long offset, System.IO.SeekOrigin origin) => 0;
                                      public override void SetLength(long value) {}
                                      public override void Write(byte[] buffer, int offset, int count) {}
                                      public override Task FlushAsync(System.Threading.CancellationToken cancellationToken) => Task.CompletedTask;
                                  }
                              }
                              """;

        var analyzer = new AsyncCancellationTokenAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA008")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_PrivateTaskMethodWithoutCancellationToken_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PrivateTaskMethodWithoutCancellationToken_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      private Task HelperAsync() => Task.CompletedTask;
                                  }
                              }
                              """;

        var analyzer = new AsyncCancellationTokenAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA008")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_PublicTaskMethodWithCancellationToken_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PublicTaskMethodWithCancellationToken_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading;
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public Task FooAsync(CancellationToken ct) => Task.CompletedTask;
                                  }
                              }
                              """;

        var analyzer = new AsyncCancellationTokenAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA008")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_PublicTaskMethodWithoutCancellationToken_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PublicTaskMethodWithoutCancellationToken_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public Task FooAsync() => Task.CompletedTask;
                                  }
                              }
                              """;

        var analyzer = new AsyncCancellationTokenAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTA008")).IsTrue();
    }
}
