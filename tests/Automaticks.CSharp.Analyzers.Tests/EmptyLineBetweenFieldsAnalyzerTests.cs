using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class EmptyLineBetweenFieldsAnalyzerTests
{

    [Test]
    public async Task Analyze_BlankLineBetweenTwoInstanceFields_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Counter {
                                      private int _a;

                                      private int _b;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new EmptyLineBetweenFieldsAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS039")).IsTrue();
    }

    [Test]
    public async Task Analyze_BlankLineBetweenTwoStaticFields_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Registry {
                                      private static int _count;

                                      private static int _total;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new EmptyLineBetweenFieldsAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS039")).IsTrue();
    }

    [Test]
    public async Task Analyze_BlankLineBetweenFieldAndConst_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Limits {
                                      private int _value;

                                      private const int Max = 100;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new EmptyLineBetweenFieldsAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS039")).IsTrue();
    }

    [Test]
    public async Task Analyze_BlankLineBetweenTwoConsts_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Constants {
                                      private const int Min = 0;

                                      private const int Max = 100;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new EmptyLineBetweenFieldsAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS039")).IsTrue();
    }

    [Test]
    public async Task Analyze_WhitespaceOnlyLineBetweenFields_ReportsDiagnostic()
    {
        const string source = "namespace MyApp {\n    public class Counter {\n        private int _a;\n   \n        private int _b;\n    }\n}";

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new EmptyLineBetweenFieldsAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS039")).IsTrue();
    }

    [Test]
    public async Task Analyze_MultipleBlankLinesBetweenFields_ReportsOneDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Counter {
                                      private int _a;

                                      private int _b;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new EmptyLineBetweenFieldsAnalyzer(), source);

        await Assert.That(diagnostics.Count(d => d.Id == "ATXCS039")).IsEqualTo(1);
    }

    [Test]
    public async Task Analyze_BlankLineBetweenFieldsInStruct_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public struct Point {
                                      public int X;

                                      public int Y;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new EmptyLineBetweenFieldsAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS039")).IsTrue();
    }

    [Test]
    public async Task Analyze_BlankLineBetweenFieldsInRecord_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public record Config {
                                      public int Width;

                                      public int Height;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new EmptyLineBetweenFieldsAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS039")).IsTrue();
    }

    [Test]
    public async Task Analyze_BlankLineBetweenFieldsInInterface_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public interface ISettings {
                                      const int DefaultTimeout = 30;

                                      const int MaxRetries = 3;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new EmptyLineBetweenFieldsAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS039")).IsTrue();
    }

    [Test]
    public async Task Analyze_ThreeFieldsWithBlankLinesBetweenEach_ReportsTwoDiagnostics()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new EmptyLineBetweenFieldsAnalyzer(), source);

        await Assert.That(diagnostics.Count(d => d.Id == "ATXCS039")).IsEqualTo(2);
    }

    [Test]
    public async Task Analyze_FieldsWithNoBlankLineBetweenThem_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Counter {
                                      private int _a;
                                      private int _b;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new EmptyLineBetweenFieldsAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS039")).IsFalse();
    }

    [Test]
    public async Task Analyze_FieldsSeparatedByCommentLine_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Counter {
                                      private int _a;
                                      // this is a comment
                                      private int _b;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new EmptyLineBetweenFieldsAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS039")).IsFalse();
    }

    [Test]
    public async Task Analyze_BlankLineBetweenFieldAndMethod_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Counter {
                                      private int _a;

                                      public void Increment() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new EmptyLineBetweenFieldsAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS039")).IsFalse();
    }

    [Test]
    public async Task Analyze_BlankLineBetweenMethodAndField_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Counter {
                                      public void Increment() { }

                                      private int _a;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new EmptyLineBetweenFieldsAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS039")).IsFalse();
    }

    [Test]
    public async Task Analyze_BlankLineBetweenFieldAndProperty_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Config {
                                      private int _value;

                                      public int Value { get; set; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new EmptyLineBetweenFieldsAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS039")).IsFalse();
    }

    [Test]
    public async Task Analyze_FieldsWithMethodBetweenThem_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new EmptyLineBetweenFieldsAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS039")).IsFalse();
    }

    [Test]
    public async Task Analyze_SingleField_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Counter {
                                      private int _count;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new EmptyLineBetweenFieldsAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS039")).IsFalse();
    }

    [Test]
    public async Task Analyze_EmptyClass_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Empty { }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new EmptyLineBetweenFieldsAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS039")).IsFalse();
    }

    [Test]
    public async Task Analyze_ConstFieldsWithNoBlankLine_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Constants {
                                      private const int Min = 0;
                                      private const int Max = 100;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new EmptyLineBetweenFieldsAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS039")).IsFalse();
    }

    [Test]
    public async Task Analyze_BlankLineBetweenFieldsInRecordStruct_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public record struct Point {
                                      public int X;

                                      public int Y;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new EmptyLineBetweenFieldsAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS039")).IsTrue();
    }

    [Test]
    public async Task Analyze_DocCommentOnSecondFieldNoBlankLine_ReportsNoDiagnostic()
        => await AssertNoDiagnosticForDocCommentOnSecondFieldAsync();

    [Test]
    public async Task Analyze_BlankLineThenDocCommentOnSecondField_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new EmptyLineBetweenFieldsAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS039")).IsFalse();
    }

    [Test]
    public async Task Analyze_NoBlankLineAndSecondFieldHasXmlDoc_ReportsNoDiagnostic()
        => await AssertNoDiagnosticForDocCommentOnSecondFieldAsync();

    private static async Task AssertNoDiagnosticForDocCommentOnSecondFieldAsync()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new EmptyLineBetweenFieldsAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS039")).IsFalse();
    }
}
