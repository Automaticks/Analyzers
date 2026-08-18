using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Documentation;

/// <summary>
///     Tests for MissingParamXmlDocAnalyzer.
/// </summary>
public class MissingParamXmlDocAnalyzerTests
{

    /// <summary>
    ///     Tests that Analyze_ExplicitInterfaceImplementationWithParams_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExplicitInterfaceImplementationWithParams_NoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new MissingParamXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS052")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExplicitInterfaceIndexer_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExplicitInterfaceIndexer_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp;
                              /// <summary>A bag.</summary>
                              public interface IBag
                              {
                                  /// <summary>Gets an item.</summary>
                                  /// <param name="index">The index.</param>
                                  int this[int index] { get; }
                              }
                              public class Foo : IBag
                              {
                                  int IBag.this[int index] { get { return index; } }
                              }
                              """;

        var analyzer = new MissingParamXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS052")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_InterfaceMethodWithParamsAndNoParamTags_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_InterfaceMethodWithParamsAndNoParamTags_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new MissingParamXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS052")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_MethodInsidePrivateNestedType_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodInsidePrivateNestedType_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp;
                              public class Outer
                              {
                                  private class Inner
                                  {
                                      /// <summary>Does work.</summary>
                                      public void Bar(int value) { }
                                  }
                              }
                              """;

        var analyzer = new MissingParamXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS052")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_MultipleParamsAllDocumented_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MultipleParamsAllDocumented_NoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new MissingParamXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS052")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_OverrideMethodWithParamsAndNoParamTags_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_OverrideMethodWithParamsAndNoParamTags_NoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new MissingParamXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS052")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_PrivateMethodWithParamsAndNoParamTags_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PrivateMethodWithParamsAndNoParamTags_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private void Bar(int value) {}
                                  }
                              }
                              """;

        var analyzer = new MissingParamXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS052")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_PublicConstructorWithAllParamTags_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PublicConstructorWithAllParamTags_NoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new MissingParamXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS052")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_PublicConstructorWithParamsAndNoParamTags_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PublicConstructorWithParamsAndNoParamTags_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new MissingParamXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS052")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_PublicDelegateWithParamsAndNoParamTags_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PublicDelegateWithParamsAndNoParamTags_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp;
                              public class Foo
                              {
                                  /// <summary>Formats a value.</summary>
                                  public delegate string Formatter(int value);
                              }
                              """;

        var analyzer = new MissingParamXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS052")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_PublicDelegateWithParamTag_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PublicDelegateWithParamTag_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp;
                              public class Foo
                              {
                                  /// <summary>Formats a value.</summary>
                                  /// <param name="value">The value.</param>
                                  public delegate string Formatter(int value);
                              }
                              """;

        var analyzer = new MissingParamXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS052")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_PublicIndexerWithParamsAndNoParamTags_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PublicIndexerWithParamsAndNoParamTags_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp;
                              public class Foo
                              {
                                  /// <summary>Gets an item.</summary>
                                  public int this[int index] { get { return index; } }
                              }
                              """;

        var analyzer = new MissingParamXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS052")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_PublicMethodMissingOneOfMultipleParamTags_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PublicMethodMissingOneOfMultipleParamTags_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new MissingParamXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS052")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_PublicMethodWithInheritDoc_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PublicMethodWithInheritDoc_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      /// <inheritdoc/>
                                      public void Bar(int value) {}
                                  }
                              }
                              """;

        var analyzer = new MissingParamXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS052")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_PublicMethodWithNoParams_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PublicMethodWithNoParams_NoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new MissingParamXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS052")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_PublicMethodWithParamsAndNoParamTags_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PublicMethodWithParamsAndNoParamTags_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new MissingParamXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS052")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_PublicMethodWithParamTagForEachParam_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PublicMethodWithParamTagForEachParam_NoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new MissingParamXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS052")).IsFalse();
    }
}
