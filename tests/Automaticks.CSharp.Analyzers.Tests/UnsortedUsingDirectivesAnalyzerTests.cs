using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class UnsortedUsingDirectivesAnalyzerTests
{

    [Test]
    public async Task Analyze_OutOfAlphabeticalOrder_ReportsDiagnostic()
    {
        const string source = """
                              using System;
                              using Inferno.Core;

                              namespace MyApp;
                              public class Foo { }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new UnsortedUsingDirectivesAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS047")).IsTrue();
    }

    [Test]
    public async Task Analyze_AlphabeticalOrder_ReportsNoDiagnostic()
    {
        const string source = """
                              using Inferno.Core;
                              using System;

                              namespace MyApp;
                              public class Foo { }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new UnsortedUsingDirectivesAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS047")).IsFalse();
    }

    [Test]
    public async Task Analyze_SingleUsing_ReportsNoDiagnostic()
    {
        const string source = """
                              using System;

                              namespace MyApp;
                              public class Foo { }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new UnsortedUsingDirectivesAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS047")).IsFalse();
    }

    [Test]
    public async Task Analyze_StaticUsingIgnoredInSortCheck_ReportsNoDiagnostic()
    {
        const string source = """
                              using Inferno.Core;
                              using static System.Math;
                              using System;

                              namespace MyApp;
                              public class Foo { }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new UnsortedUsingDirectivesAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS047")).IsFalse();
    }

    [Test]
    public async Task Analyze_AliasUsingIgnoredInSortCheck_ReportsNoDiagnostic()
    {
        const string source = """
                              using Inferno.Core;
                              using X = System.Text.StringBuilder;
                              using System;

                              namespace MyApp;
                              public class Foo { }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new UnsortedUsingDirectivesAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS047")).IsFalse();
    }

    [Test]
    public async Task Analyze_CaseInsensitiveOrder_ReportsNoDiagnostic()
    {
        const string source = """
                              using inferno.core;
                              using System;

                              namespace MyApp;
                              public class Foo { }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new UnsortedUsingDirectivesAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS047")).IsFalse();
    }

    [Test]
    public async Task Analyze_ConditionalUsingOutOfAlphabeticalOrder_ReportsDiagnostic()
    {
        const string source = """
                              using Inferno.Core;
                              using System;
                              #if true
                              using Avalonia.Diagnostics;
                              #endif

                              namespace MyApp;
                              public class Foo { }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new UnsortedUsingDirectivesAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS047")).IsTrue();
    }

    [Test]
    public async Task Analyze_ConditionalUsingInCorrectAlphabeticalPosition_ReportsNoDiagnostic()
    {
        const string source = """
                              using Avalonia;
                              #if true
                              using Avalonia.Diagnostics;
                              #endif
                              using System;

                              namespace MyApp;
                              public class Foo { }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new UnsortedUsingDirectivesAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS047")).IsFalse();
    }
}
