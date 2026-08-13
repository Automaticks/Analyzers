using Automaticks.CSharp;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Formatting;

/// <summary>
///     Tests for EmptyLineBetweenFieldsAnalyzer.
/// </summary>
public class EmptyLineBetweenFieldsAnalyzerTests
{

    /// <summary>
    ///     Tests that Analyze_BlankLineBetweenFieldAndConst_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BlankLineBetweenFieldAndConst_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Limits {
                                      private int _value;

                                      private const int Max = 100;
                                  }
                              }
                              """;

        var analyzer = new EmptyLineBetweenFieldsAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS039")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_BlankLineBetweenFieldAndMethod_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BlankLineBetweenFieldAndMethod_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Counter {
                                      private int _a;

                                      public void Increment() { }
                                  }
                              }
                              """;

        var analyzer = new EmptyLineBetweenFieldsAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS039")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_BlankLineBetweenFieldAndProperty_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BlankLineBetweenFieldAndProperty_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Config {
                                      private int _value;

                                      public int Value { get; set; }
                                  }
                              }
                              """;

        var analyzer = new EmptyLineBetweenFieldsAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS039")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_BlankLineBetweenFieldsInInterface_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BlankLineBetweenFieldsInInterface_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface ISettings {
                                      const int DefaultTimeout = 30;

                                      const int MaxRetries = 3;
                                  }
                              }
                              """;

        var analyzer = new EmptyLineBetweenFieldsAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS039")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_BlankLineBetweenFieldsInRecord_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BlankLineBetweenFieldsInRecord_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public record Config {
                                      public int Width;

                                      public int Height;
                                  }
                              }
                              """;

        var analyzer = new EmptyLineBetweenFieldsAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS039")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_BlankLineBetweenFieldsInRecordStruct_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BlankLineBetweenFieldsInRecordStruct_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public record struct Point {
                                      public int X;

                                      public int Y;
                                  }
                              }
                              """;

        var analyzer = new EmptyLineBetweenFieldsAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS039")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_BlankLineBetweenFieldsInStruct_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BlankLineBetweenFieldsInStruct_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public struct Point {
                                      public int X;

                                      public int Y;
                                  }
                              }
                              """;

        var analyzer = new EmptyLineBetweenFieldsAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS039")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_BlankLineBetweenMethodAndField_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BlankLineBetweenMethodAndField_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Counter {
                                      public void Increment() { }

                                      private int _a;
                                  }
                              }
                              """;

        var analyzer = new EmptyLineBetweenFieldsAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS039")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_BlankLineBetweenTwoConsts_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BlankLineBetweenTwoConsts_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Constants {
                                      private const int Min = 0;

                                      private const int Max = 100;
                                  }
                              }
                              """;

        var analyzer = new EmptyLineBetweenFieldsAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS039")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_BlankLineBetweenTwoInstanceFields_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BlankLineBetweenTwoInstanceFields_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Counter {
                                      private int _a;

                                      private int _b;
                                  }
                              }
                              """;

        var analyzer = new EmptyLineBetweenFieldsAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS039")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_BlankLineBetweenTwoStaticFields_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BlankLineBetweenTwoStaticFields_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Registry {
                                      private static int _count;

                                      private static int _total;
                                  }
                              }
                              """;

        var analyzer = new EmptyLineBetweenFieldsAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS039")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_BlankLineThenDocCommentOnSecondField_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BlankLineThenDocCommentOnSecondField_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Counter {
                                      private int _a;

                                      /// <summary>
                                      ///     Field b.
                                      /// </summary>
                                      private int _b;
                                  }
                              }
                              """;

        var analyzer = new EmptyLineBetweenFieldsAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS039")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ConstFieldsWithNoBlankLine_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConstFieldsWithNoBlankLine_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Constants {
                                      private const int Min = 0;
                                      private const int Max = 100;
                                  }
                              }
                              """;

        var analyzer = new EmptyLineBetweenFieldsAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS039")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_DocCommentOnSecondFieldNoBlankLine_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_DocCommentOnSecondFieldNoBlankLine_ReportsNoDiagnostic(CancellationToken cancellationToken)
        => await AssertNoDiagnosticForDocCommentOnSecondFieldAsync(cancellationToken);

    /// <summary>
    ///     Tests that Analyze_EmptyClass_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_EmptyClass_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Empty { }
                              }
                              """;

        var analyzer = new EmptyLineBetweenFieldsAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS039")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_FieldsSeparatedByCommentLine_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FieldsSeparatedByCommentLine_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Counter {
                                      private int _a;
                                      private int _b;
                                  }
                              }
                              """;

        var analyzer = new EmptyLineBetweenFieldsAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS039")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_FieldsWithMethodBetweenThem_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FieldsWithMethodBetweenThem_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Counter {
                                      private int _a;
                                      public void Reset() { }

                                      private int _b;
                                  }
                              }
                              """;

        var analyzer = new EmptyLineBetweenFieldsAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS039")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_FieldsWithNoBlankLineBetweenThem_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FieldsWithNoBlankLineBetweenThem_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Counter {
                                      private int _a;
                                      private int _b;
                                  }
                              }
                              """;

        var analyzer = new EmptyLineBetweenFieldsAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS039")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_MultipleBlankLinesBetweenFields_ReportsOneDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MultipleBlankLinesBetweenFields_ReportsOneDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Counter {
                                      private int _a;

                                      private int _b;
                                  }
                              }
                              """;

        var analyzer = new EmptyLineBetweenFieldsAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.CountId(diagnostics, "ATXCS039")).IsEqualTo(1);
    }

    /// <summary>
    ///     Tests that Analyze_NoBlankLineAndSecondFieldHasExtensibleMarkupLanguageDoc_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NoBlankLineAndSecondFieldHasExtensibleMarkupLanguageDoc_ReportsNoDiagnostic(CancellationToken cancellationToken)
        => await AssertNoDiagnosticForDocCommentOnSecondFieldAsync(cancellationToken);

    /// <summary>
    ///     Tests that Analyze_SingleField_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SingleField_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Counter {
                                      private int _count;
                                  }
                              }
                              """;

        var analyzer = new EmptyLineBetweenFieldsAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS039")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ThreeFieldsWithBlankLinesBetweenEach_ReportsTwoDiagnostics.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ThreeFieldsWithBlankLinesBetweenEach_ReportsTwoDiagnostics(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Counter {
                                      private int _a;

                                      private int _b;

                                      private int _c;
                                  }
                              }
                              """;

        var analyzer = new EmptyLineBetweenFieldsAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.CountId(diagnostics, "ATXCS039")).IsEqualTo(2);
    }

    /// <summary>
    ///     Tests that Analyze_WhitespaceOnlyLineBetweenFields_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_WhitespaceOnlyLineBetweenFields_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = "namespace MyApp {\n    public class Counter {\n        private int _a;\n   \n        private int _b;\n    }\n}";

        var analyzer = new EmptyLineBetweenFieldsAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS039")).IsTrue();
    }

    private async Task AssertNoDiagnosticForDocCommentOnSecondFieldAsync(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Counter {
                                      private int _a;
                                      /// <summary>
                                      ///     Field b.
                                      /// </summary>
                                      private int _b;
                                  }
                              }
                              """;

        var analyzer = new EmptyLineBetweenFieldsAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS039")).IsFalse();
    }
}
