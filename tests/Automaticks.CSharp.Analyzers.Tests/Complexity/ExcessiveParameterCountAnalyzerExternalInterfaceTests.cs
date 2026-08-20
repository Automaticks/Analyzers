using Automaticks.CSharp.Complexity;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Complexity;

/// <summary>
///     Tests that ExcessiveParameterCountAnalyzer exempts members bound to metadata interfaces.
/// </summary>
public class ExcessiveParameterCountAnalyzerExternalInterfaceTests
{
    /// <summary>
    ///     Tests that Analyze_ExplicitMetadataInterfaceImplementation_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExplicitMetadataInterfaceImplementation_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Handle : IDisposable {
                                      void IDisposable.Dispose() { }
                                  }
                              }
                              """;

        var analyzer = new ExcessiveParameterCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.ExcessiveParameterCount)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ImplicitMetadataInterfaceMethod_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ImplicitMetadataInterfaceMethod_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections;
                              namespace MyApp {
                                  public class Cursor : IEnumerator {
                                      public object Current { get { return null; } }
                                      public bool MoveNext() { return false; }
                                      public void Reset() { }
                                  }
                              }
                              """;

        var analyzer = new ExcessiveParameterCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.ExcessiveParameterCount)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ImplicitMetadataInterfaceProperty_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ImplicitMetadataInterfaceProperty_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Bag : IReadOnlyCollection<int> {
                                      public int Count { get { return 0; } }
                                      public IEnumerator<int> GetEnumerator() { yield break; }
                                      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { yield break; }
                                  }
                              }
                              """;

        var analyzer = new ExcessiveParameterCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.ExcessiveParameterCount)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ImplicitSourceInterfaceImplementation_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ImplicitSourceInterfaceImplementation_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface ILocal { void Work(); int Size { get; } }
                                  public class Worker : ILocal {
                                      public int Size { get { return 0; } }
                                      public void Work() { }
                                  }
                              }
                              """;

        var analyzer = new ExcessiveParameterCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.ExcessiveParameterCount)).IsFalse();
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

        var analyzer = new ExcessiveParameterCountAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.ExcessiveParameterCount)).IsFalse();
    }
}
