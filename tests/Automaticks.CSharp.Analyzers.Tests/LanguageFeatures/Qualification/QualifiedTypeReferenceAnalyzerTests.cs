using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures.Qualification;

/// <summary>
///     Tests for QualifiedTypeReferenceAnalyzer.
/// </summary>
public class QualifiedTypeReferenceAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_CollisionWithDifferentType_ReportsCollisionMessageNamingBothTypes.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CollisionWithDifferentType_ReportsCollisionMessageNamingBothTypes(CancellationToken cancellationToken)
    {
        const string source = """
                              using MyApp;

                              namespace MyApp
                              {
                                  public class File
                                  {
                                  }
                              }

                              namespace Other
                              {
                                  public class Foo
                                  {
                                      public void Bar()
                                      {
                                          var x = System.IO.File.Exists("p");
                                      }
                                  }
                              }
                              """;

        var analyzer = new QualifiedTypeReferenceAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(diagnostics, "ATXCS072", "System.IO.File")).IsTrue();
        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(diagnostics, "ATXCS072", "MyApp.File")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_CrefInXmlDoc_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CrefInXmlDoc_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                      /// <summary>
                                      /// See <see cref="System.IO.File"/> for details.
                                      /// </summary>
                                      public void Bar()
                                      {
                                      }
                                  }
                              }
                              """;

        var analyzer = new QualifiedTypeReferenceAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS072")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExpressionPositionWithUsingMissing_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExpressionPositionWithUsingMissing_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                      public void Bar()
                                      {
                                          var exists = System.IO.File.Exists("p");
                                      }
                                  }
                              }
                              """;

        var analyzer = new QualifiedTypeReferenceAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(diagnostics, "ATXCS072", "System.IO.File")).IsTrue();
        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(diagnostics, "ATXCS072", "File")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ExpressionPositionWithUsingPresent_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExpressionPositionWithUsingPresent_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.IO;

                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                      public void Bar()
                                      {
                                          var exists = System.IO.File.Exists("p");
                                      }
                                  }
                              }
                              """;

        var analyzer = new QualifiedTypeReferenceAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(diagnostics, "ATXCS072", "System.IO.File")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_NameOfExpression_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NameOfExpression_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                      public void Bar()
                                      {
                                          var name = nameof(System.IO.File);
                                      }
                                  }
                              }
                              """;

        var analyzer = new QualifiedTypeReferenceAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS072")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_NestedTypeAccess_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NestedTypeAccess_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp
                              {
                                  public class Outer
                                  {
                                      public class Inner
                                      {
                                          public const int Value = 1;
                                      }
                                  }

                                  public class Foo
                                  {
                                      public void Bar()
                                      {
                                          var nested = Outer.Inner.Value;
                                      }
                                  }
                              }
                              """;

        var analyzer = new QualifiedTypeReferenceAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS072")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_SimpleNameAlreadyUnqualified_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SimpleNameAlreadyUnqualified_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.IO;

                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                      public void Bar()
                                      {
                                          var exists = File.Exists("p");
                                      }
                                  }
                              }
                              """;

        var analyzer = new QualifiedTypeReferenceAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS072")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_TypePositionQualifiedName_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TypePositionQualifiedName_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                      public void Bar()
                                      {
                                          System.IO.FileInfo info = new System.IO.FileInfo("p");
                                      }
                                  }
                              }
                              """;

        var analyzer = new QualifiedTypeReferenceAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(diagnostics, "ATXCS072", "System.IO.FileInfo")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_UsingStaticDirective_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_UsingStaticDirective_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              using static System.Math;

                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                      public void Bar()
                                      {
                                          var x = Sqrt(4.0);
                                      }
                                  }
                              }
                              """;

        var analyzer = new QualifiedTypeReferenceAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS072")).IsFalse();
    }
}
