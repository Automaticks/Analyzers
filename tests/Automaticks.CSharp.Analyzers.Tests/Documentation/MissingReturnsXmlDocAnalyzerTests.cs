using Automaticks.CSharp;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Documentation;

/// <summary>
///     Tests for MissingReturnsXmlDocAnalyzer.
/// </summary>
public class MissingReturnsXmlDocAnalyzerTests
{

    /// <summary>
    ///     Tests that Analyze_ExplicitInterfaceNonVoidMethodWithNoReturnsTag_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExplicitInterfaceNonVoidMethodWithNoReturnsTag_NoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new MissingReturnsXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS053")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_OverrideNonVoidMethodWithNoReturnsTag_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_OverrideNonVoidMethodWithNoReturnsTag_NoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new MissingReturnsXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS053")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_PrivateNonVoidMethodWithNoReturnsTag_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PrivateNonVoidMethodWithNoReturnsTag_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private int GetValue() => 0;
                                  }
                              }
                              """;

        var analyzer = new MissingReturnsXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS053")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ProtectedNonVoidMethodWithNoReturnsTag_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ProtectedNonVoidMethodWithNoReturnsTag_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new MissingReturnsXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS053")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_PublicNonGenericTaskMethodWithNoReturnsTag_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PublicNonGenericTaskMethodWithNoReturnsTag_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new MissingReturnsXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS053")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_PublicNonVoidMethodWithInheritDoc_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PublicNonVoidMethodWithInheritDoc_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      /// <inheritdoc/>
                                      public int GetValue() => 0;
                                  }
                              }
                              """;

        var analyzer = new MissingReturnsXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS053")).IsFalse();
    }
    /// <summary>
    ///     Tests that Analyze_PublicNonVoidMethodWithNoReturnsTag_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PublicNonVoidMethodWithNoReturnsTag_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new MissingReturnsXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS053")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_PublicNonVoidMethodWithReturnsTag_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PublicNonVoidMethodWithReturnsTag_NoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new MissingReturnsXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS053")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_PublicTaskReturningMethodWithNoReturnsTag_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PublicTaskReturningMethodWithNoReturnsTag_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new MissingReturnsXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS053")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_PublicVoidMethod_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PublicVoidMethod_NoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new MissingReturnsXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS053")).IsFalse();
    }
}
