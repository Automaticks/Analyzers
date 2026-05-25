using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class MissingSummaryXmlDocAnalyzerTests
{
    [Test]
    public async Task Analyze_PublicClassWithoutDocComment_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {}
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingSummaryXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS051")).IsTrue();
    }

    [Test]
    public async Task Analyze_PublicClassWithSummary_NoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///     A class.
                                  /// </summary>
                                  public class Foo {}
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingSummaryXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS051")).IsFalse();
    }

    [Test]
    public async Task Analyze_PublicClassWithInheritDoc_NoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  /// <inheritdoc/>
                                  public class Foo {}
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingSummaryXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS051")).IsFalse();
    }

    [Test]
    public async Task Analyze_PrivateMethodWithoutDocComment_NoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///     A class.
                                  /// </summary>
                                  public class Foo {
                                      private void Bar() {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingSummaryXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS051")).IsFalse();
    }

    [Test]
    public async Task Analyze_ProtectedMethodWithoutDocComment_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      /// <summary>
                                      ///     Foo.
                                      /// </summary>
                                      public Foo() {}

                                      protected void Bar() {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingSummaryXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS051")).IsTrue();
    }

    [Test]
    public async Task Analyze_OverrideMethodWithoutDocComment_NoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///     The base class.
                                  /// </summary>
                                  public class Base {
                                      /// <summary>
                                      ///     Base.
                                      /// </summary>
                                      public Base() {}

                                      /// <summary>
                                      ///     Does something.
                                      /// </summary>
                                      public virtual void Bar() {}
                                  }

                                  /// <summary>
                                  ///     The derived class.
                                  /// </summary>
                                  public class Derived : Base {
                                      /// <summary>
                                      ///     Derived.
                                      /// </summary>
                                      public Derived() {}

                                      public override void Bar() {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingSummaryXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS051")).IsFalse();
    }

    [Test]
    public async Task Analyze_PublicConstructorWithoutDocComment_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///     A class.
                                  /// </summary>
                                  public class Foo {
                                      public Foo() {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingSummaryXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS051")).IsTrue();
    }

    [Test]
    public async Task Analyze_PublicPropertyWithoutDocComment_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///     A class.
                                  /// </summary>
                                  public class Foo {
                                      /// <summary>
                                      ///     Foo.
                                      /// </summary>
                                      public Foo() {}

                                      public int Value { get; set; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingSummaryXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS051")).IsTrue();
    }

    [Test]
    public async Task Analyze_PublicConstFieldWithoutDocComment_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///     A class.
                                  /// </summary>
                                  public class Foo {
                                      /// <summary>
                                      ///     Foo.
                                      /// </summary>
                                      public Foo() {}

                                      public const int MaxValue = 100;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingSummaryXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS051")).IsTrue();
    }

    [Test]
    public async Task Analyze_PublicEventWithoutDocComment_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public delegate void MyHandler();

                                  /// <summary>
                                  ///     A class.
                                  /// </summary>
                                  public class Foo {
                                      /// <summary>
                                      ///     Foo.
                                      /// </summary>
                                      public Foo() {}

                                      public event MyHandler? Changed;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingSummaryXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS051")).IsTrue();
    }

    [Test]
    public async Task Analyze_EnumMemberWithoutDocComment_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///     An enum.
                                  /// </summary>
                                  public enum Color {
                                      Red,
                                      Green
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingSummaryXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS051")).IsTrue();
    }

    [Test]
    public async Task Analyze_PublicDelegateWithoutDocComment_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public delegate void MyCallback();
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingSummaryXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS051")).IsTrue();
    }

    [Test]
    public async Task Analyze_PublicIndexerWithoutDocComment_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///     A class.
                                  /// </summary>
                                  public class Foo {
                                      /// <summary>
                                      ///     Foo.
                                      /// </summary>
                                      public Foo() {}

                                      public int this[int index] => index;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingSummaryXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS051")).IsTrue();
    }

    [Test]
    public async Task Analyze_ExplicitInterfaceImplementationWithoutDocComment_NoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///     The foo interface.
                                  /// </summary>
                                  public interface IFoo {
                                      /// <summary>
                                      ///     Does something.
                                      /// </summary>
                                      void Bar();
                                  }

                                  /// <summary>
                                  ///     A class.
                                  /// </summary>
                                  public class Foo : IFoo {
                                      /// <summary>
                                      ///     Foo.
                                      /// </summary>
                                      public Foo() {}

                                      void IFoo.Bar() {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingSummaryXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS051")).IsFalse();
    }

    [Test]
    public async Task Analyze_InterfaceMethodWithoutDocComment_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///     An interface.
                                  /// </summary>
                                  public interface IFoo {
                                      void Bar();
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingSummaryXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS051")).IsTrue();
    }

    [Test]
    public async Task Analyze_AllMembersDocumented_NoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///     A fully documented class.
                                  /// </summary>
                                  public class Foo {
                                      /// <summary>
                                      ///     Initializes a new instance.
                                      /// </summary>
                                      public Foo() {}

                                      /// <summary>
                                      ///     Gets the value.
                                      /// </summary>
                                      public int Value { get; set; }

                                      /// <summary>
                                      ///     Does something.
                                      /// </summary>
                                      public void Bar() {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingSummaryXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS051")).IsFalse();
    }
}
