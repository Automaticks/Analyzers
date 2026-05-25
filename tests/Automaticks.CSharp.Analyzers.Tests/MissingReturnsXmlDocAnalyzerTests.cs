using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class MissingReturnsXmlDocAnalyzerTests
{
    [Test]
    public async Task Analyze_PublicNonVoidMethodWithNoReturnsTag_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      /// <summary>
                                      ///     Gets a value.
                                      /// </summary>
                                      public int GetValue() => 0;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingReturnsXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS053")).IsTrue();
    }

    [Test]
    public async Task Analyze_PublicNonVoidMethodWithReturnsTag_NoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      /// <summary>
                                      ///     Gets a value.
                                      /// </summary>
                                      /// <returns>The value.</returns>
                                      public int GetValue() => 0;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingReturnsXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS053")).IsFalse();
    }

    [Test]
    public async Task Analyze_PublicVoidMethod_NoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      /// <summary>
                                      ///     Does something.
                                      /// </summary>
                                      public void DoSomething() {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingReturnsXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS053")).IsFalse();
    }

    [Test]
    public async Task Analyze_PublicNonVoidMethodWithInheritDoc_NoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      /// <inheritdoc/>
                                      public int GetValue() => 0;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingReturnsXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS053")).IsFalse();
    }

    [Test]
    public async Task Analyze_OverrideNonVoidMethodWithNoReturnsTag_NoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Base {
                                      /// <summary>
                                      ///     Gets a value.
                                      /// </summary>
                                      /// <returns>The value.</returns>
                                      public virtual int GetValue() => 0;
                                  }

                                  public class Derived : Base {
                                      public override int GetValue() => 1;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingReturnsXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS053")).IsFalse();
    }

    [Test]
    public async Task Analyze_PublicTaskReturningMethodWithNoReturnsTag_ReportsDiagnostic()
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      /// <summary>
                                      ///     Loads something asynchronously.
                                      /// </summary>
                                      public async Task<int> LoadAsync() => await System.Threading.Tasks.Task.FromResult(0);
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingReturnsXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS053")).IsTrue();
    }

    [Test]
    public async Task Analyze_PublicNonGenericTaskMethodWithNoReturnsTag_ReportsDiagnostic()
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Foo {
                                      /// <summary>
                                      ///     Runs asynchronously.
                                      /// </summary>
                                      public async Task RunAsync() => await System.Threading.Tasks.Task.CompletedTask;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingReturnsXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS053")).IsTrue();
    }

    [Test]
    public async Task Analyze_PrivateNonVoidMethodWithNoReturnsTag_NoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private int GetValue() => 0;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingReturnsXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS053")).IsFalse();
    }

    [Test]
    public async Task Analyze_ProtectedNonVoidMethodWithNoReturnsTag_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      /// <summary>
                                      ///     Foo.
                                      /// </summary>
                                      public Foo() {}

                                      protected int GetValue() => 0;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingReturnsXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS053")).IsTrue();
    }

    [Test]
    public async Task Analyze_ExplicitInterfaceNonVoidMethodWithNoReturnsTag_NoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo {
                                      /// <summary>
                                      ///     Gets a value.
                                      /// </summary>
                                      /// <returns>The value.</returns>
                                      int GetValue();
                                  }

                                  public class Foo : IFoo {
                                      int IFoo.GetValue() => 0;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingReturnsXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS053")).IsFalse();
    }
}
