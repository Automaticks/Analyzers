using Automaticks.CSharp.LanguageFeatures;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures.ExternalInterfaces;

/// <summary>
///     Tests the interface and override exemptions of ProviderFactoryPropertyAnalyzer.
/// </summary>
public class ProviderFactoryPropertyAnalyzerExemptionTests
{
    /// <summary>
    ///     Tests that Analyze_MetadataInterfaceProperty_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MetadataInterfaceProperty_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections;
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class DataProvider : IReadOnlyCollection<int> {
                                      public int Count { get { return 0; } }
                                      public IEnumerator<int> GetEnumerator() { yield break; }
                                      IEnumerator IEnumerable.GetEnumerator() { yield break; }
                                  }
                              }
                              """;

        var analyzer = new ProviderFactoryPropertyAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.ProviderFactoryProperty)).IsFalse();
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
                                  public class WidgetProvider : System.ComponentModel.Component {
                                      protected override bool CanRaiseEvents { get { return true; } }
                                  }
                              }
                              """;

        var analyzer = new ProviderFactoryPropertyAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.ProviderFactoryProperty)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_OverrideOfSourceProperty_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_OverrideOfSourceProperty_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public abstract class BaseProvider {
                                      public abstract int Size { get; }
                                  }
                                  public class DerivedProvider : BaseProvider {
                                      public override int Size { get { return 1; } }
                                  }
                              }
                              """;

        var analyzer = new ProviderFactoryPropertyAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.ProviderFactoryProperty)).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_SourceInterfaceProperty_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SourceInterfaceProperty_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface ILocalProvider { int Size { get; } }
                                  public class WorkerProvider : ILocalProvider {
                                      public int Size { get { return 0; } }
                                  }
                              }
                              """;

        var analyzer = new ProviderFactoryPropertyAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.ProviderFactoryProperty)).IsTrue();
    }
}
