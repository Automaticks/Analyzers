using Automaticks.CSharp;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

/// <summary>
///     Tests for ParameterDefaultValueAnalyzer.
/// </summary>
public class ParameterDefaultValueAnalyzerTests
{

    /// <summary>
    ///     Tests that Analyze_AnonymousMethodWithNoParameterList_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AnonymousMethodWithNoParameterList_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public void DoWork() {
                                          Action action = delegate { };
                                          action();
                                      }
                                  }
                              }
                              """;

        var analyzer = new ParameterDefaultValueAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS057")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_AnonymousMethodWithParametersAndNoDefaults_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AnonymousMethodWithParametersAndNoDefaults_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public void DoWork() {
                                          Action<int> action = delegate(int x) { };
                                          action(1);
                                      }
                                  }
                              }
                              """;

        var analyzer = new ParameterDefaultValueAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS057")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_CancellationTokenWithDefault_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CancellationTokenWithDefault_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading;
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      public Task DoAsync(CancellationToken ct = default) => Task.CompletedTask;
                                  }
                              }
                              """;

        var analyzer = new ParameterDefaultValueAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS057")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ConstructorWithDefaultValue_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConstructorWithDefaultValue_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public Foo(string name = "default") {}
                                  }
                              }
                              """;

        var analyzer = new ParameterDefaultValueAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS057")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ConstructorWithoutDefaultValues_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConstructorWithoutDefaultValues_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public Foo(int a, string b) {}
                                  }
                              }
                              """;

        var analyzer = new ParameterDefaultValueAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS057")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExplicitExternalInterfaceImplementation_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExplicitExternalInterfaceImplementation_ReportsNoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new ParameterDefaultValueAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS057")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ImplicitExternalInterfaceImplementation_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ImplicitExternalInterfaceImplementation_ReportsNoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new ParameterDefaultValueAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS057")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ImplicitExternalInterfaceIndexerImplementation_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ImplicitExternalInterfaceIndexerImplementation_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections;
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public interface ISourceInterface { }
                                  public class MyList : ISourceInterface, IReadOnlyList<int> {
                                      public int Count => 0;
                                      public int this[int index = 0] => 0;
                                      public IEnumerator<int> GetEnumerator() => null!;
                                      IEnumerator IEnumerable.GetEnumerator() => null!;
                                  }
                              }
                              """;

        var analyzer = new ParameterDefaultValueAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS057")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_IndexerWithDefaultValue_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_IndexerWithDefaultValue_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int this[int x = 0] => x;
                                  }
                              }
                              """;

        var analyzer = new ParameterDefaultValueAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS057")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_InternalExplicitInterfaceImplementation_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_InternalExplicitInterfaceImplementation_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface IWorker {
                                      void DoWork(int x);
                                  }
                                  public class Foo : IWorker {
                                      void IWorker.DoWork(int x = 0) {}
                                  }
                              }
                              """;

        var analyzer = new ParameterDefaultValueAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS057")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_LambdaWithDefaultValue_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LambdaWithDefaultValue_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void DoWork() {
                                          var fn = (int x = 1) => x;
                                          fn();
                                      }
                                  }
                              }
                              """;

        var analyzer = new ParameterDefaultValueAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS057")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_LocalFunctionWithDefaultValue_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LocalFunctionWithDefaultValue_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void DoWork() {
                                          void Inner(bool flag = true) {}
                                          Inner();
                                      }
                                  }
                              }
                              """;

        var analyzer = new ParameterDefaultValueAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS057")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_MethodWithDefaultInTestProject_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodWithDefaultInTestProject_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp.Tests {
                                  public class FooTests {
                                      public void Setup(int retries = 3) {}
                                  }
                              }
                              """;

        var analyzer = new ParameterDefaultValueAnalyzer();
        var options = new AnalysisOptions
{
    IsTestProject = true
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS057")).IsTrue();
    }
    /// <summary>
    ///     Tests that Analyze_MethodWithDefaultValue_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodWithDefaultValue_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void DoWork(int maxHealth = 100) {}
                                  }
                              }
                              """;

        var analyzer = new ParameterDefaultValueAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS057")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_MethodWithMultipleParams_OnlyDefaultOnesAreFlagged.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodWithMultipleParams_OnlyDefaultOnesAreFlagged(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void DoWork(int a, int b = 5, string c = "x") {}
                                  }
                              }
                              """;

        var analyzer = new ParameterDefaultValueAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);
        await Assert.That(DiagnosticCollectionAssertions.CountId(diagnostics, "ATXCS057")).IsEqualTo(2);
    }

    /// <summary>
    ///     Tests that Analyze_MethodWithoutDefaultValues_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodWithoutDefaultValues_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void DoWork(int a, string b) {}
                                  }
                              }
                              """;

        var analyzer = new ParameterDefaultValueAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS057")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_OverrideOfExternalMethod_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_OverrideOfExternalMethod_ReportsNoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new ParameterDefaultValueAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS057")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_OverrideOfInternalBaseClassIndexer_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_OverrideOfInternalBaseClassIndexer_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Base {
                                      public virtual int this[int x] => 0;
                                  }
                                  public class Derived : Base {
                                      public override int this[int x = 0] => 0;
                                  }
                              }
                              """;

        var analyzer = new ParameterDefaultValueAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS057")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_OverrideOfInternalBaseClassMethod_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_OverrideOfInternalBaseClassMethod_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Base {
                                      public virtual void Method(int x) {}
                                  }
                                  public class Derived : Base {
                                      public override void Method(int x = 0) {}
                                  }
                              }
                              """;

        var analyzer = new ParameterDefaultValueAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS057")).IsTrue();
    }
}
