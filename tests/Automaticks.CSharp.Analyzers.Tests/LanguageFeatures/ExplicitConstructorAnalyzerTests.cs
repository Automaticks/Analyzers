using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures;

/// <summary>
///     Tests for ExplicitConstructorAnalyzer.
/// </summary>
public class ExplicitConstructorAnalyzerTests
{

    /// <summary>
    ///     Tests that Analyze_ClassWithExplicitConstructor_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ClassWithExplicitConstructor_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Point {
                                      public Point(int x, int y) { }
                                  }
                              }
                              """;

        var analyzer = new ExplicitConstructorAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS037")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ClassWithNoConstructor_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ClassWithNoConstructor_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class SimpleService { }
                              }
                              """;

        var analyzer = new ExplicitConstructorAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS037")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ClassWithPrimaryConstructor_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ClassWithPrimaryConstructor_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Point(int x, int y) { }
                              }
                              """;

        var analyzer = new ExplicitConstructorAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS037")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ClassWithPrimaryConstructorAndBody_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ClassWithPrimaryConstructorAndBody_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Service(string name) {
                                      public string Name { get; } = name;
                                  }
                              }
                              """;

        var analyzer = new ExplicitConstructorAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS037")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_MultiplePrimaryConstructorTypes_ReportsOneDiagnosticEach.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MultiplePrimaryConstructorTypes_ReportsOneDiagnosticEach(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo(int x) { }
                                  public struct Bar(int y) { }
                              }
                              """;

        var analyzer = new ExplicitConstructorAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.CountId(diagnostics, "ATXCS037")).IsEqualTo(2);
    }

    /// <summary>
    ///     Tests that Analyze_RecordStructWithPrimaryConstructor_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_RecordStructWithPrimaryConstructor_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public record struct Coordinate(double Latitude, double Longitude);
                              }
                              """;

        var analyzer = new ExplicitConstructorAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS037")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_RecordWithNoParameterList_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_RecordWithNoParameterList_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public record Config {
                                      public int Timeout { get; init; }
                                  }
                              }
                              """;

        var analyzer = new ExplicitConstructorAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS037")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_RecordWithPrimaryConstructor_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_RecordWithPrimaryConstructor_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public record Person(string Name, int Age);
                              }
                              """;

        var analyzer = new ExplicitConstructorAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS037")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_StaticClass_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StaticClass_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public static class MathHelper {
                                      public static int Add(int a, int b)
                                      {
                                          return a + b;
                                      }
                                  }
                              }
                              """;

        var analyzer = new ExplicitConstructorAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS037")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_StructWithExplicitConstructor_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StructWithExplicitConstructor_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public struct Vector {
                                      public Vector(float x, float y) { }
                                  }
                              }
                              """;

        var analyzer = new ExplicitConstructorAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS037")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_StructWithPrimaryConstructor_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StructWithPrimaryConstructor_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public struct Vector(float x, float y) { }
                              }
                              """;

        var analyzer = new ExplicitConstructorAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS037")).IsTrue();
    }
}
