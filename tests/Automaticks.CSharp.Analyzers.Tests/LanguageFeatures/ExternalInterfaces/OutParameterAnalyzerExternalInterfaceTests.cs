using Automaticks.CSharp.LanguageFeatures;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures.ExternalInterfaces;

/// <summary>
///     Tests that OutParameterAnalyzer exempts members bound to interfaces that come from metadata.
/// </summary>
public class OutParameterAnalyzerExternalInterfaceTests
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

        var analyzer = new OutParameterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.OutParameterPosition)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ImplicitMetadataInterfaceImplementation_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ImplicitMetadataInterfaceImplementation_ReportsNoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new OutParameterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.OutParameterCount)).IsFalse();
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
                                  public interface ILocal { void Work(); }
                                  public class Worker : ILocal {
                                      public void Work() { }
                                  }
                              }
                              """;

        var analyzer = new OutParameterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.OutParameterCount)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_LocalFunctionWithNonOutModifier_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LocalFunctionWithNonOutModifier_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Bar() {
                                          int Total(params int[] values) {
                                              var sum = 0;
                                              foreach (var value in values) { sum += value; }
                                              return sum;
                                          }
                                          return Total(1, 2);
                                      }
                                  }
                              }
                              """;

        var analyzer = new OutParameterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.OutParameterCount)).IsFalse();
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

        var analyzer = new OutParameterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.OutParameterCount)).IsFalse();
    }
}
