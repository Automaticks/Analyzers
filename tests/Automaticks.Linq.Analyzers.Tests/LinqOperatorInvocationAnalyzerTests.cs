using Automaticks.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Linq.Analyzers.Tests;

/// <summary>
///     Tests for LinqOperatorInvocationAnalyzer.
/// </summary>
public class LinqOperatorInvocationAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_ListInstanceMethod_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ListInstanceMethod_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Foo {
                                      public int Bar(List<int> numbers) { return numbers.Find(n => n > 1); }
                                  }
                              }
                              """;

        var analyzer = new LinqOperatorInvocationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXLQ003")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_MethodSyntaxLinqCall_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodSyntaxLinqCall_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections.Generic;
                              using System.Linq;
                              namespace MyApp {
                                  public class Foo {
                                      public IEnumerable<int> Bar(List<int> numbers) { return numbers.Select(n => n + 1); }
                                  }
                              }
                              """;

        var analyzer = new LinqOperatorInvocationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXLQ003")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_NoLinqOperator_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NoLinqOperator_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Foo {
                                      public int Bar(List<int> numbers) {
                                          var total = 0;
                                          foreach (var number in numbers) { total += number; }
                                          return total;
                                      }
                                  }
                              }
                              """;

        var analyzer = new LinqOperatorInvocationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXLQ003")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_QueryableOperator_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_QueryableOperator_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Linq;
                              namespace MyApp {
                                  public class Foo {
                                      public IQueryable<int> Bar(IQueryable<int> numbers) { return numbers.Where(n => n > 1); }
                                  }
                              }
                              """;

        var analyzer = new LinqOperatorInvocationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXLQ003")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_QuerySyntaxExpression_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_QuerySyntaxExpression_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections.Generic;
                              using System.Linq;
                              namespace MyApp {
                                  public class Foo {
                                      public IEnumerable<int> Bar(List<int> numbers) { return from n in numbers select n; }
                                  }
                              }
                              """;

        var analyzer = new LinqOperatorInvocationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXLQ003")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_StaticEnumerableCallWithoutUsingDirective_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StaticEnumerableCallWithoutUsingDirective_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Foo {
                                      public int Bar(List<int> numbers) { return System.Linq.Enumerable.First(numbers); }
                                  }
                              }
                              """;

        var analyzer = new LinqOperatorInvocationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXLQ003")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_UnresolvedInvocation_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_UnresolvedInvocation_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar() { Missing.Method(); }
                                  }
                              }
                              """;

        var analyzer = new LinqOperatorInvocationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXLQ003")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_UserDefinedSelectMethod_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_UserDefinedSelectMethod_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Select(int value) { return value; }
                                      public int Bar() { return Select(1); }
                                  }
                              }
                              """;

        var analyzer = new LinqOperatorInvocationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXLQ003")).IsFalse();
    }
}
