using Automaticks.CSharp;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

/// <summary>
///     Tests for AbbreviatedIdentifierAnalyzer.
/// </summary>
public class AbbreviatedIdentifierAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_AxisVariableX_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AxisVariableX_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(int positionX) {
                                          var x = positionX;
                                      }
                                  }
                              }
                              """;

        var analyzer = new AbbreviatedIdentifierAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS017")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_AxisVariableY_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AxisVariableY_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(int positionY) {
                                          var y = positionY;
                                      }
                                  }
                              }
                              """;

        var analyzer = new AbbreviatedIdentifierAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS017")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_AxisVariableZ_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AxisVariableZ_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(int positionZ) {
                                          var z = positionZ;
                                      }
                                  }
                              }
                              """;

        var analyzer = new AbbreviatedIdentifierAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS017")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_CamelCaseWithAbbreviatedSegment_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CamelCaseWithAbbreviatedSegment_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(CancellationTokenSource source) {
                                          var walkCts = source;
                                      }
                                  }
                              }
                              """;

        var analyzer = new AbbreviatedIdentifierAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS017")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_CamelCaseWithFullWords_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CamelCaseWithFullWords_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(CancellationTokenSource source) {
                                          var walkCancellationTokenSource = source;
                                      }
                                  }
                              }
                              """;

        var analyzer = new AbbreviatedIdentifierAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS017")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ClassWithAbbreviatedSegment_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ClassWithAbbreviatedSegment_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class CtsManager { }
                              }
                              """;

        var analyzer = new AbbreviatedIdentifierAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS017")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ExternalPropertyOverride_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExternalPropertyOverride_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string externalSource = """
                                      public abstract class Base {
                                          public abstract string Vm { get; }
                                      }
                                      """;

        const string source = """
                              namespace MyApp {
                                  public class Derived : Base {
                                      public override string Vm { get; } = string.Empty;
                                  }
                              }
                              """;

        var externalRef = AnalyzerTestRunner.CompileToReference(externalSource);
        var analyzer = new AbbreviatedIdentifierAnalyzer();
        var options = new AnalysisOptions
{
    AdditionalReferences = [externalRef]
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS017")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_FieldWithAbbreviatedName_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FieldWithAbbreviatedName_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private int _sb;
                                  }
                              }
                              """;

        var analyzer = new AbbreviatedIdentifierAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS017")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_FieldWithFullName_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FieldWithFullName_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private int _stringBuilder;
                                  }
                              }
                              """;

        var analyzer = new AbbreviatedIdentifierAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS017")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ForEachVariableWithAbbreviatedName_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ForEachVariableWithAbbreviatedName_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(List<string> spawns) {
                                          foreach (var s in spawns) { }
                                      }
                                  }
                              }
                              """;

        var analyzer = new AbbreviatedIdentifierAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS017")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ForEachVariableWithFullName_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ForEachVariableWithFullName_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(List<string> items) {
                                          foreach (var spawn in items) { }
                                      }
                                  }
                              }
                              """;

        var analyzer = new AbbreviatedIdentifierAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS017")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_InterfaceWithAbbreviatedSegment_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_InterfaceWithAbbreviatedSegment_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface ICtxProvider { }
                              }
                              """;

        var analyzer = new AbbreviatedIdentifierAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS017")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_LocalVariableWithAbbreviatedName_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LocalVariableWithAbbreviatedName_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(CancellationToken token) {
                                          var ct = token;
                                      }
                                  }
                              }
                              """;

        var analyzer = new AbbreviatedIdentifierAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS017")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_LocalVariableWithFullName_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LocalVariableWithFullName_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(CancellationToken token) {
                                          var cancellationToken = token;
                                      }
                                  }
                              }
                              """;

        var analyzer = new AbbreviatedIdentifierAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS017")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_MethodWithAbbreviatedName_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodWithAbbreviatedName_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void GetVm() { }
                                  }
                              }
                              """;

        var analyzer = new AbbreviatedIdentifierAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS017")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_MethodWithFullName_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodWithFullName_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void GetViewModel() { }
                                  }
                              }
                              """;

        var analyzer = new AbbreviatedIdentifierAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS017")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ParameterWithAbbreviatedName_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ParameterWithAbbreviatedName_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(int ct) { }
                                  }
                              }
                              """;

        var analyzer = new AbbreviatedIdentifierAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS017")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ParameterWithFullName_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ParameterWithFullName_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(int count) { }
                                  }
                              }
                              """;

        var analyzer = new AbbreviatedIdentifierAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS017")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_PatternMatchVariableWithAbbreviatedName_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PatternMatchVariableWithAbbreviatedName_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(object obj) {
                                          if (obj is string str) { }
                                      }
                                  }
                              }
                              """;

        var analyzer = new AbbreviatedIdentifierAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS017")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_PropertyWithAbbreviatedName_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PropertyWithAbbreviatedName_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public string Vm { get; set; } = string.Empty;
                                  }
                              }
                              """;

        var analyzer = new AbbreviatedIdentifierAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS017")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_PropertyWithFullName_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PropertyWithFullName_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public string ViewModel { get; set; } = string.Empty;
                                  }
                              }
                              """;

        var analyzer = new AbbreviatedIdentifierAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS017")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ShortWordWithVowels_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ShortWordWithVowels_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() {
                                          var key = 0;
                                      }
                                  }
                              }
                              """;

        var analyzer = new AbbreviatedIdentifierAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS017")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_SingleLetterVariable_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SingleLetterVariable_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() {
                                          var i = 0;
                                      }
                                  }
                              }
                              """;

        var analyzer = new AbbreviatedIdentifierAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS017")).IsTrue();
    }
}
