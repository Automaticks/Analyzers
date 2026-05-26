using Automaticks.CSharp;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

/// <summary>
///     Tests for MissingSummaryXmlDocAnalyzer.
/// </summary>
public class MissingSummaryXmlDocAnalyzerTests
{

    /// <summary>
    ///     Tests that Analyze_AllMembersDocumented_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AllMembersDocumented_NoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new MissingSummaryXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS051")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_EnumMemberWithoutDocComment_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_EnumMemberWithoutDocComment_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new MissingSummaryXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS051")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ExplicitInterfaceImplementationWithoutDocComment_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExplicitInterfaceImplementationWithoutDocComment_NoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new MissingSummaryXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS051")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_InterfaceMethodWithoutDocComment_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_InterfaceMethodWithoutDocComment_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new MissingSummaryXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS051")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_OverrideMethodWithoutDocComment_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_OverrideMethodWithoutDocComment_NoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new MissingSummaryXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS051")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_PrivateMethodWithoutDocComment_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PrivateMethodWithoutDocComment_NoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new MissingSummaryXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS051")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ProtectedMethodWithoutDocComment_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ProtectedMethodWithoutDocComment_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new MissingSummaryXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS051")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_PublicClassWithInheritDoc_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PublicClassWithInheritDoc_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  /// <inheritdoc/>
                                  public class Foo {}
                              }
                              """;

        var analyzer = new MissingSummaryXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS051")).IsFalse();
    }
    /// <summary>
    ///     Tests that Analyze_PublicClassWithoutDocComment_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PublicClassWithoutDocComment_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {}
                              }
                              """;

        var analyzer = new MissingSummaryXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS051")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_PublicClassWithSummary_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PublicClassWithSummary_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///     A class.
                                  /// </summary>
                                  public class Foo {}
                              }
                              """;

        var analyzer = new MissingSummaryXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS051")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_PublicConstFieldWithoutDocComment_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PublicConstFieldWithoutDocComment_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new MissingSummaryXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS051")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_PublicConstructorWithoutDocComment_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PublicConstructorWithoutDocComment_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new MissingSummaryXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS051")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_PublicDelegateWithoutDocComment_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PublicDelegateWithoutDocComment_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public delegate void MyCallback();
                              }
                              """;

        var analyzer = new MissingSummaryXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS051")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_PublicEventWithoutDocComment_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PublicEventWithoutDocComment_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new MissingSummaryXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS051")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_PublicIndexerWithoutDocComment_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PublicIndexerWithoutDocComment_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new MissingSummaryXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS051")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_PublicPropertyWithoutDocComment_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PublicPropertyWithoutDocComment_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new MissingSummaryXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS051")).IsTrue();
    }
}
