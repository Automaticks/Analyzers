using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class PlainCommentAnalyzerTests
{
    [Test]
    public async Task Analyze_SingleLineComment_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp
                              {
                                  // This is a plain comment
                                  public class Foo {}
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new PlainCommentAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS041")).IsTrue();
    }

    [Test]
    public async Task Analyze_TrailingInlineComment_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                      private const int Value = 42; // inline trailing comment
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new PlainCommentAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS041")).IsTrue();
    }

    [Test]
    public async Task Analyze_BlockComment_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp
                              {
                                  /* This is a block comment */
                                  public class Foo {}
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new PlainCommentAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS041")).IsTrue();
    }

    [Test]
    public async Task Analyze_MultipleCommentsInFile_ReportsOneDiagnosticPerComment()
    {
        const string source = """
                              namespace MyApp
                              {
                                  // First comment
                                  public class Foo
                                  {
                                      // Second comment
                                      public void Bar() {} /* Third comment */
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new PlainCommentAnalyzer(), source);

        await Assert.That(diagnostics.Count(d => d.Id == "ATXCS041")).IsEqualTo(3);
    }

    [Test]
    public async Task Analyze_XmlDocSingleLine_NoDiagnostic()
    {
        const string source = """
                              namespace MyApp
                              {
                                  /// <summary>
                                  ///     An XML doc comment.
                                  /// </summary>
                                  public class Foo {}
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new PlainCommentAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS041")).IsFalse();
    }

    [Test]
    public async Task Analyze_XmlDocMultiLine_NoDiagnostic()
    {
        const string source = """
                              namespace MyApp
                              {
                                  /// <summary>
                                  ///     A multi-line XML doc comment.
                                  /// </summary>
                                  public class Foo {}
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new PlainCommentAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS041")).IsFalse();
    }

    [Test]
    public async Task Analyze_NoComments_NoDiagnostic()
    {
        const string source = """
                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                      public void Bar() {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new PlainCommentAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS041")).IsFalse();
    }
}
