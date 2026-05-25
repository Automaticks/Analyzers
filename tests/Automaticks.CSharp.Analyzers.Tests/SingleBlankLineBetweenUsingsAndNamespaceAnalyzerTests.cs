using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class SingleBlankLineBetweenUsingsAndNamespaceAnalyzerTests
{

    [Test]
    public async Task Analyze_NoBlankLineBetweenLastUsingAndBlockNamespace_ReportsDiagnostic()
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo { }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SingleBlankLineBetweenUsingsAndNamespaceAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS043")).IsTrue();
    }

    [Test]
    public async Task Analyze_NoBlankLineBetweenLastUsingAndFileScopedNamespace_ReportsDiagnostic()
    {
        const string source = """
                              using System;
                              namespace MyApp;
                              public class Foo { }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SingleBlankLineBetweenUsingsAndNamespaceAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS043")).IsTrue();
    }

    [Test]
    public async Task Analyze_MultipleUsingsNoBlankLineBeforeNamespace_ReportsDiagnostic()
    {
        const string source = """
                              using System;
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Foo { }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SingleBlankLineBetweenUsingsAndNamespaceAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS043")).IsTrue();
    }

    [Test]
    public async Task Analyze_OneBlankLineBetweenLastUsingAndBlockNamespace_ReportsNoDiagnostic()
    {
        const string source = """
                              using System;

                              namespace MyApp {
                                  public class Foo { }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SingleBlankLineBetweenUsingsAndNamespaceAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS043")).IsFalse();
    }

    [Test]
    public async Task Analyze_OneBlankLineBetweenLastUsingAndFileScopedNamespace_ReportsNoDiagnostic()
    {
        const string source = """
                              using System;

                              namespace MyApp;
                              public class Foo { }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SingleBlankLineBetweenUsingsAndNamespaceAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS043")).IsFalse();
    }

    [Test]
    public async Task Analyze_NoUsingDirectives_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SingleBlankLineBetweenUsingsAndNamespaceAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS043")).IsFalse();
    }

    [Test]
    public async Task Analyze_NoNamespaceDeclaration_ReportsNoDiagnostic()
    {
        const string source = """
                              using System;

                              public class Foo { }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SingleBlankLineBetweenUsingsAndNamespaceAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS043")).IsFalse();
    }

    [Test]
    public async Task Analyze_MultipleUsingsOneBlankLineBeforeNamespace_ReportsNoDiagnostic()
    {
        const string source = """
                              using System;
                              using System.Collections.Generic;

                              namespace MyApp {
                                  public class Foo { }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SingleBlankLineBetweenUsingsAndNamespaceAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS043")).IsFalse();
    }

    [Test]
    public async Task Analyze_BlankLineBetweenUsingsButNoBlankLineBeforeNamespace_ReportsDiagnostic()
    {
        const string source = "using System;\n\nusing System.Collections.Generic;\nnamespace MyApp;\npublic class Foo { }";

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SingleBlankLineBetweenUsingsAndNamespaceAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS043")).IsTrue();
    }
}
