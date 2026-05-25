using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class ParameterDefaultValueAnalyzerTests
{
    [Test]
    public async Task Analyze_MethodWithDefaultValue_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void DoWork(int maxHealth = 100) {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ParameterDefaultValueAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS057")).IsTrue();
    }

    [Test]
    public async Task Analyze_ConstructorWithDefaultValue_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public Foo(string name = "default") {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ParameterDefaultValueAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS057")).IsTrue();
    }

    [Test]
    public async Task Analyze_LocalFunctionWithDefaultValue_ReportsDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ParameterDefaultValueAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS057")).IsTrue();
    }

    [Test]
    public async Task Analyze_LambdaWithDefaultValue_ReportsDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ParameterDefaultValueAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS057")).IsTrue();
    }

    [Test]
    public async Task Analyze_IndexerWithDefaultValue_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int this[int x = 0] => x;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ParameterDefaultValueAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS057")).IsTrue();
    }

    [Test]
    public async Task Analyze_MethodWithoutDefaultValues_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void DoWork(int a, string b) {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ParameterDefaultValueAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS057")).IsFalse();
    }

    [Test]
    public async Task Analyze_ConstructorWithoutDefaultValues_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public Foo(int a, string b) {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ParameterDefaultValueAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS057")).IsFalse();
    }

    [Test]
    public async Task Analyze_CancellationTokenWithDefault_ReportsDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ParameterDefaultValueAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS057")).IsTrue();
    }

    [Test]
    public async Task Analyze_OverrideOfExternalMethod_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ParameterDefaultValueAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS057")).IsFalse();
    }

    [Test]
    public async Task Analyze_ImplicitExternalInterfaceImplementation_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ParameterDefaultValueAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS057")).IsFalse();
    }

    [Test]
    public async Task Analyze_ExplicitExternalInterfaceImplementation_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ParameterDefaultValueAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS057")).IsFalse();
    }

    [Test]
    public async Task Analyze_MethodWithMultipleParams_OnlyDefaultOnesAreFlagged()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void DoWork(int a, int b = 5, string c = "x") {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ParameterDefaultValueAnalyzer(), source);
        var flagged = diagnostics.Where(d => d.Id == "ATXCS057").ToList();

        await Assert.That(flagged.Count).IsEqualTo(2);
    }

    [Test]
    public async Task Analyze_MethodWithDefaultInTestProject_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp.Tests {
                                  public class FooTests {
                                      public void Setup(int retries = 3) {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ParameterDefaultValueAnalyzer(), source, true);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS057")).IsTrue();
    }

    [Test]
    public async Task Analyze_InternalExplicitInterfaceImplementation_ReportsDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ParameterDefaultValueAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS057")).IsTrue();
    }

    [Test]
    public async Task Analyze_AnonymousMethodWithNoParameterList_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ParameterDefaultValueAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS057")).IsFalse();
    }

    [Test]
    public async Task Analyze_AnonymousMethodWithParametersAndNoDefaults_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ParameterDefaultValueAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS057")).IsFalse();
    }

    [Test]
    public async Task Analyze_OverrideOfInternalBaseClassMethod_ReportsDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ParameterDefaultValueAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS057")).IsTrue();
    }

    [Test]
    public async Task Analyze_OverrideOfInternalBaseClassIndexer_ReportsDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ParameterDefaultValueAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS057")).IsTrue();
    }

    [Test]
    public async Task Analyze_ImplicitExternalInterfaceIndexerImplementation_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ParameterDefaultValueAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS057")).IsFalse();
    }
}
