using Automaticks.CSharp;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures;

/// <summary>
///     Tests for InlineFieldInitializerAnalyzer.
/// </summary>
public class InlineFieldInitializerAnalyzerTests
{

    /// <summary>
    ///     Tests that Analyze_AutoPropertyWithInitializer_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AutoPropertyWithInitializer_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Config {
                                      public int MaxRetries { get; set; } = 3;
                                  }
                              }
                              """;

        var analyzer = new InlineFieldInitializerAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS036")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_AutoPropertyWithoutInitializer_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AutoPropertyWithoutInitializer_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Config {
                                      public int MaxRetries { get; set; }
                                  }
                              }
                              """;

        var analyzer = new InlineFieldInitializerAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS036")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ConstField_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConstField_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Constants {
                                      private const int MaxValue = 100;
                                  }
                              }
                              """;

        var analyzer = new InlineFieldInitializerAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS036")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_FieldWithoutInitializer_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FieldWithoutInitializer_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Service { }
                                  public class Consumer {
                                      private Service? _service;
                                  }
                              }
                              """;

        var analyzer = new InlineFieldInitializerAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS036")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_FullBodyProperty_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FullBodyProperty_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Config {
                                      private int _retries;
                                      public int MaxRetries
                                      {
                                          get { return _retries; }
                                          set { _retries = value; }
                                      }
                                  }
                              }
                              """;

        var analyzer = new InlineFieldInitializerAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS036")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_InstanceFieldWithNewExpression_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_InstanceFieldWithNewExpression_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Service { }
                                  public class Consumer {
                                      private readonly Service _service = new();
                                  }
                              }
                              """;

        var analyzer = new InlineFieldInitializerAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS036")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_InstanceFieldWithNullLiteral_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_InstanceFieldWithNullLiteral_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Service { }
                                  public class Consumer {
                                      private Service? _service = null;
                                  }
                              }
                              """;

        var analyzer = new InlineFieldInitializerAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS036")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_InstanceFieldWithPrimitiveLiteral_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_InstanceFieldWithPrimitiveLiteral_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Counter {
                                      private int _count = 0;
                                  }
                              }
                              """;

        var analyzer = new InlineFieldInitializerAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS036")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_InstanceFieldWithStaticMethodCall_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_InstanceFieldWithStaticMethodCall_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Repository {
                                      private readonly List<int> _items = new List<int>();
                                  }
                              }
                              """;

        var analyzer = new InlineFieldInitializerAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS036")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_InstanceFieldWithStringLiteral_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_InstanceFieldWithStringLiteral_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Greeter {
                                      private string _name = "default";
                                  }
                              }
                              """;

        var analyzer = new InlineFieldInitializerAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS036")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_MultipleVariablesSomeWithoutInitializers_ReportsOnlyInitialized.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MultipleVariablesSomeWithoutInitializers_ReportsOnlyInitialized(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Counters {
                                      private int _a = 1, _b;
                                  }
                              }
                              """;

        var analyzer = new InlineFieldInitializerAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.CountId(diagnostics, "ATXCS036")).IsEqualTo(1);
    }

    /// <summary>
    ///     Tests that Analyze_MultipleVariablesWithInitializers_ReportsMultipleDiagnostics.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MultipleVariablesWithInitializers_ReportsMultipleDiagnostics(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Counters {
                                      private int _a = 1, _b = 2;
                                  }
                              }
                              """;

        var analyzer = new InlineFieldInitializerAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.CountId(diagnostics, "ATXCS036")).IsEqualTo(2);
    }

    /// <summary>
    ///     Tests that Analyze_PrimaryConstructorFieldCapture_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PrimaryConstructorFieldCapture_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Container(int capacity) {
                                      private int _capacity = capacity;
                                  }
                              }
                              """;

        var analyzer = new InlineFieldInitializerAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS036")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_PrimaryConstructorParameterCapture_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PrimaryConstructorParameterCapture_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Point(int x, int y) {
                                      public int X { get; } = x;
                                      public int Y { get; } = y;
                                  }
                              }
                              """;

        var analyzer = new InlineFieldInitializerAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS036")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_PrimaryConstructorWithNonParamInitializer_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PrimaryConstructorWithNonParamInitializer_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Container(int capacity) {
                                      private int _capacity = capacity;
                                      private readonly List<int> _items = [];
                                  }
                              }
                              """;

        var analyzer = new InlineFieldInitializerAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.CountId(diagnostics, "ATXCS036")).IsEqualTo(1);
    }

    /// <summary>
    ///     Tests that Analyze_RecordFieldWithInitializer_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_RecordFieldWithInitializer_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public record Config {
                                      public int Timeout { get; init; } = 30;
                                  }
                              }
                              """;

        var analyzer = new InlineFieldInitializerAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS036")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_StaticFieldWithInitializer_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StaticFieldWithInitializer_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Registry {
                                      private static int _count = 0;
                                  }
                              }
                              """;

        var analyzer = new InlineFieldInitializerAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS036")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_StaticFieldWithoutInitializer_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StaticFieldWithoutInitializer_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Registry {
                                      private static int _count;
                                  }
                              }
                              """;

        var analyzer = new InlineFieldInitializerAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS036")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_StructFieldWithInitializer_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StructFieldWithInitializer_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public struct Point {
                                      public int X = 0;
                                      public int Y = 0;
                                  }
                              }
                              """;

        var analyzer = new InlineFieldInitializerAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.CountId(diagnostics, "ATXCS036")).IsEqualTo(2);
    }
}
