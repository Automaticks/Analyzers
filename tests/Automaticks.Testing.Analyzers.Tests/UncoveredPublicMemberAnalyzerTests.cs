using Automaticks.Testing.Analyzers.Tests.Stubs;
using Automaticks.Testing.Coverage;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Testing.Analyzers.Tests;

/// <summary>
///     Tests for UncoveredPublicMemberAnalyzer.
/// </summary>
public class UncoveredPublicMemberAnalyzerTests
{
    private const string Report = """
                                  <?xml version="1.0" encoding="utf-8"?>
                                  <coverage version="1.9">
                                    <packages>
                                      <package name="MyApp">
                                        <classes>
                                          <class name="MyApp.Foo" filename="MyApp/Foo.cs">
                                            <methods>
                                              <method name="Covered" signature="()">
                                                <lines><line number="3" hits="4" /></lines>
                                              </method>
                                              <method name="Uncovered" signature="()">
                                                <lines><line number="4" hits="0" /></lines>
                                              </method>
                                            </methods>
                                            <lines>
                                              <line number="3" hits="4" />
                                              <line number="4" hits="0" />
                                            </lines>
                                          </class>
                                        </classes>
                                      </package>
                                    </packages>
                                  </coverage>
                                  """;
    private const string Source = """
                                  namespace MyApp {
                                      public class Foo {
                                          public int Covered() { return 1; }
                                          public int Uncovered() { return 2; }
                                      }
                                  }
                                  """;

    /// <summary>
    ///     Tests that Analyze_MethodWithCoverage_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodWithCoverage_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        var diagnostics = await AnalyzeWithReportAsync(Report, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(
            diagnostics,
            "ATXTST012",
            "'Covered'")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_MethodWithoutCoverage_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodWithoutCoverage_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        var diagnostics = await AnalyzeWithReportAsync(Report, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(
            diagnostics,
            "ATXTST012",
            "Uncovered")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ReportForDifferentFile_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ReportForDifferentFile_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string otherReport = """
                                   <?xml version="1.0" encoding="utf-8"?>
                                   <coverage version="1.9">
                                     <packages><package name="Other"><classes>
                                       <class name="Other.Bar" filename="Other/Bar.cs">
                                         <methods><method name="Uncovered" signature="()">
                                           <lines><line number="4" hits="0" /></lines>
                                         </method></methods>
                                       </class>
                                     </classes></package></packages>
                                   </coverage>
                                   """;

        var diagnostics = await AnalyzeWithReportAsync(otherReport, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST012")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_WithoutReport_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_WithoutReport_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        var analyzer = new UncoveredPublicMemberAnalyzer();
        var options = new AnalysisOptions
        {
            FilePath = "C:/repo/MyApp/Foo.cs",
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, Source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST012")).IsFalse();
    }

    private async Task<ImmutableArray<Diagnostic>> AnalyzeWithReportAsync(
        string reportXml,
        CancellationToken cancellationToken)
    {
        var analyzer = new UncoveredPublicMemberAnalyzer();
        var additionalText = new TestAdditionalText("C:/repo/artifacts/coverage.cobertura.xml", reportXml);
        var additionalFiles = new List<AdditionalText>
        {
            additionalText,
        };
        var options = new AnalysisOptions
        {
            FilePath = "C:/repo/MyApp/Foo.cs",
            AdditionalFiles = additionalFiles,
        };
        return await AnalyzerTestRunner.AnalyzeAsync(analyzer, Source, options, cancellationToken);
    }
}
