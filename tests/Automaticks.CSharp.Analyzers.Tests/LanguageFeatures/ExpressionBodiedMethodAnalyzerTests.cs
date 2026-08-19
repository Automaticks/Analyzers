using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures;

/// <summary>
///     Tests for ExpressionBodiedMethodAnalyzer.
/// </summary>
public class ExpressionBodiedMethodAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_AnonymousMethod_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AnonymousMethod_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;

                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                      public void Bar()
                                      {
                                          Func<int, int> square = delegate (int x)
                                          {
                                              return x * x;
                                          };
                                          square(2);
                                      }
                                  }
                              }
                              """;

        var analyzer = new ExpressionBodiedMethodAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS075")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_BlockBodiedLocalFunction_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BlockBodiedLocalFunction_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                      public bool HasValue(int value)
                                      {
                                          bool IsPositive(int candidate)
                                          {
                                              return candidate > 0;
                                          }

                                          return IsPositive(value);
                                      }
                                  }
                              }
                              """;

        var analyzer = new ExpressionBodiedMethodAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS075")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_BlockBodiedMethod_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BlockBodiedMethod_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                      public bool HasValue(int value)
                                      {
                                          return value > 0;
                                      }
                                  }
                              }
                              """;

        var analyzer = new ExpressionBodiedMethodAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS075")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExpressionBodiedAccessor_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExpressionBodiedAccessor_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                      private readonly int _value;

                                      public int Value
                                      {
                                          get => _value;
                                      }
                                  }
                              }
                              """;

        var analyzer = new ExpressionBodiedMethodAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS075")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExpressionBodiedConstructor_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExpressionBodiedConstructor_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                      private readonly int _value;

                                      public Foo(int value) => _value = value;
                                  }
                              }
                              """;

        var analyzer = new ExpressionBodiedMethodAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS075")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExpressionBodiedConversionOperator_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExpressionBodiedConversionOperator_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp
                              {
                                  public class Money
                                  {
                                      public static explicit operator int(Money money) => 0;
                                  }
                              }
                              """;

        var analyzer = new ExpressionBodiedMethodAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS075")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExpressionBodiedFinalizer_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExpressionBodiedFinalizer_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp
                              {
                                  public class Resource
                                  {
                                      ~Resource() => Release();

                                      private void Release()
                                      {
                                      }
                                  }
                              }
                              """;

        var analyzer = new ExpressionBodiedMethodAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS075")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExpressionBodiedIndexer_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExpressionBodiedIndexer_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                      private readonly int[] _items = new int[1];

                                      public int this[int index] => _items[index];
                                  }
                              }
                              """;

        var analyzer = new ExpressionBodiedMethodAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS075")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExpressionBodiedLocalFunction_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExpressionBodiedLocalFunction_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                      public void Bar()
                                      {
                                          bool HasPositive(int candidate) => candidate > 0;
                                          HasPositive(1);
                                      }
                                  }
                              }
                              """;

        var analyzer = new ExpressionBodiedMethodAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(diagnostics, "ATXCS075", "HasPositive")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ExpressionBodiedMethod_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExpressionBodiedMethod_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                      public bool HasValue(int value) => value > 0;
                                  }
                              }
                              """;

        var analyzer = new ExpressionBodiedMethodAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(diagnostics, "ATXCS075", "HasValue")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ExpressionBodiedOperator_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExpressionBodiedOperator_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp
                              {
                                  public class Money
                                  {
                                      public static Money operator +(Money left, Money right) => left;
                                  }
                              }
                              """;

        var analyzer = new ExpressionBodiedMethodAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS075")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExpressionBodiedProperty_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExpressionBodiedProperty_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                      private readonly int _value;

                                      public int Value => _value;
                                  }
                              }
                              """;

        var analyzer = new ExpressionBodiedMethodAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS075")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_LambdaExpression_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LambdaExpression_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;

                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                      public void Bar()
                                      {
                                          Func<int, int> square = x => x + 1;
                                          square(2);
                                      }
                                  }
                              }
                              """;

        var analyzer = new ExpressionBodiedMethodAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS075")).IsFalse();
    }
}
