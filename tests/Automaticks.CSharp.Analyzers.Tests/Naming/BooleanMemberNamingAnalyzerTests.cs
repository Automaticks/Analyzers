using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Naming;

/// <summary>
///     Tests for BooleanMemberNamingAnalyzer.
/// </summary>
public class BooleanMemberNamingAnalyzerTests
{

    /// <summary>
    ///     Tests that Analyze_BoolAutoPropertyWithIsPrefix_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BoolAutoPropertyWithIsPrefix_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool IsEnabled { get; set; }
                                  }
                              }
                              """;

        var analyzer = new BooleanMemberNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS062")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_BoolAutoPropertyWithoutPrefix_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BoolAutoPropertyWithoutPrefix_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool Enabled { get; set; }
                                  }
                              }
                              """;

        var analyzer = new BooleanMemberNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS062")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_BoolFieldWithAllowPrefix_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BoolFieldWithAllowPrefix_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private bool allowRetry;
                                  }
                              }
                              """;

        var analyzer = new BooleanMemberNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS062")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_BoolFieldWithAllowPrefixUpperCase_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BoolFieldWithAllowPrefixUpperCase_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private bool AllowRetry;
                                  }
                              }
                              """;

        var analyzer = new BooleanMemberNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS062")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_BoolFieldWithIsPrefix_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BoolFieldWithIsPrefix_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private bool isEnabled;
                                  }
                              }
                              """;

        var analyzer = new BooleanMemberNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS062")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_BoolFieldWithIsPrefixUpperCase_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BoolFieldWithIsPrefixUpperCase_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private bool IsEnabled;
                                  }
                              }
                              """;

        var analyzer = new BooleanMemberNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS062")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_BoolFieldWithoutPrefix_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BoolFieldWithoutPrefix_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private bool enabled;
                                  }
                              }
                              """;

        var analyzer = new BooleanMemberNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS062")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_BoolFieldWithUnderscoreAllowPrefix_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BoolFieldWithUnderscoreAllowPrefix_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private bool _allowRetry;
                                  }
                              }
                              """;

        var analyzer = new BooleanMemberNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS062")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_BoolFieldWithUnderscoreIsPrefix_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BoolFieldWithUnderscoreIsPrefix_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private bool _isEnabled;
                                  }
                              }
                              """;

        var analyzer = new BooleanMemberNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS062")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_BoolFieldWithUnderscoreOnlyPrefix_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BoolFieldWithUnderscoreOnlyPrefix_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private bool _enabled;
                                  }
                              }
                              """;

        var analyzer = new BooleanMemberNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS062")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_BoolFullPropertyWithoutPrefix_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BoolFullPropertyWithoutPrefix_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private bool _enabled;
                                      public bool Enabled { get => _enabled; set => _enabled = value; }
                                  }
                              }
                              """;

        var analyzer = new BooleanMemberNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS062")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ConstBoolFieldWithIsPrefix_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConstBoolFieldWithIsPrefix_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public const bool IsDefaultEnabled = true;
                                  }
                              }
                              """;

        var analyzer = new BooleanMemberNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS062")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ConstBoolFieldWithoutPrefix_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConstBoolFieldWithoutPrefix_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public const bool DefaultEnabled = true;
                                  }
                              }
                              """;

        var analyzer = new BooleanMemberNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS062")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ExplicitExternalInterfacePropertyImplementation_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExplicitExternalInterfacePropertyImplementation_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string externalSource = """
                                      public interface IFoo {
                                          bool Enabled { get; }
                                      }
                                      """;

        const string source = """
                              namespace MyApp {
                                  public class Foo : IFoo {
                                      bool IFoo.Enabled { get; } = true;
                                  }
                              }
                              """;

        var externalRef = AnalyzerTestRunner.CompileToReference(externalSource);
        var analyzer = new BooleanMemberNamingAnalyzer();
        var options = new AnalysisOptions
{
    AdditionalReferences = [externalRef]
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS062")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExternalOverrideProperty_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExternalOverrideProperty_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string externalSource = """
                                      public abstract class Base {
                                          public abstract bool Enabled { get; }
                                      }
                                      """;

        const string source = """
                              namespace MyApp {
                                  public class Derived : Base {
                                      public override bool Enabled { get; } = true;
                                  }
                              }
                              """;

        var externalRef = AnalyzerTestRunner.CompileToReference(externalSource);
        var analyzer = new BooleanMemberNamingAnalyzer();
        var options = new AnalysisOptions
{
    AdditionalReferences = [externalRef]
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS062")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ImplicitExternalInterfacePropertyImplementation_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ImplicitExternalInterfacePropertyImplementation_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string externalSource = """
                                      public interface IFoo {
                                          bool Enabled { get; }
                                      }
                                      """;

        const string source = """
                              namespace MyApp {
                                  public class Foo : IFoo {
                                      public bool Enabled { get; } = true;
                                  }
                              }
                              """;

        var externalRef = AnalyzerTestRunner.CompileToReference(externalSource);
        var analyzer = new BooleanMemberNamingAnalyzer();
        var options = new AnalysisOptions
{
    AdditionalReferences = [externalRef]
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS062")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_LocalExplicitInterfacePropertyImplementation_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LocalExplicitInterfacePropertyImplementation_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo {
                                      bool Enabled { get; }
                                  }
                                  public class Foo : IFoo {
                                      bool IFoo.Enabled { get; } = true;
                                  }
                              }
                              """;

        var analyzer = new BooleanMemberNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS062")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_LocalInterfacePropertyImplementation_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LocalInterfacePropertyImplementation_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo {
                                      bool Enabled { get; }
                                  }
                                  public class Foo : IFoo {
                                      public bool Enabled { get; } = true;
                                  }
                              }
                              """;

        var analyzer = new BooleanMemberNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS062")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_NonBoolField_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NonBoolField_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private int count;
                                  }
                              }
                              """;

        var analyzer = new BooleanMemberNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS062")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_NonBoolProperty_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NonBoolProperty_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public string Name { get; set; } = string.Empty;
                                  }
                              }
                              """;

        var analyzer = new BooleanMemberNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS062")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_NullableBoolFieldWithIsPrefix_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NullableBoolFieldWithIsPrefix_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private bool? isEnabled;
                                  }
                              }
                              """;

        var analyzer = new BooleanMemberNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS062")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_NullableBoolFieldWithoutPrefix_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NullableBoolFieldWithoutPrefix_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private bool? enabled;
                                  }
                              }
                              """;

        var analyzer = new BooleanMemberNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS062")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_NullableBoolPropertyWithoutPrefix_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NullableBoolPropertyWithoutPrefix_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool? Enabled { get; set; }
                                  }
                              }
                              """;

        var analyzer = new BooleanMemberNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS062")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_StaticReadonlyBoolFieldWithAllowPrefix_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StaticReadonlyBoolFieldWithAllowPrefix_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public static readonly bool AllowOverride = false;
                                  }
                              }
                              """;

        var analyzer = new BooleanMemberNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS062")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_StaticReadonlyBoolFieldWithoutPrefix_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StaticReadonlyBoolFieldWithoutPrefix_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public static readonly bool DefaultEnabled = true;
                                  }
                              }
                              """;

        var analyzer = new BooleanMemberNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS062")).IsTrue();
    }
}
