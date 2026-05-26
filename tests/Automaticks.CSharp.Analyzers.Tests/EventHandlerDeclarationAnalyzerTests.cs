using Automaticks.CSharp;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

/// <summary>
///     Tests for EventHandlerDeclarationAnalyzer.
/// </summary>
public class EventHandlerDeclarationAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_ActionEventDeclaration_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ActionEventDeclaration_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public event Action<string> SomethingHappened;
                                  }
                              }
                              """;

        var analyzer = new EventHandlerDeclarationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS007")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_EventHandlerFieldDeclaration_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_EventHandlerFieldDeclaration_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      private EventHandler<EventArgs> _handler;
                                  }
                              }
                              """;

        var analyzer = new EventHandlerDeclarationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS007")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_EventHandlerOfGenericTypeDeclaration_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_EventHandlerOfGenericTypeDeclaration_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public event EventHandler<EventArgs> SomethingHappened;
                                  }
                              }
                              """;

        var analyzer = new EventHandlerDeclarationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS007")).IsTrue();
    }
}
