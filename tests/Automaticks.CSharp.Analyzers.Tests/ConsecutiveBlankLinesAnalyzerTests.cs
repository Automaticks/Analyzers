using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class ConsecutiveBlankLinesAnalyzerTests
{

    [Test]
    public async Task Analyze_TwoConsecutiveBlankLinesBetweenMethods_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Service {
                                      public void Foo() { }


                                      public void Bar() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ConsecutiveBlankLinesAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS044")).IsTrue();
    }

    [Test]
    public async Task Analyze_ThreeConsecutiveBlankLinesBetweenMethods_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Service {
                                      public void Foo() { }



                                      public void Bar() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ConsecutiveBlankLinesAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS044")).IsTrue();
    }

    [Test]
    public async Task Analyze_TwoConsecutiveBlankLinesInsideMethodBody_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Service {
                                      public void Foo()
                                      {
                                          var x = 1;


                                          var y = 2;
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ConsecutiveBlankLinesAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS044")).IsTrue();
    }

    [Test]
    public async Task Analyze_TwoConsecutiveBlankLinesBetweenUsingDirectives_ReportsDiagnostic()
    {
        const string source = """
                              using System;


                              using System.Collections.Generic;

                              namespace MyApp {
                                  public class Foo { }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ConsecutiveBlankLinesAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS044")).IsTrue();
    }

    [Test]
    public async Task Analyze_TwoConsecutiveBlankLinesBetweenProperties_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      public int Foo { get; set; }


                                      public int Bar { get; set; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ConsecutiveBlankLinesAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS044")).IsTrue();
    }

    [Test]
    public async Task Analyze_TwoConsecutiveBlankLinesAtTopOfFile_ReportsDiagnostic()
    {
        const string source = "\n\nnamespace MyApp {\n    public class Foo { }\n}";

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ConsecutiveBlankLinesAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS044")).IsTrue();
    }

    [Test]
    public async Task Analyze_WhitespaceOnlyLineCountsAsBlankLine_ReportsDiagnostic()
    {
        const string source = "namespace MyApp {\n    public class Service {\n        public void Foo() { }\n   \n\n        public void Bar() { }\n    }\n}";

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ConsecutiveBlankLinesAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS044")).IsTrue();
    }

    [Test]
    public async Task Analyze_TwoRunsOfConsecutiveBlankLines_ReportsTwoDiagnostics()
    {
        const string source = """
                              namespace MyApp {
                                  public class Service {
                                      public void Foo() { }


                                      public void Bar() { }


                                      public void Baz() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ConsecutiveBlankLinesAnalyzer(), source);

        await Assert.That(diagnostics.Count(d => d.Id == "ATXCS044")).IsEqualTo(2);
    }

    [Test]
    public async Task Analyze_ThreeConsecutiveBlankLines_ReportsOneDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Service {
                                      public void Foo() { }



                                      public void Bar() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ConsecutiveBlankLinesAnalyzer(), source);

        await Assert.That(diagnostics.Count(d => d.Id == "ATXCS044")).IsEqualTo(1);
    }

    [Test]
    public async Task Analyze_SingleBlankLineBetweenMethods_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Service {
                                      public void Foo() { }

                                      public void Bar() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ConsecutiveBlankLinesAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS044")).IsFalse();
    }

    [Test]
    public async Task Analyze_NoBlankLinesBetweenMembers_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Service {
                                      public void Foo() { }
                                      public void Bar() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ConsecutiveBlankLinesAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS044")).IsFalse();
    }

    [Test]
    public async Task Analyze_SingleBlankLineBetweenUsings_ReportsNoDiagnostic()
    {
        const string source = """
                              using System;

                              using System.Collections.Generic;

                              namespace MyApp {
                                  public class Foo { }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ConsecutiveBlankLinesAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS044")).IsFalse();
    }
}
