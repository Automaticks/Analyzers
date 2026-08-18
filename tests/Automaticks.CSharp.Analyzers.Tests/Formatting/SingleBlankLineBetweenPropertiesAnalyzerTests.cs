using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Formatting;

/// <summary>
///     Tests for SingleBlankLineBetweenPropertiesAnalyzer.
/// </summary>
public class SingleBlankLineBetweenPropertiesAnalyzerTests
{

    /// <summary>
    ///     Tests that Analyze_AdjacentPropertiesNoBlankLineInInterface_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AdjacentPropertiesNoBlankLineInInterface_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface IModel {
                                      int Foo { get; }
                                      int Bar { get; }
                                  }
                              }
                              """;

        var analyzer = new SingleBlankLineBetweenPropertiesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS040")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_AdjacentPropertiesNoBlankLineInRecord_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AdjacentPropertiesNoBlankLineInRecord_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public record Config {
                                      public int Width { get; init; }
                                      public int Height { get; init; }
                                  }
                              }
                              """;

        var analyzer = new SingleBlankLineBetweenPropertiesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS040")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_AdjacentPropertiesNoBlankLineInStruct_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AdjacentPropertiesNoBlankLineInStruct_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public struct Point {
                                      public int X { get; set; }
                                      public int Y { get; set; }
                                  }
                              }
                              """;

        var analyzer = new SingleBlankLineBetweenPropertiesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS040")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_AdjacentPropertiesOneBlankLineInRecordStruct_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AdjacentPropertiesOneBlankLineInRecordStruct_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public record struct Size {
                                      public int Width { get; init; }

                                      public int Height { get; init; }
                                  }
                              }
                              """;

        var analyzer = new SingleBlankLineBetweenPropertiesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS040")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_FieldThenPropertyNoBlankLine_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FieldThenPropertyNoBlankLine_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      private int _value;
                                      public int Value { get; set; }
                                  }
                              }
                              """;

        var analyzer = new SingleBlankLineBetweenPropertiesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS040")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_FieldThenPropertyOneBlankLine_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FieldThenPropertyOneBlankLine_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      private int _value;

                                      public int Value { get; set; }
                                  }
                              }
                              """;

        var analyzer = new SingleBlankLineBetweenPropertiesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS040")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_FirstMemberIsProperty_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FirstMemberIsProperty_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      public int Value { get; set; }
                                  }
                              }
                              """;

        var analyzer = new SingleBlankLineBetweenPropertiesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS040")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_IndexerAdjacentToPropertyNoBlankLine_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_IndexerAdjacentToPropertyNoBlankLine_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      public int Value { get; set; }
                                      public int this[int index] { get => index; }
                                  }
                              }
                              """;

        var analyzer = new SingleBlankLineBetweenPropertiesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS040")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_PropertyThenMethodNoBlankLine_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PropertyThenMethodNoBlankLine_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      public int Value { get; set; }
                                      public void Reset() { }
                                  }
                              }
                              """;

        var analyzer = new SingleBlankLineBetweenPropertiesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS040")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_PropertyThenMethodOneBlankLine_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PropertyThenMethodOneBlankLine_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      public int Value { get; set; }

                                      public void Reset() { }
                                  }
                              }
                              """;

        var analyzer = new SingleBlankLineBetweenPropertiesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS040")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_PropertyWithDocCommentAndOneBlankLineBefore_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PropertyWithDocCommentAndOneBlankLineBefore_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      public int Foo { get; set; }

                                      /// <summary>
                                      ///     Gets bar.
                                      /// </summary>
                                      public int Bar { get; set; }
                                  }
                              }
                              """;

        var analyzer = new SingleBlankLineBetweenPropertiesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS040")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_PropertyWithDocCommentNoBlankLineAfterPrecedingProperty_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PropertyWithDocCommentNoBlankLineAfterPrecedingProperty_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      public int Foo { get; set; }
                                      /// <summary>
                                      ///     Gets bar.
                                      /// </summary>
                                      public int Bar { get; set; }
                                  }
                              }
                              """;

        var analyzer = new SingleBlankLineBetweenPropertiesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS040")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_TwoAdjacentFieldsNoProperty_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TwoAdjacentFieldsNoProperty_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      private int _a;
                                      private int _b;
                                  }
                              }
                              """;

        var analyzer = new SingleBlankLineBetweenPropertiesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS040")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_TwoAdjacentMethodsNoProperty_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TwoAdjacentMethodsNoProperty_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      public void Foo() { }
                                      public void Bar() { }
                                  }
                              }
                              """;

        var analyzer = new SingleBlankLineBetweenPropertiesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS040")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_TwoAdjacentPropertiesNoBlankLine_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TwoAdjacentPropertiesNoBlankLine_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      public int Foo { get; set; }
                                      public int Bar { get; set; }
                                  }
                              }
                              """;

        var analyzer = new SingleBlankLineBetweenPropertiesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS040")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_TwoAdjacentPropertiesOneBlankLine_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TwoAdjacentPropertiesOneBlankLine_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      public int Foo { get; set; }

                                      public int Bar { get; set; }
                                  }
                              }
                              """;

        var analyzer = new SingleBlankLineBetweenPropertiesAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS040")).IsFalse();
    }
}
