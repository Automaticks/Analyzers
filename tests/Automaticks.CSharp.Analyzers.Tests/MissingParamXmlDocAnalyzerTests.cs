using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class MissingParamXmlDocAnalyzerTests
{
    [Test]
    public async Task Analyze_PublicMethodWithParamsAndNoParamTags_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      /// <summary>
                                      ///     Does something.
                                      /// </summary>
                                      public void Bar(int value) {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingParamXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS052")).IsTrue();
    }

    [Test]
    public async Task Analyze_PublicMethodWithParamTagForEachParam_NoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      /// <summary>
                                      ///     Does something.
                                      /// </summary>
                                      /// <param name="value">The value.</param>
                                      public void Bar(int value) {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingParamXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS052")).IsFalse();
    }

    [Test]
    public async Task Analyze_PublicMethodWithInheritDoc_NoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      /// <inheritdoc/>
                                      public void Bar(int value) {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingParamXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS052")).IsFalse();
    }

    [Test]
    public async Task Analyze_OverrideMethodWithParamsAndNoParamTags_NoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Base {
                                      /// <summary>
                                      ///     Does something.
                                      /// </summary>
                                      /// <param name="value">The value.</param>
                                      public virtual void Bar(int value) {}
                                  }

                                  public class Derived : Base {
                                      public override void Bar(int value) {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingParamXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS052")).IsFalse();
    }

    [Test]
    public async Task Analyze_PublicMethodWithNoParams_NoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      /// <summary>
                                      ///     Does something.
                                      /// </summary>
                                      public void Bar() {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingParamXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS052")).IsFalse();
    }

    [Test]
    public async Task Analyze_PublicMethodMissingOneOfMultipleParamTags_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      /// <summary>
                                      ///     Does something.
                                      /// </summary>
                                      /// <param name="first">The first.</param>
                                      public void Bar(int first, int second) {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingParamXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS052")).IsTrue();
    }

    [Test]
    public async Task Analyze_PublicConstructorWithParamsAndNoParamTags_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      /// <summary>
                                      ///     Initializes a new instance.
                                      /// </summary>
                                      public Foo(int value) {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingParamXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS052")).IsTrue();
    }

    [Test]
    public async Task Analyze_PublicConstructorWithAllParamTags_NoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      /// <summary>
                                      ///     Initializes a new instance.
                                      /// </summary>
                                      /// <param name="value">The value.</param>
                                      public Foo(int value) {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingParamXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS052")).IsFalse();
    }

    [Test]
    public async Task Analyze_PrivateMethodWithParamsAndNoParamTags_NoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private void Bar(int value) {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingParamXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS052")).IsFalse();
    }

    [Test]
    public async Task Analyze_ExplicitInterfaceImplementationWithParams_NoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo {
                                      /// <summary>
                                      ///     Does something.
                                      /// </summary>
                                      /// <param name="value">The value.</param>
                                      void Bar(int value);
                                  }

                                  public class Foo : IFoo {
                                      void IFoo.Bar(int value) {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingParamXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS052")).IsFalse();
    }

    [Test]
    public async Task Analyze_MultipleParamsAllDocumented_NoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      /// <summary>
                                      ///     Does something.
                                      /// </summary>
                                      /// <param name="first">First.</param>
                                      /// <param name="second">Second.</param>
                                      public void Bar(int first, string second) {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingParamXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS052")).IsFalse();
    }

    [Test]
    public async Task Analyze_InterfaceMethodWithParamsAndNoParamTags_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo {
                                      /// <summary>
                                      ///     Does something.
                                      /// </summary>
                                      void Bar(int value);
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingParamXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS052")).IsTrue();
    }
}
