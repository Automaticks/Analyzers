using Automaticks.CSharp.Complexity;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Complexity;

/// <summary>
///     Tests that CyclomaticComplexityAnalyzer covers its accessor, operator, and conversion-operator branches.
/// </summary>
public class CyclomaticComplexityAnalyzerAccessorsTests
{
    /// <summary>
    ///     Tests that Analyze_ExternConversionOperatorHasNullBody_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExternConversionOperatorHasNullBody_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public static extern explicit operator int(Foo f);
                                  }
                              }
                              """;

        var analyzer = new CyclomaticComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.CyclomaticComplexity)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExternOperatorHasNullBody_NoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExternOperatorHasNullBody_NoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public static extern Foo operator +(Foo a, Foo b);
                                  }
                              }
                              """;

        var analyzer = new CyclomaticComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.CyclomaticComplexity)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_GetAccessorAboveThreshold_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_GetAccessorAboveThreshold_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private int _value;
                                      public int Value {
                                          get {
                                              if (_value == 1) return 1;
                                              if (_value == 2) return 2;
                                              if (_value == 3) return 3;
                                              if (_value == 4) return 4;
                                              if (_value == 5) return 5;
                                              if (_value == 6) return 6;
                                              if (_value == 7) return 7;
                                              if (_value == 8) return 8;
                                              if (_value == 9) return 9;
                                              if (_value == 10) return 10;
                                              if (_value == 11) return 11;
                                              if (_value == 12) return 12;
                                              if (_value == 13) return 13;
                                              if (_value == 14) return 14;
                                              if (_value == 15) return 15;
                                              if (_value == 16) return 16;
                                              return 0;
                                          }
                                          set { _value = value; }
                                      }
                                  }
                              }
                              """;

        var analyzer = new CyclomaticComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(diagnostics, DiagnosticIds.CSharp.CyclomaticComplexity, "get accessor of 'Value'")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_HighComplexityConversionOperator_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_HighComplexityConversionOperator_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Value;
                                      public static explicit operator int(Foo f) {
                                          if (f.Value == 1) return 1;
                                          if (f.Value == 2) return 2;
                                          if (f.Value == 3) return 3;
                                          if (f.Value == 4) return 4;
                                          if (f.Value == 5) return 5;
                                          if (f.Value == 6) return 6;
                                          if (f.Value == 7) return 7;
                                          if (f.Value == 8) return 8;
                                          if (f.Value == 9) return 9;
                                          if (f.Value == 10) return 10;
                                          if (f.Value == 11) return 11;
                                          if (f.Value == 12) return 12;
                                          if (f.Value == 13) return 13;
                                          if (f.Value == 14) return 14;
                                          if (f.Value == 15) return 15;
                                          if (f.Value == 16) return 16;
                                          return 0;
                                      }
                                  }
                              }
                              """;

        var analyzer = new CyclomaticComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(diagnostics, DiagnosticIds.CSharp.CyclomaticComplexity, "operator int")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_HighComplexityOperator_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_HighComplexityOperator_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Value;
                                      public static Foo operator +(Foo a, Foo b) {
                                          if (a.Value == 1) return a;
                                          if (a.Value == 2) return a;
                                          if (a.Value == 3) return a;
                                          if (a.Value == 4) return a;
                                          if (a.Value == 5) return a;
                                          if (a.Value == 6) return a;
                                          if (a.Value == 7) return a;
                                          if (a.Value == 8) return a;
                                          if (a.Value == 9) return a;
                                          if (a.Value == 10) return a;
                                          if (a.Value == 11) return a;
                                          if (a.Value == 12) return a;
                                          if (a.Value == 13) return a;
                                          if (a.Value == 14) return a;
                                          if (a.Value == 15) return a;
                                          if (a.Value == 16) return a;
                                          return b;
                                      }
                                  }
                              }
                              """;

        var analyzer = new CyclomaticComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(diagnostics, DiagnosticIds.CSharp.CyclomaticComplexity, "operator +")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_IndexerGetAccessorAboveThreshold_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_IndexerGetAccessorAboveThreshold_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private int _value;
                                      public int this[int index] {
                                          get {
                                              if (index == 1) return 1;
                                              if (index == 2) return 2;
                                              if (index == 3) return 3;
                                              if (index == 4) return 4;
                                              if (index == 5) return 5;
                                              if (index == 6) return 6;
                                              if (index == 7) return 7;
                                              if (index == 8) return 8;
                                              if (index == 9) return 9;
                                              if (index == 10) return 10;
                                              if (index == 11) return 11;
                                              if (index == 12) return 12;
                                              if (index == 13) return 13;
                                              if (index == 14) return 14;
                                              if (index == 15) return 15;
                                              if (index == 16) return 16;
                                              return 0;
                                          }
                                          set { _value = value; }
                                      }
                                  }
                              }
                              """;

        var analyzer = new CyclomaticComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(diagnostics, DiagnosticIds.CSharp.CyclomaticComplexity, "get accessor of indexer")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_InitAccessorAboveThreshold_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_InitAccessorAboveThreshold_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private int _value;
                                      public int Value {
                                          get => _value;
                                          init {
                                              if (value == 1) return;
                                              if (value == 2) return;
                                              if (value == 3) return;
                                              if (value == 4) return;
                                              if (value == 5) return;
                                              if (value == 6) return;
                                              if (value == 7) return;
                                              if (value == 8) return;
                                              if (value == 9) return;
                                              if (value == 10) return;
                                              if (value == 11) return;
                                              if (value == 12) return;
                                              if (value == 13) return;
                                              if (value == 14) return;
                                              if (value == 15) return;
                                              if (value == 16) return;
                                              _value = value;
                                          }
                                      }
                                  }
                              }
                              """;

        var analyzer = new CyclomaticComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(diagnostics, DiagnosticIds.CSharp.CyclomaticComplexity, "init accessor of 'Value'")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_PropertyWithAutoImplementedAccessors_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PropertyWithAutoImplementedAccessors_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Value { get; set; }
                                  }
                              }
                              """;

        var analyzer = new CyclomaticComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.CyclomaticComplexity)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_SetAccessorAboveThreshold_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SetAccessorAboveThreshold_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private int _value;
                                      public int Value {
                                          get => _value;
                                          set {
                                              if (value == 1) return;
                                              if (value == 2) return;
                                              if (value == 3) return;
                                              if (value == 4) return;
                                              if (value == 5) return;
                                              if (value == 6) return;
                                              if (value == 7) return;
                                              if (value == 8) return;
                                              if (value == 9) return;
                                              if (value == 10) return;
                                              if (value == 11) return;
                                              if (value == 12) return;
                                              if (value == 13) return;
                                              if (value == 14) return;
                                              if (value == 15) return;
                                              if (value == 16) return;
                                              _value = value;
                                          }
                                      }
                                  }
                              }
                              """;

        var analyzer = new CyclomaticComplexityAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(diagnostics, DiagnosticIds.CSharp.CyclomaticComplexity, "set accessor of 'Value'")).IsTrue();
    }
}
