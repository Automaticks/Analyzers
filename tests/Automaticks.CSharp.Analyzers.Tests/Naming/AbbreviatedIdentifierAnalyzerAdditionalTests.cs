using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Naming;

/// <summary>
///     Additional tests for AbbreviatedIdentifierAnalyzer.
/// </summary>
public class AbbreviatedIdentifierAnalyzerAdditionalTests
{
    /// <summary>
    ///     Tests that Analyze_AcronymBeforeWord_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AcronymBeforeWord_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class APIClient { }
                              }
                              """;

        var analyzer = new AbbreviatedIdentifierAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS017")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExplicitExternalInterfaceMethod_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExplicitExternalInterfaceMethod_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string externalSource = """
                                      public interface IFoo {
                                          void GetVm();
                                      }
                                      """;

        const string source = """
                              namespace MyApp {
                                  public class Foo : IFoo {
                                      void IFoo.GetVm() { }
                                  }
                              }
                              """;

        var externalRef = AnalyzerTestRunner.CompileToReference(externalSource);
        var analyzer = new AbbreviatedIdentifierAnalyzer();
        var options = new AnalysisOptions
{
    AdditionalReferences = [externalRef]
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS017")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExplicitLocalInterfaceMethod_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExplicitLocalInterfaceMethod_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo {
                                      void GetVm();
                                  }
                                  public class Foo : IFoo {
                                      void IFoo.GetVm() { }
                                  }
                              }
                              """;

        var analyzer = new AbbreviatedIdentifierAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS017")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ExternalMethodOverride_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExternalMethodOverride_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string externalSource = """
                                      public abstract class Base {
                                          public abstract void GetVm();
                                      }
                                      """;

        const string source = """
                              namespace MyApp {
                                  public class Derived : Base {
                                      public override void GetVm() { }
                                  }
                              }
                              """;

        var externalRef = AnalyzerTestRunner.CompileToReference(externalSource);
        var analyzer = new AbbreviatedIdentifierAnalyzer();
        var options = new AnalysisOptions
{
    AdditionalReferences = [externalRef]
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS017")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_GenericTypeParameterSegment_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_GenericTypeParameterSegment_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class FactoryOfT { }
                              }
                              """;

        var analyzer = new AbbreviatedIdentifierAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS017")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_IndexerProperty_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_IndexerProperty_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public string this[int index] => string.Empty;
                                  }
                              }
                              """;

        var analyzer = new AbbreviatedIdentifierAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS017")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_LocalOverrideMethodWithAbbreviatedName_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LocalOverrideMethodWithAbbreviatedName_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public abstract class Base {
                                      public abstract void GetVm();
                                  }
                                  public class Derived : Base {
                                      public override void GetVm() { }
                                  }
                              }
                              """;

        var analyzer = new AbbreviatedIdentifierAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS017")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_LocalOverridePropertyWithAbbreviatedName_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LocalOverridePropertyWithAbbreviatedName_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public abstract class Base {
                                      public abstract string Vm { get; }
                                  }
                                  public class Derived : Base {
                                      public override string Vm { get; } = string.Empty;
                                  }
                              }
                              """;

        var analyzer = new AbbreviatedIdentifierAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS017")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_NSubstituteSegment_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NSubstituteSegment_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class NSubstituteFactory { }
                              }
                              """;

        var analyzer = new AbbreviatedIdentifierAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS017")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_UnderscoreOnlyVariable_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_UnderscoreOnlyVariable_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() {
                                          var _ = 0;
                                      }
                                  }
                              }
                              """;

        var analyzer = new AbbreviatedIdentifierAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS017")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_XmlSegment_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_XmlSegment_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class XmlReaderFactory { }
                              }
                              """;

        var analyzer = new AbbreviatedIdentifierAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS017")).IsFalse();
    }
}
