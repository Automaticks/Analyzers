using Automaticks.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Diagnostics.CodeAnalysis.Analyzers.Tests;

/// <summary>
///     Tests for SuppressMessageAnalyzer.
/// </summary>
public class SuppressMessageAnalyzerTests
{

    /// <summary>
    ///     Tests that Analyze_CleanClass_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CleanClass_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() { }
                                  }
                              }
                              """;

        var analyzer = new SuppressMessageAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXDC056")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_MultipleSuppressMessages_ReportsMultipleDiagnostics.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MultipleSuppressMessages_ReportsMultipleDiagnostics(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Diagnostics.CodeAnalysis;
                              namespace MyApp {
                                  public class Foo {
                                      [SuppressMessage("Architecture", "ATXCS011")]
                                      [SuppressMessage("Architecture", "ATXCS021")]
                                      private static void Helper() { }
                                  }
                              }
                              """;

        var analyzer = new SuppressMessageAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.CountId(diagnostics, "ATXDC056")).IsEqualTo(2);
    }

    /// <summary>
    ///     Tests that Analyze_QualifiedSuppressMessage_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_QualifiedSuppressMessage_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      [System.Diagnostics.CodeAnalysis.SuppressMessage("Architecture", "ATXCS011")]
                                      private static void Helper() { }
                                  }
                              }
                              """;

        var analyzer = new SuppressMessageAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXDC056")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_SuppressMessageAttributeFullName_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SuppressMessageAttributeFullName_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Diagnostics.CodeAnalysis;
                              namespace MyApp {
                                  public class Foo {
                                      [SuppressMessageAttribute("Architecture", "ATXCS011")]
                                      private static void Helper() { }
                                  }
                              }
                              """;

        var analyzer = new SuppressMessageAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXDC056")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_SuppressMessageOnClass_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SuppressMessageOnClass_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Diagnostics.CodeAnalysis;
                              namespace MyApp {
                                  [SuppressMessage("Architecture", "ATXCS017")]
                                  public class Foo { }
                              }
                              """;

        var analyzer = new SuppressMessageAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXDC056")).IsTrue();
    }
    /// <summary>
    ///     Tests that Analyze_SuppressMessageOnMethod_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SuppressMessageOnMethod_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Diagnostics.CodeAnalysis;
                              namespace MyApp {
                                  public class Foo {
                                      [SuppressMessage("Architecture", "ATXCS011")]
                                      private static void Helper() { }
                                  }
                              }
                              """;

        var analyzer = new SuppressMessageAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXDC056")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_SuppressMessageWithJustification_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SuppressMessageWithJustification_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Diagnostics.CodeAnalysis;
                              namespace MyApp {
                                  public class Foo {
                                      [SuppressMessage("Architecture", "ATXRF030", Justification = "DI registration code.")]
                                      public static void Register() { }
                                  }
                              }
                              """;

        var analyzer = new SuppressMessageAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXDC056")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_UnrelatedAttribute_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_UnrelatedAttribute_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  [Obsolete("Use NewFoo instead.")]
                                  public class Foo { }
                              }
                              """;

        var analyzer = new SuppressMessageAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXDC056")).IsFalse();
    }
}
