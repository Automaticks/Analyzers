using Automaticks.CSharp;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Formatting;

/// <summary>
///     Tests for AutoPropertySingleLineAnalyzer.
/// </summary>
public class AutoPropertySingleLineAnalyzerTests
{

    /// <summary>
    ///     Tests that Analyze_AttributeOnSeparateLineWithSingleLineAutoProperty_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AttributeOnSeparateLineWithSingleLineAutoProperty_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;
                              namespace MyApp {
                                  public class Model {
                                      [Required]
                                      public string Name { get; set; }
                                  }
                              }
                              """;

        var analyzer = new AutoPropertySingleLineAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS045")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExpressionBodiedProperty_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExpressionBodiedProperty_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      private int _value = 0;
                                      public int Value => _value;
                                  }
                              }
                              """;

        var analyzer = new AutoPropertySingleLineAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS045")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_MultiLineAutoPropertyInInterface_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MultiLineAutoPropertyInInterface_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface IModel {
                                      int Count
                                      {
                                          get;
                                      }
                                  }
                              }
                              """;

        var analyzer = new AutoPropertySingleLineAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS045")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_MultiLineGetInitAutoProperty_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MultiLineGetInitAutoProperty_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public record Config {
                                      public int Width
                                      {
                                          get;
                                          init;
                                      }
                                  }
                              }
                              """;

        var analyzer = new AutoPropertySingleLineAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS045")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_MultiLineGetOnlyAutoProperty_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MultiLineGetOnlyAutoProperty_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      public string Name
                                      {
                                          get;
                                      }
                                  }
                              }
                              """;

        var analyzer = new AutoPropertySingleLineAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS045")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_MultiLineGetSetAutoProperty_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MultiLineGetSetAutoProperty_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      public string Name
                                      {
                                          get;
                                          set;
                                      }
                                  }
                              }
                              """;

        var analyzer = new AutoPropertySingleLineAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS045")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_MultiLinePropertyWithBlockBodyGetter_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MultiLinePropertyWithBlockBodyGetter_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      private string _name = "";
                                      public string Name
                                      {
                                          get { return _name; }
                                      }
                                  }
                              }
                              """;

        var analyzer = new AutoPropertySingleLineAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS045")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_MultiLinePropertyWithExpressionBodyGetter_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MultiLinePropertyWithExpressionBodyGetter_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      private string _name = "";
                                      public string Name
                                      {
                                          get => _name;
                                      }
                                  }
                              }
                              """;

        var analyzer = new AutoPropertySingleLineAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS045")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_MultiLineSetOnlyAutoProperty_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MultiLineSetOnlyAutoProperty_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      public string Name
                                      {
                                          set;
                                      }
                                  }
                              }
                              """;

        var analyzer = new AutoPropertySingleLineAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS045")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_SingleLineGetInitAutoProperty_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SingleLineGetInitAutoProperty_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public record Config {
                                      public int Width { get; init; }
                                  }
                              }
                              """;

        var analyzer = new AutoPropertySingleLineAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS045")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_SingleLineGetOnlyAutoProperty_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SingleLineGetOnlyAutoProperty_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      public string Name { get; }
                                  }
                              }
                              """;

        var analyzer = new AutoPropertySingleLineAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS045")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_SingleLineGetSetAutoProperty_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SingleLineGetSetAutoProperty_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      public string Name { get; set; }
                                  }
                              }
                              """;

        var analyzer = new AutoPropertySingleLineAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS045")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_SingleLineSetOnlyAutoProperty_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SingleLineSetOnlyAutoProperty_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      public string Name { set; }
                                  }
                              }
                              """;

        var analyzer = new AutoPropertySingleLineAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS045")).IsFalse();
    }
}
