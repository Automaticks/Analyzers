using Automaticks.Threading.Tasks;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.Threading.Tasks.Analyzers.Tests;

public class AsyncCancellationTokenAnalyzerTests
{
    [Test]
    public async Task Analyze_OverrideOfExternalTaskMethod_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AsyncCancellationTokenAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXTA008")).IsFalse();
    }

    [Test]
    public async Task Analyze_PrivateTaskMethodWithoutCancellationToken_ReportsDiagnostic()
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      private Task HelperAsync() => Task.CompletedTask;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AsyncCancellationTokenAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXTA008")).IsTrue();
    }

    [Test]
    public async Task Analyze_PublicTaskMethodWithCancellationToken_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AsyncCancellationTokenAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXTA008")).IsFalse();
    }

    [Test]
    public async Task Analyze_PublicTaskMethodWithoutCancellationToken_ReportsDiagnostic()
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public Task FooAsync() => Task.CompletedTask;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AsyncCancellationTokenAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXTA008")).IsTrue();
    }

    [Test]
    public async Task Analyze_IAsyncEnumerableMethodWithoutCancellationToken_ReportsDiagnostic()
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Foo {
                                      public IAsyncEnumerable<int> GetItemsAsync() => throw new System.NotImplementedException();
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AsyncCancellationTokenAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXTA008")).IsTrue();
    }

    [Test]
    public async Task Analyze_IAsyncEnumerableMethodWithCancellationToken_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AsyncCancellationTokenAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXTA008")).IsFalse();
    }
}
