using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class RemarksXmlDocAnalyzerTests
{

    [Test]
    public async Task Analyze_RemarksElementOnClass_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///     A class.
                                  /// </summary>
                                  /// <remarks>Extra notes.</remarks>
                                  public class Foo {}
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new RemarksXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS038")).IsTrue();
    }

    [Test]
    public async Task Analyze_RemarksElementOnMethod_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      /// <summary>
                                      ///     Does something.
                                      /// </summary>
                                      /// <remarks>Additional detail.</remarks>
                                      public void Bar() {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new RemarksXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS038")).IsTrue();
    }

    [Test]
    public async Task Analyze_SelfClosingRemarks_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///     A class.
                                  /// </summary>
                                  /// <remarks/>
                                  public class Foo {}
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new RemarksXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS038")).IsTrue();
    }

    [Test]
    public async Task Analyze_MultipleRemarksElements_ReportsOneDiagnosticPerElement()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      /// <summary>
                                      ///     Foo.
                                      /// </summary>
                                      /// <remarks>First.</remarks>
                                      /// <remarks>Second.</remarks>
                                      public void Bar() {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new RemarksXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Count(d => d.Id == "ATXCS038")).IsEqualTo(2);
    }

    [Test]
    public async Task Analyze_SummaryOnly_NoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  /// <summary>
                                  ///     A class.
                                  /// </summary>
                                  public class Foo {}
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new RemarksXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS038")).IsFalse();
    }

    [Test]
    public async Task Analyze_NoDocComment_NoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {}
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new RemarksXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS038")).IsFalse();
    }

    [Test]
    public async Task Analyze_SummaryAndParamOnly_NoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      /// <summary>
                                      ///     Does something.
                                      /// </summary>
                                      /// <param name="value">The value.</param>
                                      public void Bar(int value) {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new RemarksXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS038")).IsFalse();
    }
}
