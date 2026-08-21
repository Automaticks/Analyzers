using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Naming;

/// <summary>
///     Tests for StaticMethodInNonStaticClassAnalyzer.
/// </summary>
public class StaticMethodInNonStaticClassAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_ExtensionMethodInNonStaticClass_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExtensionMethodInNonStaticClass_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class FooExtensions {
                                      public static void Bar(this object obj) {}
                                  }
                              }
                              """;

        var analyzer = new StaticMethodInNonStaticClassAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS011")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExtensionMethodInStaticClass_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExtensionMethodInStaticClass_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public static class FooExtensions {
                                      public static void Bar(this object obj) {}
                                  }
                              }
                              """;

        var analyzer = new StaticMethodInNonStaticClassAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS011")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_InstanceMethodInNonStaticClass_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_InstanceMethodInNonStaticClass_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() {}
                                  }
                              }
                              """;

        var analyzer = new StaticMethodInNonStaticClassAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS011")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_StaticMethodInNonStaticClass_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StaticMethodInNonStaticClass_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public static void Bar() {}
                                  }
                              }
                              """;

        var analyzer = new StaticMethodInNonStaticClassAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS011")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_StaticMethodInStaticClass_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StaticMethodInStaticClass_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public static class Foo {
                                      public static void Bar() {}
                                  }
                              }
                              """;

        var analyzer = new StaticMethodInNonStaticClassAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS011")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_StaticMethodInStruct_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StaticMethodInStruct_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public struct Foo {
                                      public static void Bar() {}
                                  }
                              }
                              """;

        var analyzer = new StaticMethodInNonStaticClassAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS011")).IsFalse();
    }
}
