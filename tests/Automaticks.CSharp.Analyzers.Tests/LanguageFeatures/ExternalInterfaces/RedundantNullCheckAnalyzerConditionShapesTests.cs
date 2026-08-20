using Automaticks.CSharp.LanguageFeatures;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures.ExternalInterfaces;

/// <summary>
///     Tests the condition shapes RedundantNullCheckAnalyzer inspects.
/// </summary>
public class RedundantNullCheckAnalyzerConditionShapesTests
{
    /// <summary>
    ///     Tests that Analyze_IdentifierCondition_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_IdentifierCondition_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              #nullable enable
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(string value, bool flag) {
                                          if (flag) { throw new ArgumentNullException(nameof(value)); }
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantNullCheckAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.RedundantNullCheck)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_NonEqualityCondition_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NonEqualityCondition_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(string value) {
                                          if (value.Length > 0) { throw new ArgumentNullException(nameof(value)); }
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantNullCheckAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.RedundantNullCheck)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_NullOnLeftSide_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NullOnLeftSide_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              #nullable enable
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(string value) {
                                          if (null == value) { throw new ArgumentNullException(nameof(value)); }
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantNullCheckAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.RedundantNullCheck)).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ThrowOfOtherException_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ThrowOfOtherException_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(string value) {
                                          if (value == null) { throw new InvalidOperationException("no"); }
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantNullCheckAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.RedundantNullCheck)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ThrowOfUnresolvedType_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ThrowOfUnresolvedType_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              #nullable enable
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(string value) {
                                          if (value == null) { throw new NotDeclaredException("x"); }
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantNullCheckAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.RedundantNullCheck)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ValueComparedToConstant_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ValueComparedToConstant_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public void Bar(string value) {
                                          if (value == "a") { throw new ArgumentNullException(nameof(value)); }
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantNullCheckAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.RedundantNullCheck)).IsFalse();
    }
}
