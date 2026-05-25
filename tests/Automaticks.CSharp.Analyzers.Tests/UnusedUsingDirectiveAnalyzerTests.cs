using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class UnusedUsingDirectiveAnalyzerTests
{

    [Test]
    public async Task Analyze_UnusedRegularUsing_ReportsDiagnostic()
    {
        const string source = """
                              using System.Text;

                              namespace MyApp;
                              public class Foo { }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new UnusedUsingDirectiveAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS048")).IsTrue();
    }

    [Test]
    public async Task Analyze_UsedRegularUsing_ReportsNoDiagnostic()
    {
        const string source = """
                              using System.Text;

                              namespace MyApp;
                              public class Foo {
                                  public System.Text.StringBuilder Builder { get; set; }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new UnusedUsingDirectiveAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS048")).IsFalse();
    }

    [Test]
    public async Task Analyze_StaticUsingNotChecked_ReportsNoDiagnostic()
    {
        const string source = """
                              using static System.Math;

                              namespace MyApp;
                              public class Foo { }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new UnusedUsingDirectiveAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS048")).IsFalse();
    }

    [Test]
    public async Task Analyze_AliasUsingNotChecked_ReportsNoDiagnostic()
    {
        const string source = """
                              using X = System.Text.StringBuilder;

                              namespace MyApp;
                              public class Foo { }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new UnusedUsingDirectiveAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS048")).IsFalse();
    }

    [Test]
    public async Task Analyze_GlobalUsingNotChecked_ReportsNoDiagnostic()
    {
        const string source = "global using System.Text;\nnamespace MyApp;\npublic class Foo { }";

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new UnusedUsingDirectiveAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS048")).IsFalse();
    }

    [Test]
    public async Task Analyze_NoUsings_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp;
                              public class Foo { }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new UnusedUsingDirectiveAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS048")).IsFalse();
    }

    [Test]
    public async Task Analyze_UsedViaSimpleTypeName_ReportsNoDiagnostic()
    {
        const string source = """
                              using System.Collections.Generic;

                              namespace MyApp;
                              public class Foo {
                                  public List<int> Items { get; set; }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new UnusedUsingDirectiveAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS048")).IsFalse();
    }
}
