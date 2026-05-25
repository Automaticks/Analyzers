using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class DuplicateUsingDirectiveAnalyzerTests
{

    [Test]
    public async Task Analyze_DuplicateRegularUsing_ReportsDiagnostic()
    {
        const string source = """
                              using System;
                              using System;

                              namespace MyApp;
                              public class Foo { }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new DuplicateUsingDirectiveAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS046")).IsTrue();
    }

    [Test]
    public async Task Analyze_DuplicateStaticUsing_ReportsDiagnostic()
    {
        const string source = """
                              using static System.Math;
                              using static System.Math;

                              namespace MyApp;
                              public class Foo { }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new DuplicateUsingDirectiveAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS046")).IsTrue();
    }

    [Test]
    public async Task Analyze_DuplicateAliasUsing_ReportsDiagnostic()
    {
        const string source = """
                              using X = System.Text.StringBuilder;
                              using X = System.Text.StringBuilder;

                              namespace MyApp;
                              public class Foo { }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new DuplicateUsingDirectiveAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS046")).IsTrue();
    }

    [Test]
    public async Task Analyze_DuplicateGlobalUsing_ReportsNoDiagnostic()
    {
        const string source = "global using System;\nglobal using System;\nnamespace MyApp;\npublic class Foo { }";

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new DuplicateUsingDirectiveAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS046")).IsFalse();
    }

    [Test]
    public async Task Analyze_UniqueUsings_ReportsNoDiagnostic()
    {
        const string source = """
                              using System;
                              using System.Collections.Generic;

                              namespace MyApp;
                              public class Foo { }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new DuplicateUsingDirectiveAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS046")).IsFalse();
    }

    [Test]
    public async Task Analyze_TriplicateUsing_ReportsDiagnosticTwice()
    {
        const string source = """
                              using System;
                              using System;
                              using System;

                              namespace MyApp;
                              public class Foo { }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new DuplicateUsingDirectiveAnalyzer(), source);

        await Assert.That(diagnostics.Count(d => d.Id == "ATXCS046")).IsEqualTo(2);
    }
}
