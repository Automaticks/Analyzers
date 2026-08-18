using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures;

/// <summary>
///     Tests that GenericDelegateAnalyzer covers its expression-tree and interface-implementation branches.
/// </summary>
public class GenericDelegateAnalyzerBranchesTests
{
    /// <summary>
    ///     Tests that Analyze_ExternalExplicitInterfaceImplementation_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExternalExplicitInterfaceImplementation_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string externalSource = """
                                      using System;
                                      namespace External {
                                          public interface IHandler {
                                              void Handle(Action callback);
                                          }
                                      }
                                      """;
        const string source = """
                              using System;
                              using External;
                              namespace MyApp {
                                  public class Foo : IHandler {
                                      void IHandler.Handle(Action callback) {
                                          callback();
                                      }
                                  }
                              }
                              """;

        var externalRef = AnalyzerTestRunner.CompileToReference(externalSource);
        var analyzer = new GenericDelegateAnalyzer();
        var options = new AnalysisOptions
        {
            AdditionalReferences = [externalRef]
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS020")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExternalImplicitInterfaceImplementation_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExternalImplicitInterfaceImplementation_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string externalSource = """
                                      using System;
                                      namespace External {
                                          public interface IHandler {
                                              void Handle(Action callback);
                                          }
                                      }
                                      """;
        const string source = """
                              using System;
                              using External;
                              namespace MyApp {
                                  public class Foo : IHandler {
                                      public void Handle(Action callback) {
                                          callback();
                                      }
                                  }
                              }
                              """;

        var externalRef = AnalyzerTestRunner.CompileToReference(externalSource);
        var analyzer = new GenericDelegateAnalyzer();
        var options = new AnalysisOptions
        {
            AdditionalReferences = [externalRef]
        };
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS020")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_FuncInsideExpressionTypeArgument_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FuncInsideExpressionTypeArgument_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              using System.Linq.Expressions;
                              namespace MyApp {
                                  public class Foo {
                                      private Expression<Func<int, bool>> _predicate = x => x > 0;
                                  }
                              }
                              """;

        var analyzer = new GenericDelegateAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS020")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_LocalImplicitInterfaceImplementation_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LocalImplicitInterfaceImplementation_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public interface IHandler {
                                      void Handle(Action callback);
                                  }

                                  public class Foo : IHandler {
                                      public void Handle(Action callback) {
                                          callback();
                                      }
                                  }
                              }
                              """;

        var analyzer = new GenericDelegateAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS020")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_OverrideMethodParameter_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_OverrideMethodParameter_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Base {
                                      public virtual void Handle(Action callback) {
                                          callback();
                                      }
                                  }

                                  public class Derived : Base {
                                      public override void Handle(Action callback) {
                                          callback();
                                      }
                                  }
                              }
                              """;

        var analyzer = new GenericDelegateAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS020")).IsTrue();
    }
}
