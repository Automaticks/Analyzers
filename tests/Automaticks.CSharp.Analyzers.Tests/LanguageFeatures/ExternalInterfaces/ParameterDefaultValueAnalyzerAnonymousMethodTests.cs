using Automaticks.CSharp.LanguageFeatures;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures.ExternalInterfaces;

/// <summary>
///     Tests ParameterDefaultValueAnalyzer against anonymous method parameter lists.
/// </summary>
public class ParameterDefaultValueAnalyzerAnonymousMethodTests
{
    /// <summary>
    ///     Tests that a default written on an anonymous method parameter is reported.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AnonymousMethodWithDefault_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() {
                                          Action<int> handler = delegate (int size = 1) { };
                                      }
                                  }
                              }
                              """;

        var analyzer = new ParameterDefaultValueAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.ParameterDefaultValue)).IsTrue();
    }

    /// <summary>
    ///     Tests that an anonymous method without defaults is left alone.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AnonymousMethodWithoutDefault_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() {
                                          Action<int> handler = delegate (int size) { };
                                      }
                                  }
                              }
                              """;

        var analyzer = new ParameterDefaultValueAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.ParameterDefaultValue)).IsFalse();
    }
}
