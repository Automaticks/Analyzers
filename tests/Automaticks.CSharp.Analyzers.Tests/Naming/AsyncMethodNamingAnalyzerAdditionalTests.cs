using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Naming;

/// <summary>
///     Additional tests for AsyncMethodNamingAnalyzer.
/// </summary>
public class AsyncMethodNamingAnalyzerAdditionalTests
{
    /// <summary>
    ///     Tests that Analyze_CustomAsyncEnumerableInDifferentNamespace_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CustomAsyncEnumerableInDifferentNamespace_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class IAsyncEnumerable<T> { }
                                  public class Foo {
                                      public IAsyncEnumerable<int> GetItems() => new IAsyncEnumerable<int>();
                                  }
                              }
                              """;

        var analyzer = new AsyncMethodNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS003")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_CustomAsyncEnumerableWithTwoArguments_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CustomAsyncEnumerableWithTwoArguments_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class IAsyncEnumerable<TFirst, TSecond> { }
                                  public class Foo {
                                      public IAsyncEnumerable<int, int> GetItems() => new IAsyncEnumerable<int, int>();
                                  }
                              }
                              """;

        var analyzer = new AsyncMethodNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS003")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExternalInterfacePropertyWithMethod_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExternalInterfacePropertyWithMethod_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string externalSource = """
                                      public interface IWorker {
                                          bool IsReady { get; }
                                      }
                                      """;

        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class Worker : IWorker {
                                      public bool IsReady { get; } = true;
                                      public Task DoWork() => Task.CompletedTask;
                                  }
                              }
                              """;

        var externalRef = AnalyzerTestRunner.CompileToReference(externalSource);
        var analyzer = new AsyncMethodNamingAnalyzer();
        var options = new AnalysisOptions
{
    AdditionalReferences = [externalRef]
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS003")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_TestMethodWithMissingAttribute_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TestMethodWithMissingAttribute_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading.Tasks;
                              namespace MyApp {
                                  public class MyTests {
                                      [Missing]
                                      public Task VerifyBehavior() => Task.CompletedTask;
                                  }
                              }
                              """;

        var analyzer = new AsyncMethodNamingAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS003")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_TestMethodWithOtherAttribute_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TestMethodWithOtherAttribute_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string externalSource = """
                                      namespace TUnit {
                                          [System.AttributeUsage(System.AttributeTargets.Method)]
                                          public sealed class OtherAttribute : System.Attribute { }
                                      }
                                      """;

        const string source = """
                              using System.Threading.Tasks;
                              using TUnit;
                              namespace MyApp {
                                  public class MyTests {
                                      [Other]
                                      public Task VerifyBehavior() => Task.CompletedTask;
                                  }
                              }
                              """;

        var externalRef = AnalyzerTestRunner.CompileToReference(externalSource);
        var analyzer = new AsyncMethodNamingAnalyzer();
        var options = new AnalysisOptions
{
    AdditionalReferences = [externalRef]
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS003")).IsTrue();
    }
}
