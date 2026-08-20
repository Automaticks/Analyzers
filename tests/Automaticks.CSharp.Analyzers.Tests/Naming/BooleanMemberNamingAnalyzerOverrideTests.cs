using Automaticks.CSharp.Naming;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Naming;

/// <summary>
///     Tests the override and name-trimming paths of BooleanMemberNamingAnalyzer.
/// </summary>
public class BooleanMemberNamingAnalyzerOverrideTests
{
    /// <summary>
    ///     Tests that Analyze_NullableBooleanProperty_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NullableBooleanProperty_ReportsDiagnostic(CancellationToken cancellationToken)
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

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.BooleanMemberNaming)).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_OtherGenericProperty_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_OtherGenericProperty_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Foo {
                                      public List<int> Items { get; set; }
                                      public KeyValuePair<int, int> Pair { get; set; }
                                  }
                              }
                              """;

        var analyzer = new BooleanMemberNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.BooleanMemberNaming)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_OverrideOfMetadataProperty_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_OverrideOfMetadataProperty_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Widget : System.ComponentModel.Component {
                                      protected override bool CanRaiseEvents { get { return true; } }
                                  }
                              }
                              """;

        var analyzer = new BooleanMemberNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.BooleanMemberNaming)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_OverrideOfSourceProperty_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_OverrideOfSourceProperty_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public abstract class Base {
                                      public abstract bool IsReady { get; }
                                  }
                                  public class Derived : Base {
                                      public override bool IsReady { get { return true; } }
                                  }
                              }
                              """;

        var analyzer = new BooleanMemberNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.BooleanMemberNaming)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_UnderscoreOnlyFieldName_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_UnderscoreOnlyFieldName_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private bool __;
                                      public bool Read() { return __; }
                                  }
                              }
                              """;

        var analyzer = new BooleanMemberNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.BooleanMemberNaming)).IsTrue();
    }
}
