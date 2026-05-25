using Automaticks.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.Diagnostics.CodeAnalysis.Analyzers.Tests;

public class SuppressMessageAnalyzerTests
{
    [Test]
    public async Task Analyze_SuppressMessageOnMethod_ReportsDiagnostic()
    {
        const string source = """
                              using System.Diagnostics.CodeAnalysis;
                              namespace MyApp {
                                  public class Foo {
                                      [SuppressMessage("Architecture", "ATXCS011")]
                                      private static void Helper() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SuppressMessageAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXDC056")).IsTrue();
    }

    [Test]
    public async Task Analyze_SuppressMessageOnClass_ReportsDiagnostic()
    {
        const string source = """
                              using System.Diagnostics.CodeAnalysis;
                              namespace MyApp {
                                  [SuppressMessage("Architecture", "ATXCS017")]
                                  public class Foo { }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SuppressMessageAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXDC056")).IsTrue();
    }

    [Test]
    public async Task Analyze_SuppressMessageWithJustification_ReportsDiagnostic()
    {
        const string source = """
                              using System.Diagnostics.CodeAnalysis;
                              namespace MyApp {
                                  public class Foo {
                                      [SuppressMessage("Architecture", "ATXRF030", Justification = "DI registration code.")]
                                      public static void Register() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SuppressMessageAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXDC056")).IsTrue();
    }

    [Test]
    public async Task Analyze_SuppressMessageAttributeFullName_ReportsDiagnostic()
    {
        const string source = """
                              using System.Diagnostics.CodeAnalysis;
                              namespace MyApp {
                                  public class Foo {
                                      [SuppressMessageAttribute("Architecture", "ATXCS011")]
                                      private static void Helper() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SuppressMessageAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXDC056")).IsTrue();
    }

    [Test]
    public async Task Analyze_QualifiedSuppressMessage_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      [System.Diagnostics.CodeAnalysis.SuppressMessage("Architecture", "ATXCS011")]
                                      private static void Helper() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SuppressMessageAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXDC056")).IsTrue();
    }

    [Test]
    public async Task Analyze_UnrelatedAttribute_ReportsNoDiagnostic()
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  [Obsolete("Use NewFoo instead.")]
                                  public class Foo { }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SuppressMessageAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXDC056")).IsFalse();
    }

    [Test]
    public async Task Analyze_CleanClass_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SuppressMessageAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXDC056")).IsFalse();
    }

    [Test]
    public async Task Analyze_MultipleSuppressMessages_ReportsMultipleDiagnostics()
    {
        const string source = """
                              using System.Diagnostics.CodeAnalysis;
                              namespace MyApp {
                                  public class Foo {
                                      [SuppressMessage("Architecture", "ATXCS011")]
                                      [SuppressMessage("Architecture", "ATXCS021")]
                                      private static void Helper() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new SuppressMessageAnalyzer(), source);

        await Assert.That(diagnostics.Count(d => d.Id == "ATXDC056")).IsEqualTo(2);
    }
}
