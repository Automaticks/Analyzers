using Automaticks.CSharp;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures;

/// <summary>
///     Tests for RedundantNullCheckAnalyzer.
/// </summary>
public class RedundantNullCheckAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_CoalesceThrowOnNonNullableParam_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CoalesceThrowOnNonNullableParam_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              #nullable enable
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      private readonly string _x;
                                      public Foo(string x) {
                                          _x = x ?? throw new ArgumentNullException(nameof(x));
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantNullCheckAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS014")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_CoalesceThrowOnNullableParam_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CoalesceThrowOnNullableParam_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              #nullable enable
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      private readonly string _x;
                                      public Foo(string? x) {
                                          _x = x ?? throw new ArgumentNullException(nameof(x));
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantNullCheckAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS014")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_CoalesceThrowOnValueTypeParam_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CoalesceThrowOnValueTypeParam_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              #nullable enable
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      private readonly int _x;
                                      public Foo(int x) {
                                          _x = x ?? throw new ArgumentNullException(nameof(x));
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantNullCheckAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS014")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_CoalesceWithDifferentException_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CoalesceWithDifferentException_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              #nullable enable
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      private readonly string _x;
                                      public Foo(string x) {
                                          _x = x ?? throw new InvalidOperationException();
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantNullCheckAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS014")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_IfEqualsNullThrowOnNonNullableParam_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_IfEqualsNullThrowOnNonNullableParam_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              #nullable enable
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public Foo(string x) {
                                          if (x == null) throw new ArgumentNullException(nameof(x));
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantNullCheckAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS014")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_IfIsNullThrowOnNonNullableParam_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_IfIsNullThrowOnNonNullableParam_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              #nullable enable
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public Foo(string x) {
                                          if (x is null) throw new ArgumentNullException(nameof(x));
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantNullCheckAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS014")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ThrowIfNullOnNonNullableParam_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ThrowIfNullOnNonNullableParam_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              #nullable enable
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public Foo(string x) {
                                          ArgumentNullException.ThrowIfNull(x);
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantNullCheckAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS014")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ThrowIfNullOnNullableParam_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ThrowIfNullOnNullableParam_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              #nullable enable
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public Foo(string? x) {
                                          ArgumentNullException.ThrowIfNull(x);
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantNullCheckAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS014")).IsFalse();
    }
}
