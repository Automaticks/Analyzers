using Automaticks.CSharp;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures;

/// <summary>
///     Tests that OutParameterAnalyzer covers its constructor, override, interface, and local-function branches.
/// </summary>
public class OutParameterAnalyzerBranchesTests
{
    /// <summary>
    ///     Tests that Analyze_ConstructorWithTwoOutParams_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConstructorWithTwoOutParams_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public Foo(out int a, out int b) { a = 0; b = 0; }
                                  }
                              }
                              """;

        var analyzer = new OutParameterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasAnyId(diagnostics, ["ATXCS023", "ATXCS024"])).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExternalOverrideWithTwoOutParams_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExternalOverrideWithTwoOutParams_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string externalSource = """
                                      namespace External {
                                          public abstract class Base {
                                              public abstract void GetValues(out int a, out int b);
                                          }
                                      }
                                      """;
        const string source = """
                              using External;
                              namespace MyApp {
                                  public class Derived : Base {
                                      public override void GetValues(out int a, out int b) { a = 0; b = 0; }
                                  }
                              }
                              """;

        var externalRef = AnalyzerTestRunner.CompileToReference(externalSource);
        var analyzer = new OutParameterAnalyzer();
        var options = new AnalysisOptions
        {
            AdditionalReferences = [externalRef]
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasAnyId(diagnostics, ["ATXCS023", "ATXCS024"])).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ImplicitExternalInterfaceOutParam_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ImplicitExternalInterfaceOutParam_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string externalSource = """
                                      namespace External {
                                          public interface IFoo {
                                              void GetValues(out int a, out int b);
                                          }
                                      }
                                      """;
        const string source = """
                              using External;
                              namespace MyApp {
                                  public class FooImpl : IFoo {
                                      public void GetValues(out int a, out int b) { a = 0; b = 0; }
                                  }
                              }
                              """;

        var externalRef = AnalyzerTestRunner.CompileToReference(externalSource);
        var analyzer = new OutParameterAnalyzer();
        var options = new AnalysisOptions
        {
            AdditionalReferences = [externalRef]
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasAnyId(diagnostics, ["ATXCS023", "ATXCS024"])).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_LocalFunctionWithOneOutParamLast_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LocalFunctionWithOneOutParamLast_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method() {
                                          bool TryGet(int id, out string result) { result = ""; return true; }
                                          TryGet(1, out var r);
                                      }
                                  }
                              }
                              """;

        var analyzer = new OutParameterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasAnyId(diagnostics, ["ATXCS023", "ATXCS024"])).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_LocalFunctionWithOutParamNotLast_ReportsAtxCs024.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LocalFunctionWithOutParamNotLast_ReportsAtxCs024(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method() {
                                          void DoWork(out int result, int extra) { result = 0; }
                                          DoWork(out var r, 1);
                                      }
                                  }
                              }
                              """;

        var analyzer = new OutParameterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS024")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_LocalFunctionWithTwoOutParams_ReportsAtxCs023.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LocalFunctionWithTwoOutParams_ReportsAtxCs023(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method() {
                                          void GetValues(out int a, out int b) { a = 0; b = 0; }
                                          GetValues(out var x, out var y);
                                      }
                                  }
                              }
                              """;

        var analyzer = new OutParameterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS023")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_LocalFunctionWithTwoOutParamsFirstNotLast_ReportsBothDiagnostics.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LocalFunctionWithTwoOutParamsFirstNotLast_ReportsBothDiagnostics(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Method() {
                                          void DoWork(out int a, out int b, int extra) { a = 0; b = 0; }
                                          DoWork(out var x, out var y, 1);
                                      }
                                  }
                              }
                              """;

        var analyzer = new OutParameterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS023")).IsTrue();
        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS024")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_LocalOverrideWithTwoOutParams_ReportsAtxCs023.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LocalOverrideWithTwoOutParams_ReportsAtxCs023(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public abstract class Base {
                                      public abstract void GetValues(out int a, out int b);
                                  }

                                  public class Derived : Base {
                                      public override void GetValues(out int a, out int b) { a = 0; b = 0; }
                                  }
                              }
                              """;

        var analyzer = new OutParameterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS023")).IsTrue();
    }
}
