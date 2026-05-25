using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class SummaryXmlDocFormatAnalyzerTests
{
    [Test]
    public async Task Analyze_InlineSummaryOnClass_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>The Android entry point for the Inferno client.</summary>
                                  public class Foo {}
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SummaryXmlDocFormatAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS050")).IsTrue();
    }

    [Test]
    public async Task Analyze_InlineSummaryOnMethod_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      /// <summary>Does something.</summary>
                                      public void Bar() {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SummaryXmlDocFormatAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS050")).IsTrue();
    }

    [Test]
    public async Task Analyze_ContentOnNewLineWithNoIndentation_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  /// No indentation here.
                                  /// </summary>
                                  public class Foo {}
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SummaryXmlDocFormatAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS050")).IsTrue();
    }

    [Test]
    public async Task Analyze_ContentOnNewLineWithTwoSpaceIndentation_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///   Two spaces only.
                                  /// </summary>
                                  public class Foo {}
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SummaryXmlDocFormatAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS050")).IsTrue();
    }

    [Test]
    public async Task Analyze_ContentOnNewLineWithThreeSpaceIndentation_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///    Three spaces only.
                                  /// </summary>
                                  public class Foo {}
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SummaryXmlDocFormatAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS050")).IsTrue();
    }

    [Test]
    public async Task Analyze_ContentOnNewLineWithFourSpaceIndentation_NoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///     The Android entry point for the Inferno client.
                                  /// </summary>
                                  public class Foo {}
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SummaryXmlDocFormatAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS050")).IsFalse();
    }

    [Test]
    public async Task Analyze_MultiLineContentAllLinesProperlyIndented_NoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///     First line of the summary.
                                  ///     Second line of the summary.
                                  /// </summary>
                                  public class Foo {}
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SummaryXmlDocFormatAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS050")).IsFalse();
    }

    [Test]
    public async Task Analyze_MultiLineContentOneLineMissingIndentation_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///     First line properly indented.
                                  /// Missing indentation on this line.
                                  /// </summary>
                                  public class Foo {}
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SummaryXmlDocFormatAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS050")).IsTrue();
    }

    [Test]
    public async Task Analyze_NoDocComment_NoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {}
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SummaryXmlDocFormatAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS050")).IsFalse();
    }

    [Test]
    public async Task Analyze_ProperlyFormattedSummaryOnMethod_NoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      /// <summary>
                                      ///     Does something useful.
                                      /// </summary>
                                      public void Bar() {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SummaryXmlDocFormatAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS050")).IsFalse();
    }

    [Test]
    public async Task Analyze_SummaryWithSeeRefProperlyFormatted_NoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///     Works with <see cref="System.String" /> values.
                                  /// </summary>
                                  public class Foo {}
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SummaryXmlDocFormatAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS050")).IsFalse();
    }

    [Test]
    public async Task Analyze_InlineSummaryWithSeeRef_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>Works with <see cref="System.String" /> values.</summary>
                                  public class Foo {}
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SummaryXmlDocFormatAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS050")).IsTrue();
    }
}
