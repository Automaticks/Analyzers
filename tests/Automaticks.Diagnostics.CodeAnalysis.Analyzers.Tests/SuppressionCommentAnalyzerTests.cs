using Automaticks.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.Diagnostics.CodeAnalysis.Analyzers.Tests;

public class SuppressionCommentAnalyzerTests
{
    [Test]
    public async Task Analyze_CleanCode_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp
                              {
                                  // This method is intentionally empty
                                  public class Foo
                                  {
                                      public void Bar() {}
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SuppressionCommentAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id is "ATXDC018" or "ATXDC019")).IsFalse();
    }

    [Test]
    public async Task Analyze_PragmaWarningDisable_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp
                              {
                              #pragma warning disable CS0168
                                  public class Foo {}
                              #pragma warning restore CS0168
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SuppressionCommentAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXDC018")).IsTrue();
    }

    [Test]
    public async Task Analyze_PragmaWarningRestore_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp
                              {
                              #pragma warning restore CS0168
                                  public class Foo {}
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SuppressionCommentAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXDC018")).IsFalse();
    }

    [Test]
    public async Task Analyze_RegularComment_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp
                              {
                                  // This is a regular comment about ReSharper features
                                  public class Foo {}
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SuppressionCommentAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id is "ATXDC018" or "ATXDC019")).IsFalse();
    }

    [Test]
    public async Task Analyze_ReSharperDisableFile_ReportsDiagnostic()
    {
        const string source = """
                              // ReSharper disable UnusedMember.Global
                              namespace MyApp
                              {
                                  public class Foo {}
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SuppressionCommentAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXDC019")).IsTrue();
    }

    [Test]
    public async Task Analyze_ReSharperDisableOnce_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp
                              {
                                  // ReSharper disable once UnusedType.Global
                                  public class Foo {}
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SuppressionCommentAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXDC019")).IsTrue();
    }
}
