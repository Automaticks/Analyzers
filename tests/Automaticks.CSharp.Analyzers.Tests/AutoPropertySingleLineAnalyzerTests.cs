using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class AutoPropertySingleLineAnalyzerTests
{

    [Test]
    public async Task Analyze_MultiLineGetOnlyAutoProperty_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      public string Name
                                      {
                                          get;
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AutoPropertySingleLineAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS045")).IsTrue();
    }

    [Test]
    public async Task Analyze_MultiLineGetSetAutoProperty_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      public string Name
                                      {
                                          get;
                                          set;
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AutoPropertySingleLineAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS045")).IsTrue();
    }

    [Test]
    public async Task Analyze_MultiLineGetInitAutoProperty_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public record Config {
                                      public int Width
                                      {
                                          get;
                                          init;
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AutoPropertySingleLineAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS045")).IsTrue();
    }

    [Test]
    public async Task Analyze_MultiLineSetOnlyAutoProperty_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      public string Name
                                      {
                                          set;
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AutoPropertySingleLineAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS045")).IsTrue();
    }

    [Test]
    public async Task Analyze_MultiLineAutoPropertyInInterface_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public interface IModel {
                                      int Count
                                      {
                                          get;
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AutoPropertySingleLineAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS045")).IsTrue();
    }

    [Test]
    public async Task Analyze_SingleLineGetOnlyAutoProperty_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      public string Name { get; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AutoPropertySingleLineAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS045")).IsFalse();
    }

    [Test]
    public async Task Analyze_SingleLineGetSetAutoProperty_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      public string Name { get; set; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AutoPropertySingleLineAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS045")).IsFalse();
    }

    [Test]
    public async Task Analyze_SingleLineGetInitAutoProperty_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public record Config {
                                      public int Width { get; init; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AutoPropertySingleLineAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS045")).IsFalse();
    }

    [Test]
    public async Task Analyze_SingleLineSetOnlyAutoProperty_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      public string Name { set; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AutoPropertySingleLineAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS045")).IsFalse();
    }

    [Test]
    public async Task Analyze_MultiLinePropertyWithExpressionBodyGetter_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      private string _name = "";
                                      public string Name
                                      {
                                          get => _name;
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AutoPropertySingleLineAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS045")).IsFalse();
    }

    [Test]
    public async Task Analyze_MultiLinePropertyWithBlockBodyGetter_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      private string _name = "";
                                      public string Name
                                      {
                                          get { return _name; }
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AutoPropertySingleLineAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS045")).IsFalse();
    }

    [Test]
    public async Task Analyze_ExpressionBodiedProperty_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Model {
                                      private int _value = 0;
                                      public int Value => _value;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AutoPropertySingleLineAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS045")).IsFalse();
    }

    [Test]
    public async Task Analyze_AttributeOnSeparateLineWithSingleLineAutoProperty_ReportsNoDiagnostic()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;
                              namespace MyApp {
                                  public class Model {
                                      [Required]
                                      public string Name { get; set; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new AutoPropertySingleLineAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS045")).IsFalse();
    }
}
