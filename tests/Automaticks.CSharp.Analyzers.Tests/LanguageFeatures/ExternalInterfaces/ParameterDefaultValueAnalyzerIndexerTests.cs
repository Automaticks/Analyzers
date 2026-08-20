using Automaticks.CSharp.LanguageFeatures;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures.ExternalInterfaces;

/// <summary>
///     Tests the indexer and metadata-interface paths of ParameterDefaultValueAnalyzer.
/// </summary>
public class ParameterDefaultValueAnalyzerIndexerTests
{
    /// <summary>
    ///     Tests that Analyze_IndexerOfMetadataInterface_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_IndexerOfMetadataInterface_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections;
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Row : IReadOnlyList<int> {
                                      public int Count { get { return 0; } }
                                      public int this[int index] { get { return 0; } }
                                      public IEnumerator<int> GetEnumerator() { yield break; }
                                      IEnumerator IEnumerable.GetEnumerator() { yield break; }
                                  }
                              }
                              """;

        var analyzer = new ParameterDefaultValueAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.ParameterDefaultValue)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_IndexerWithDefaultValue_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_IndexerWithDefaultValue_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Table {
                                      public int this[int index, int fallback = 0] { get { return fallback; } }
                                  }
                              }
                              """;

        var analyzer = new ParameterDefaultValueAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.ParameterDefaultValue)).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_MetadataInterfaceNotImplemented_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MetadataInterfaceNotImplemented_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Partial : IReadOnlyList<int> {
                                      public void Extra() { }
                                  }
                              }
                              """;

        var analyzer = new ParameterDefaultValueAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.ParameterDefaultValue)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_OverrideOfMetadataIndexer_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_OverrideOfMetadataIndexer_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections;
                              namespace MyApp {
                                  public class Bag : CollectionBase {
                                      public object this[int index] { get { return List[index]; } }
                                  }
                              }
                              """;

        var analyzer = new ParameterDefaultValueAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.ParameterDefaultValue)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_OverrideOfMetadataMethod_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_OverrideOfMetadataMethod_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public override string ToString() { return "foo"; }
                                  }
                              }
                              """;

        var analyzer = new ParameterDefaultValueAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.ParameterDefaultValue)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_SourceInterfaceIndexer_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SourceInterfaceIndexer_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface ILocalIndexed { int this[int index] { get; } }
                                  public class Table : ILocalIndexed {
                                      public int this[int index] { get { return 0; } }
                                  }
                              }
                              """;

        var analyzer = new ParameterDefaultValueAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.ParameterDefaultValue)).IsFalse();
    }
}
