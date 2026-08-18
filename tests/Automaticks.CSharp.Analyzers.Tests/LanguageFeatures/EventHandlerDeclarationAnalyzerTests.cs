using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures;

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
    ///     Tests that Analyze_AvaloniaStyleBlockBodiedAccessorsWithThisQualifier_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AvaloniaStyleBlockBodiedAccessorsWithThisQualifier_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public event EventHandler<EventArgs> Click
                                      {
                                          add { this.AddHandler(value); }
                                          remove { this.RemoveHandler(value); }
                                      }

                                      private void AddHandler(EventHandler<EventArgs> handler) { }
                                      private void RemoveHandler(EventHandler<EventArgs> handler) { }
                                  }
                              }
                              """;

        var analyzer = new EventHandlerDeclarationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS007")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_AvaloniaStyleExpressionBodiedAccessors_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AvaloniaStyleExpressionBodiedAccessors_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public event EventHandler<EventArgs> Click
                                      {
                                          add => AddHandler(value);
                                          remove => RemoveHandler(value);
                                      }

                                      private void AddHandler(EventHandler<EventArgs> handler) { }
                                      private void RemoveHandler(EventHandler<EventArgs> handler) { }
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

    /// <summary>
    ///     Tests that Analyze_EventHandlerPropertyDeclaration_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_EventHandlerPropertyDeclaration_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public EventHandler<EventArgs> Handler { get; set; }
                                  }
                              }
                              """;

        var analyzer = new EventHandlerDeclarationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS007")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_EventWithCustomAccessorsNotCallingAddOrRemoveHandler_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_EventWithCustomAccessorsNotCallingAddOrRemoveHandler_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public event EventHandler<EventArgs> Click
                                      {
                                          add { Console.WriteLine("added"); }
                                          remove { Console.WriteLine("removed"); }
                                      }
                                  }
                              }
                              """;

        var analyzer = new EventHandlerDeclarationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS007")).IsTrue();
    }
}
