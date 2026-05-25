using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class SingleBlankLineBetweenPropertiesAnalyzerTests
{

    [Test]
    public async Task Analyze_TwoAdjacentPropertiesNoBlankLine_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      public int Foo { get; set; }
                                      public int Bar { get; set; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SingleBlankLineBetweenPropertiesAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS040")).IsTrue();
    }

    [Test]
    public async Task Analyze_FieldThenPropertyNoBlankLine_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      private int _value;
                                      public int Value { get; set; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SingleBlankLineBetweenPropertiesAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS040")).IsTrue();
    }

    [Test]
    public async Task Analyze_PropertyThenMethodNoBlankLine_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      public int Value { get; set; }
                                      public void Reset() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SingleBlankLineBetweenPropertiesAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS040")).IsTrue();
    }

    [Test]
    public async Task Analyze_IndexerAdjacentToPropertyNoBlankLine_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      public int Value { get; set; }
                                      public int this[int index] { get => index; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SingleBlankLineBetweenPropertiesAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS040")).IsTrue();
    }

    [Test]
    public async Task Analyze_AdjacentPropertiesNoBlankLineInStruct_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public struct Point {
                                      public int X { get; set; }
                                      public int Y { get; set; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SingleBlankLineBetweenPropertiesAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS040")).IsTrue();
    }

    [Test]
    public async Task Analyze_AdjacentPropertiesNoBlankLineInInterface_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public interface IModel {
                                      int Foo { get; }
                                      int Bar { get; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SingleBlankLineBetweenPropertiesAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS040")).IsTrue();
    }

    [Test]
    public async Task Analyze_AdjacentPropertiesNoBlankLineInRecord_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public record Config {
                                      public int Width { get; init; }
                                      public int Height { get; init; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SingleBlankLineBetweenPropertiesAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS040")).IsTrue();
    }

    [Test]
    public async Task Analyze_PropertyWithDocCommentNoBlankLineAfterPrecedingProperty_ReportsDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SingleBlankLineBetweenPropertiesAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS040")).IsTrue();
    }

    [Test]
    public async Task Analyze_TwoAdjacentPropertiesOneBlankLine_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      public int Foo { get; set; }

                                      public int Bar { get; set; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SingleBlankLineBetweenPropertiesAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS040")).IsFalse();
    }

    [Test]
    public async Task Analyze_FieldThenPropertyOneBlankLine_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      private int _value;

                                      public int Value { get; set; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SingleBlankLineBetweenPropertiesAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS040")).IsFalse();
    }

    [Test]
    public async Task Analyze_PropertyThenMethodOneBlankLine_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      public int Value { get; set; }

                                      public void Reset() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SingleBlankLineBetweenPropertiesAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS040")).IsFalse();
    }

    [Test]
    public async Task Analyze_TwoAdjacentFieldsNoProperty_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      private int _a;
                                      private int _b;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SingleBlankLineBetweenPropertiesAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS040")).IsFalse();
    }

    [Test]
    public async Task Analyze_TwoAdjacentMethodsNoProperty_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      public void Foo() { }
                                      public void Bar() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SingleBlankLineBetweenPropertiesAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS040")).IsFalse();
    }

    [Test]
    public async Task Analyze_FirstMemberIsProperty_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      public int Value { get; set; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SingleBlankLineBetweenPropertiesAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS040")).IsFalse();
    }

    [Test]
    public async Task Analyze_PropertyWithDocCommentAndOneBlankLineBefore_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SingleBlankLineBetweenPropertiesAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS040")).IsFalse();
    }

    [Test]
    public async Task Analyze_AdjacentPropertiesOneBlankLineInRecordStruct_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public record struct Size {
                                      public int Width { get; init; }

                                      public int Height { get; init; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SingleBlankLineBetweenPropertiesAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS040")).IsFalse();
    }
}
