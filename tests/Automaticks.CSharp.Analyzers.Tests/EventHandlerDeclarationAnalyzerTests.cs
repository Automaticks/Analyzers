using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class EventHandlerDeclarationAnalyzerTests
{
    [Test]
    public async Task Analyze_ActionEventDeclaration_ReportsNoDiagnostic()
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public event Action<string> SomethingHappened;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new EventHandlerDeclarationAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS007")).IsFalse();
    }

    [Test]
    public async Task Analyze_EventHandlerFieldDeclaration_ReportsDiagnostic()
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      private EventHandler<EventArgs> _handler;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new EventHandlerDeclarationAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS007")).IsTrue();
    }

    [Test]
    public async Task Analyze_EventHandlerOfTDeclaration_ReportsDiagnostic()
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public event EventHandler<EventArgs> SomethingHappened;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new EventHandlerDeclarationAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS007")).IsTrue();
    }
}
