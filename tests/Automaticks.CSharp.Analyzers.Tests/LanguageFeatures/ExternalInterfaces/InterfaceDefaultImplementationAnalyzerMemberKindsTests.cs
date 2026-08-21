using Automaticks.CSharp.LanguageFeatures;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures.ExternalInterfaces;

/// <summary>
///     Tests that InterfaceDefaultImplementationAnalyzer inspects every interface member kind.
/// </summary>
public class InterfaceDefaultImplementationAnalyzerMemberKindsTests
{
    /// <summary>
    ///     Tests that Analyze_AbstractMembersWithoutBodies_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AbstractMembersWithoutBodies_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public interface IShape<T> where T : IShape<T> {
                                      event EventHandler Changed;
                                      int this[int index] { get; }
                                      int Size { get; }
                                      void Draw();
                                      static abstract T operator +(T left, T right);
                                      static abstract explicit operator int(T value);
                                  }
                              }
                              """;

        var analyzer = new InterfaceDefaultImplementationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.InterfaceDefaultImplementation)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_EventWithAccessorBody_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_EventWithAccessorBody_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public interface IShape {
                                      event EventHandler Changed { add { } remove { } }
                                  }
                              }
                              """;

        var analyzer = new InterfaceDefaultImplementationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.InterfaceDefaultImplementation)).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_EventWithoutAccessorBody_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_EventWithoutAccessorBody_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public interface IShape {
                                      event EventHandler Changed { add; remove; }
                                  }
                              }
                              """;

        var analyzer = new InterfaceDefaultImplementationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.InterfaceDefaultImplementation)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExpressionBodiedProperty_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExpressionBodiedProperty_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface IShape {
                                      int Size => 1;
                                  }
                              }
                              """;

        var analyzer = new InterfaceDefaultImplementationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.InterfaceDefaultImplementation)).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_IndexerWithExpressionBody_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_IndexerWithExpressionBody_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface IShape {
                                      int this[int index] => index;
                                  }
                              }
                              """;

        var analyzer = new InterfaceDefaultImplementationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.InterfaceDefaultImplementation)).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_OperatorsWithBodies_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_OperatorsWithBodies_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface IShape {
                                      static IShape operator +(IShape left, IShape right) { return left; }
                                      static explicit operator int(IShape value) { return 0; }
                                  }
                              }
                              """;

        var analyzer = new InterfaceDefaultImplementationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.InterfaceDefaultImplementation)).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_StaticEventField_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StaticEventField_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public interface IShape {
                                      static event EventHandler Raised;
                                  }
                              }
                              """;

        var analyzer = new InterfaceDefaultImplementationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.InterfaceDefaultImplementation)).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_StaticPropertyWithBody_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StaticPropertyWithBody_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface IShape {
                                      static int Origin { get { return 0; } }
                                  }
                              }
                              """;

        var analyzer = new InterfaceDefaultImplementationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.InterfaceDefaultImplementation)).IsTrue();
    }
}
