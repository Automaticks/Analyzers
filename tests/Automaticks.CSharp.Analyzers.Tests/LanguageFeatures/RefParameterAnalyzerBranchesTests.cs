using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures;

/// <summary>
///     Tests that RefParameterAnalyzer covers its constructor, override, interface, and local-function branches.
/// </summary>
public class RefParameterAnalyzerBranchesTests
{
    /// <summary>
    ///     Tests that Analyze_ConstructorWithTwoRefParams_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConstructorWithTwoRefParams_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public Foo(ref int a, ref int b) { a = 0; b = 0; }
                                  }
                              }
                              """;

        var analyzer = new RefParameterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasAnyId(diagnostics, ["ATXCS025", "ATXCS026", "ATXCS027"])).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExternalOverrideWithTwoRefParams_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExternalOverrideWithTwoRefParams_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string externalSource = """
                                      namespace External {
                                          public abstract class Base {
                                              public abstract void SetValues(ref int a, ref int b);
                                          }
                                      }
                                      """;
        const string source = """
                              using External;
                              namespace MyApp {
                                  public class Derived : Base {
                                      public override void SetValues(ref int a, ref int b) { a = 0; b = 0; }
                                  }
                              }
                              """;

        var externalRef = AnalyzerTestRunner.CompileToReference(externalSource);
        var analyzer = new RefParameterAnalyzer();
        var options = new AnalysisOptions
        {
            AdditionalReferences = [externalRef]
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasAnyId(diagnostics, ["ATXCS025", "ATXCS026", "ATXCS027"])).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ImplicitExternalInterfaceRefParam_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ImplicitExternalInterfaceRefParam_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string externalSource = """
                                      namespace External {
                                          public interface IFoo {
                                              void SetValues(ref int a, ref int b);
                                          }
                                      }
                                      """;
        const string source = """
                              using External;
                              namespace MyApp {
                                  public class FooImpl : IFoo {
                                      public void SetValues(ref int a, ref int b) { a = 0; b = 0; }
                                  }
                              }
                              """;

        var externalRef = AnalyzerTestRunner.CompileToReference(externalSource);
        var analyzer = new RefParameterAnalyzer();
        var options = new AnalysisOptions
        {
            AdditionalReferences = [externalRef]
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasAnyId(diagnostics, ["ATXCS025", "ATXCS026", "ATXCS027"])).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_LocalFunctionSetPropertyWithRefFirst_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LocalFunctionSetPropertyWithRefFirst_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method() {
                                          bool SetProperty(ref int field, int value) { field = value; return true; }
                                          var x = 0;
                                          SetProperty(ref x, 1);
                                      }
                                  }
                              }
                              """;

        var analyzer = new RefParameterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasAnyId(diagnostics, ["ATXCS025", "ATXCS026", "ATXCS027"])).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_LocalFunctionWithNoRefParams_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LocalFunctionWithNoRefParams_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method() {
                                          void Helper(int a, int b) { }
                                          Helper(1, 2);
                                      }
                                  }
                              }
                              """;

        var analyzer = new RefParameterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasAnyId(diagnostics, ["ATXCS025", "ATXCS026", "ATXCS027"])).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_LocalFunctionWithRefNotFirst_ReportsForbiddenAndPosition.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LocalFunctionWithRefNotFirst_ReportsForbiddenAndPosition(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method() {
                                          void DoWork(int a, ref int value) { value = 0; }
                                          var x = 0;
                                          DoWork(1, ref x);
                                      }
                                  }
                              }
                              """;

        var analyzer = new RefParameterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS025")).IsTrue();
        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS026")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_LocalFunctionWithTwoRefs_ReportsForbiddenAndCount.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LocalFunctionWithTwoRefs_ReportsForbiddenAndCount(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method() {
                                          void DoWork(ref int a, ref int b) { a = 0; b = 0; }
                                          var x = 0;
                                          var y = 0;
                                          DoWork(ref x, ref y);
                                      }
                                  }
                              }
                              """;

        var analyzer = new RefParameterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS025")).IsTrue();
        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS027")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_LocalOverrideWithTwoRefParams_ReportsDiagnostics.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LocalOverrideWithTwoRefParams_ReportsDiagnostics(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public abstract class Base {
                                      public abstract void SetValues(ref int a, ref int b);
                                  }

                                  public class Derived : Base {
                                      public override void SetValues(ref int a, ref int b) { a = 0; b = 0; }
                                  }
                              }
                              """;

        var analyzer = new RefParameterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS025")).IsTrue();
        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS027")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_MethodWithNoRefParams_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodWithNoRefParams_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void DoWork(int a, int b) { }
                                  }
                              }
                              """;

        var analyzer = new RefParameterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasAnyId(diagnostics, ["ATXCS025", "ATXCS026", "ATXCS027"])).IsFalse();
    }
}
