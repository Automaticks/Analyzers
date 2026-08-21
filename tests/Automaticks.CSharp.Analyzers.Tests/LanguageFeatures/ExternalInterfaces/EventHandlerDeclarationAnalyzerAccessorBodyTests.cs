using Automaticks.CSharp.LanguageFeatures;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures.ExternalInterfaces;

/// <summary>
///     Tests how EventHandlerDeclarationAnalyzer reads the bodies of event accessors.
/// </summary>
public class EventHandlerDeclarationAnalyzerAccessorBodyTests
{
    /// <summary>
    ///     Tests that statements other than handler calls do not exempt the event.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AccessorWithUnrelatedStatements_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Shape {
                                      private EventHandler _changed;
                                      public event EventHandler Changed {
                                          add {
                                              var flag = 1;
                                              Register<int>();
                                              _changed += value;
                                          }
                                          remove { _changed -= value; }
                                      }
                                      private void Register<T>() { }
                                  }
                              }
                              """;

        var analyzer = new EventHandlerDeclarationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.EventHandlerDeclaration)).IsTrue();
    }
}
