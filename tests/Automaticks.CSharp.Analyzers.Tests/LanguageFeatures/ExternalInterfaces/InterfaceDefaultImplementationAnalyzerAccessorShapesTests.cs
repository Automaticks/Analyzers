using Automaticks.CSharp.LanguageFeatures;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures.ExternalInterfaces;

/// <summary>
///     Tests InterfaceDefaultImplementationAnalyzer against members that carry no accessor list.
/// </summary>
public class InterfaceDefaultImplementationAnalyzerAccessorShapesTests
{
    /// <summary>
    ///     Tests that members left without an accessor list by a partial edit are not reported.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MembersWithoutAccessorList_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public interface IOther {
                                      event EventHandler Changed;
                                      int this[int index] { get; }
                                      int Size { get; }
                                  }
                                  public interface IShape {
                                      event EventHandler IOther.Changed;
                                      int IOther.this[int index];
                                      int IOther.Size;
                                  }
                              }
                              """;

        var analyzer = new InterfaceDefaultImplementationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.InterfaceDefaultImplementation)).IsFalse();
    }

    /// <summary>
    ///     Tests that static is found when an accessibility modifier precedes it.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StaticPropertyAfterAccessibility_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface IShape {
                                      public static int Size { get; set; }
                                  }
                              }
                              """;

        var analyzer = new InterfaceDefaultImplementationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.InterfaceDefaultImplementation)).IsTrue();
    }
}
