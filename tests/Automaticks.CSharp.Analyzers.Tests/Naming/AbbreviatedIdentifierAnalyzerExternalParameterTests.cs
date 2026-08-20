using Automaticks.CSharp.Naming;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Naming;

/// <summary>
///     Tests that AbbreviatedIdentifierAnalyzer leaves parameters of compiled overrides alone.
/// </summary>
public class AbbreviatedIdentifierAnalyzerExternalParameterTests
{
    /// <summary>
    ///     Tests that a parameter inherited from a compiled base is not reported.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ParameterOfExternalOverride_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string externalSource = """
                                      namespace External {
                                          public class Base {
                                              public virtual void Run(int vm) { }
                                          }
                                      }
                                      """;
        const string source = """
                              namespace MyApp {
                                  public class Runner : External.Base {
                                      public override void Run(int vm) { }
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
    ///     Tests that a parameter of an override declared here is still reported.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ParameterOfSourceOverride_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Base {
                                      public virtual void Run(int vm) { }
                                  }
                                  public class Runner : Base {
                                      public override void Run(int vm) { }
                                  }
                              }
                              """;

        var analyzer = new AbbreviatedIdentifierAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS017")).IsTrue();
    }
}
