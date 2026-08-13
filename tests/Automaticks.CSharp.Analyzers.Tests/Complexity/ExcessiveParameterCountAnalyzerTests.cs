using Automaticks.CSharp;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Complexity;

/// <summary>
///     Tests for ExcessiveParameterCountAnalyzer.
/// </summary>
public class ExcessiveParameterCountAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_AnonymousMethodWith5Params_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AnonymousMethodWith5Params_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public void DoWork() {
                                          Action<int, int, int, int, int> action = delegate(int a, int b, int c, int d, int e) {};
                                          action(1, 2, 3, 4, 5);
                                      }
                                  }
                              }
                              """;

        var analyzer = new ExcessiveParameterCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS022")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ConstructorWith4Params_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConstructorWith4Params_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public Foo(int a, int b, int c, int d) {}
                                  }
                              }
                              """;

        var analyzer = new ExcessiveParameterCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS022")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExplicitInterfaceImplWith4Params_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExplicitInterfaceImplWith4Params_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Foo : IEqualityComparer<string> {
                                      bool IEqualityComparer<string>.Equals(string? x, string? y) => x == y;
                                      int IEqualityComparer<string>.GetHashCode(string obj) => obj.GetHashCode();
                                  }
                              }
                              """;

        var analyzer = new ExcessiveParameterCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS022")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ImplicitExternalInterfaceImplWith4Params_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ImplicitExternalInterfaceImplWith4Params_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Runtime.Serialization;
                              namespace MyApp {
                                  public class MySurrogate : ISerializationSurrogate {
                                      public void GetObjectData(object obj, SerializationInfo info, StreamingContext context) {}
                                      public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector) => obj;
                                  }
                              }
                              """;

        var analyzer = new ExcessiveParameterCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS022")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_IndexerWith4Params_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_IndexerWith4Params_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int this[int x, int y, int z, int w] => x + y + z + w;
                                  }
                              }
                              """;

        var analyzer = new ExcessiveParameterCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS022")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_IndexerWith5Params_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_IndexerWith5Params_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int this[int x, int y, int z, int w, int v] => x + y + z + w + v;
                                  }
                              }
                              """;

        var analyzer = new ExcessiveParameterCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS022")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_LambdaWith5Params_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LambdaWith5Params_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public void DoWork() {
                                          Action<int, int, int, int, int> action = (a, b, c, d, e) => {};
                                          action(1, 2, 3, 4, 5);
                                      }
                                  }
                              }
                              """;

        var analyzer = new ExcessiveParameterCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS022")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_LocalFunctionWith5Params_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LocalFunctionWith5Params_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void DoWork() {
                                          void Inner(int a, int b, int c, int d, int e) {}
                                          Inner(1, 2, 3, 4, 5);
                                      }
                                  }
                              }
                              """;

        var analyzer = new ExcessiveParameterCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS022")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_MethodWith4Params_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodWith4Params_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void DoWork(int a, int b, int c, int d) {}
                                  }
                              }
                              """;

        var analyzer = new ExcessiveParameterCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS022")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_MethodWith5Params_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodWith5Params_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void DoWork(int a, int b, int c, int d, int e) {}
                                  }
                              }
                              """;

        var analyzer = new ExcessiveParameterCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS022")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_OperatorWith2Params_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_OperatorWith2Params_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public struct Vec {
                                      public int X;
                                      public static Vec operator +(Vec a, Vec b) => new Vec { X = a.X + b.X };
                                  }
                              }
                              """;

        var analyzer = new ExcessiveParameterCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS022")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_OverrideOfExternalMethodWith4Params_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_OverrideOfExternalMethodWith4Params_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
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
                                  }
                              }
                              """;

        var analyzer = new ExcessiveParameterCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS022")).IsFalse();
    }
}
