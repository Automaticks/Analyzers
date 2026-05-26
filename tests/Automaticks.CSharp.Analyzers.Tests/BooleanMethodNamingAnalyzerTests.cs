using Automaticks.CSharp;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

/// <summary>
///     Tests for BooleanMethodNamingAnalyzer.
/// </summary>
public class BooleanMethodNamingAnalyzerTests
{

    /// <summary>
    ///     Tests that Analyze_BoolLocalFunctionWithCanPrefix_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BoolLocalFunctionWithCanPrefix_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Execute() {
                                          bool CanValidate() => true;
                                          _ = CanValidate();
                                      }
                                  }
                              }
                              """;

        var analyzer = new BooleanMethodNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS063")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_BoolLocalFunctionWithoutPrefix_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BoolLocalFunctionWithoutPrefix_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Execute() {
                                          bool Validate() => true;
                                          _ = Validate();
                                      }
                                  }
                              }
                              """;

        var analyzer = new BooleanMethodNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS063")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_BoolMethodWithCanPrefix_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BoolMethodWithCanPrefix_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool CanExecute() => true;
                                  }
                              }
                              """;

        var analyzer = new BooleanMethodNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS063")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_BoolMethodWithHasPrefix_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BoolMethodWithHasPrefix_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool HasPermission() => true;
                                  }
                              }
                              """;

        var analyzer = new BooleanMethodNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS063")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_BoolMethodWithLowercaseCanPrefix_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BoolMethodWithLowercaseCanPrefix_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool canExecute() => true;
                                  }
                              }
                              """;

        var analyzer = new BooleanMethodNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS063")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_BoolMethodWithLowercaseHasPrefix_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BoolMethodWithLowercaseHasPrefix_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool hasAccess() => true;
                                  }
                              }
                              """;

        var analyzer = new BooleanMethodNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS063")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_BoolMethodWithoutPrefix_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BoolMethodWithoutPrefix_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool Validate() => true;
                                  }
                              }
                              """;

        var analyzer = new BooleanMethodNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS063")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ExplicitExternalInterfaceMethodImplementation_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExplicitExternalInterfaceMethodImplementation_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string externalSource = """
                                      public interface IValidator {
                                          bool Validate();
                                      }
                                      """;

        const string source = """
                              namespace MyApp {
                                  public class Validator : IValidator {
                                      bool IValidator.Validate() => true;
                                  }
                              }
                              """;

        var externalRef = AnalyzerTestRunner.CompileToReference(externalSource);
        var analyzer = new BooleanMethodNamingAnalyzer();
        var options = new AnalysisOptions
{
    AdditionalReferences = [externalRef]
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS063")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExternalOverrideMethod_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExternalOverrideMethod_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string externalSource = """
                                      public abstract class Base {
                                          public abstract bool Validate();
                                      }
                                      """;

        const string source = """
                              namespace MyApp {
                                  public class Derived : Base {
                                      public override bool Validate() => true;
                                  }
                              }
                              """;

        var externalRef = AnalyzerTestRunner.CompileToReference(externalSource);
        var analyzer = new BooleanMethodNamingAnalyzer();
        var options = new AnalysisOptions
{
    AdditionalReferences = [externalRef]
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS063")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ImplicitExternalInterfaceMethodImplementation_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ImplicitExternalInterfaceMethodImplementation_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string externalSource = """
                                      public interface IValidator {
                                          bool Validate();
                                      }
                                      """;

        const string source = """
                              namespace MyApp {
                                  public class Validator : IValidator {
                                      public bool Validate() => true;
                                  }
                              }
                              """;

        var externalRef = AnalyzerTestRunner.CompileToReference(externalSource);
        var analyzer = new BooleanMethodNamingAnalyzer();
        var options = new AnalysisOptions
{
    AdditionalReferences = [externalRef]
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS063")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_LocalInterfaceMethodImplementation_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LocalInterfaceMethodImplementation_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface IValidator {
                                      bool Validate();
                                  }
                                  public class Validator : IValidator {
                                      public bool Validate() => true;
                                  }
                              }
                              """;

        var analyzer = new BooleanMethodNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS063")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_NonBoolMethod_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NonBoolMethod_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public string GetName() => "foo";
                                  }
                              }
                              """;

        var analyzer = new BooleanMethodNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS063")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_NullableBoolLocalFunctionWithoutPrefix_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NullableBoolLocalFunctionWithoutPrefix_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Execute() {
                                          bool? Check() => null;
                                          _ = Check();
                                      }
                                  }
                              }
                              """;

        var analyzer = new BooleanMethodNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS063")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_NullableBoolMethodWithCanPrefix_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NullableBoolMethodWithCanPrefix_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool? CanCheck() => null;
                                  }
                              }
                              """;

        var analyzer = new BooleanMethodNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS063")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_NullableBoolMethodWithoutPrefix_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NullableBoolMethodWithoutPrefix_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool? Check() => null;
                                  }
                              }
                              """;

        var analyzer = new BooleanMethodNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS063")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_PrivateBoolMethodWithoutPrefix_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PrivateBoolMethodWithoutPrefix_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private bool Validate() => true;
                                  }
                              }
                              """;

        var analyzer = new BooleanMethodNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS063")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ProtectedBoolMethodWithoutPrefix_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ProtectedBoolMethodWithoutPrefix_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      protected bool Validate() => true;
                                  }
                              }
                              """;

        var analyzer = new BooleanMethodNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS063")).IsTrue();
    }
}
