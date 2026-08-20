using Automaticks.CSharp.LanguageFeatures;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures.ExternalInterfaces;

/// <summary>
///     Tests the delegate shapes and statement forms EventHandlerDeclarationAnalyzer inspects.
/// </summary>
public class EventHandlerDeclarationAnalyzerDelegateShapesTests
{
    /// <summary>
    ///     Tests that Analyze_CustomDelegateEvent_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CustomDelegateEvent_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public delegate void Notify(int value);
                                  public class Foo {
                                      public event Notify Changed;
                                  }
                              }
                              """;

        var analyzer = new EventHandlerDeclarationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.EventHandlerDeclaration)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_GenericEventHandlerEvent_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_GenericEventHandlerEvent_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Payload : EventArgs { }
                                  public class Foo {
                                      public event EventHandler<Payload> Changed;
                                  }
                              }
                              """;

        var analyzer = new EventHandlerDeclarationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.EventHandlerDeclaration)).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_OtherGenericDelegateEvent_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_OtherGenericDelegateEvent_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public event Action<int> Changed;
                                  }
                              }
                              """;

        var analyzer = new EventHandlerDeclarationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.EventHandlerDeclaration)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_UnrelatedStatementsInAccessor_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_UnrelatedStatementsInAccessor_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      private int _count;
                                      private EventHandler _inner;
                                      public event EventHandler Changed {
                                          add { _count = _count + 1; _inner += value; }
                                          remove { _count = _count - 1; _inner -= value; }
                                      }
                                  }
                              }
                              """;

        var analyzer = new EventHandlerDeclarationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.EventHandlerDeclaration)).IsTrue();
    }
}
