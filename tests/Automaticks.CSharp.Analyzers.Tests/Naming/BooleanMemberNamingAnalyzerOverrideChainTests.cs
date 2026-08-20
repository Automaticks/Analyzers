using Automaticks.CSharp.Naming;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Naming;

/// <summary>
///     Tests how BooleanMemberNamingAnalyzer walks a property override chain.
/// </summary>
public class BooleanMemberNamingAnalyzerOverrideChainTests
{
    /// <summary>
    ///     Tests that an override reaching a compiled base property is left alone.
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
    ///     Tests that an override whose whole chain is declared here is still reported.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_OverrideOfSourceProperty_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Shape {
                                      public virtual bool CanFly { get; set; }
                                  }
                                  public class Bird : Shape {
                                      public override bool CanFly { get; set; }
                                  }
                              }
                              """;

        var analyzer = new BooleanMemberNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.BooleanMemberNaming)).IsTrue();
    }
}
