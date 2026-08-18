using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures;

/// <summary>
///     Tests that RedundantNullCheckAnalyzer covers its coalesce, if-statement, and invocation guard branches.
/// </summary>
public class RedundantNullCheckAnalyzerBranchesTests
{
    /// <summary>
    ///     Tests that Analyze_CoalesceThrowOnLocalVariable_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CoalesceThrowOnLocalVariable_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              #nullable enable
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public Foo(string x) {
                                          string local = x;
                                          _ = local ?? throw new ArgumentNullException(nameof(local));
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantNullCheckAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS014")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_CoalesceThrowWithNonIdentifierLeftSide_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CoalesceThrowWithNonIdentifierLeftSide_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              #nullable enable
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      private string GetValue() => "value";
                                      public Foo() {
                                          _ = GetValue() ?? throw new ArgumentNullException();
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantNullCheckAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS014")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_CoalesceWithoutThrow_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CoalesceWithoutThrow_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              #nullable enable
                              namespace MyApp {
                                  public class Foo {
                                      public Foo(string? x) {
                                          var result = x ?? "default";
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantNullCheckAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS014")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_IfBodyNonThrowStatement_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_IfBodyNonThrowStatement_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              #nullable enable
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(string x) {
                                          if (x == null) return;
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantNullCheckAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS014")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_IfBodyWithMultipleStatements_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_IfBodyWithMultipleStatements_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              #nullable enable
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(string x) {
                                          if (x == null) {
                                              Console.WriteLine("null");
                                              throw new ArgumentNullException(nameof(x));
                                          }
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantNullCheckAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS014")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_IfConditionNotNullCheck_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_IfConditionNotNullCheck_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              #nullable enable
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(string x) {
                                          if (x.Length > 0) throw new ArgumentNullException(nameof(x));
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantNullCheckAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS014")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_IfWithElseClause_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_IfWithElseClause_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              #nullable enable
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(string x) {
                                          if (x == null) {
                                              throw new ArgumentNullException(nameof(x));
                                          } else {
                                              Console.WriteLine(x);
                                          }
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantNullCheckAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS014")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_InvocationDifferentMethodName_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_InvocationDifferentMethodName_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              #nullable enable
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(string x) {
                                          Console.WriteLine(x);
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantNullCheckAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS014")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_InvocationDifferentType_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_InvocationDifferentType_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              #nullable enable
                              namespace MyApp {
                                  public static class MyGuard {
                                      public static void ThrowIfNull(string value) {}
                                  }
                                  public class Foo {
                                      public void Bar(string x) {
                                          MyGuard.ThrowIfNull(x);
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantNullCheckAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS014")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_InvocationNotMemberAccess_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_InvocationNotMemberAccess_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              #nullable enable
                              namespace MyApp {
                                  public class Foo {
                                      private void ThrowIfNull(string value) {}
                                      public void Bar(string x) {
                                          ThrowIfNull(x);
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantNullCheckAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS014")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_InvocationOnInstanceNotType_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_InvocationOnInstanceNotType_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              #nullable enable
                              namespace MyApp {
                                  public class MyGuard {
                                      public void ThrowIfNull(string value) {}
                                  }
                                  public class Foo {
                                      public void Bar(string x) {
                                          var guard = new MyGuard();
                                          guard.ThrowIfNull(x);
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantNullCheckAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS014")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_InvocationZeroArguments_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_InvocationZeroArguments_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              #nullable enable
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public Foo() {
                                          ArgumentNullException.ThrowIfNull();
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantNullCheckAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS014")).IsFalse();
    }
}
