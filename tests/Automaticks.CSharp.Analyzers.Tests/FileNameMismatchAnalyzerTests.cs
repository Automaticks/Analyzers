using Automaticks.CSharp;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

/// <summary>
///     Tests for FileNameMismatchAnalyzer.
/// </summary>
public class FileNameMismatchAnalyzerTests
{
    /// <summary>
    ///     Tests that AnalyzeTypeDeclaration_DottedFileName_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AnalyzeTypeDeclaration_DottedFileName_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class OrderService { }
                              }
                              """;

        var analyzer = new FileNameMismatchAnalyzer();
        var options = new AnalysisOptions
{
    FilePath = "Order.Service.cs"
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS031")).IsTrue();
    }

    /// <summary>
    ///     Tests that AnalyzeTypeDeclaration_EmptyFilePath_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AnalyzeTypeDeclaration_EmptyFilePath_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { }
                              }
                              """;

        var analyzer = new FileNameMismatchAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS031")).IsFalse();
    }

    /// <summary>
    ///     Tests that AnalyzeTypeDeclaration_MatchingClassName_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AnalyzeTypeDeclaration_MatchingClassName_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { }
                              }
                              """;

        var analyzer = new FileNameMismatchAnalyzer();
        var options = new AnalysisOptions
{
    FilePath = "Foo.cs"
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS031")).IsFalse();
    }

    /// <summary>
    ///     Tests that AnalyzeTypeDeclaration_MatchingDelegateName_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AnalyzeTypeDeclaration_MatchingDelegateName_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public delegate void Handler(object sender);
                              }
                              """;

        var analyzer = new FileNameMismatchAnalyzer();
        var options = new AnalysisOptions
{
    FilePath = "Handler.cs"
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS031")).IsFalse();
    }

    /// <summary>
    ///     Tests that AnalyzeTypeDeclaration_MatchingEnumName_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AnalyzeTypeDeclaration_MatchingEnumName_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public enum Color { Red, Green, Blue }
                              }
                              """;

        var analyzer = new FileNameMismatchAnalyzer();
        var options = new AnalysisOptions
{
    FilePath = "Color.cs"
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS031")).IsFalse();
    }

    /// <summary>
    ///     Tests that AnalyzeTypeDeclaration_MatchingInterfaceName_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AnalyzeTypeDeclaration_MatchingInterfaceName_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo { }
                              }
                              """;

        var analyzer = new FileNameMismatchAnalyzer();
        var options = new AnalysisOptions
{
    FilePath = "IFoo.cs"
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS031")).IsFalse();
    }

    /// <summary>
    ///     Tests that AnalyzeTypeDeclaration_MatchingRecordName_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AnalyzeTypeDeclaration_MatchingRecordName_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public record Order(int Id);
                              }
                              """;

        var analyzer = new FileNameMismatchAnalyzer();
        var options = new AnalysisOptions
{
    FilePath = "Order.cs"
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS031")).IsFalse();
    }

    /// <summary>
    ///     Tests that AnalyzeTypeDeclaration_MatchingStructName_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AnalyzeTypeDeclaration_MatchingStructName_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public struct Point { }
                              }
                              """;

        var analyzer = new FileNameMismatchAnalyzer();
        var options = new AnalysisOptions
{
    FilePath = "Point.cs"
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS031")).IsFalse();
    }

    /// <summary>
    ///     Tests that AnalyzeTypeDeclaration_MismatchedClassName_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AnalyzeTypeDeclaration_MismatchedClassName_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { }
                              }
                              """;

        var analyzer = new FileNameMismatchAnalyzer();
        var options = new AnalysisOptions
{
    FilePath = "Bar.cs"
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS031")).IsTrue();
    }

    /// <summary>
    ///     Tests that AnalyzeTypeDeclaration_NestedClass_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AnalyzeTypeDeclaration_NestedClass_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Bar {
                                      public class Foo { }
                                  }
                              }
                              """;

        var analyzer = new FileNameMismatchAnalyzer();
        var options = new AnalysisOptions
{
    FilePath = "Bar.cs"
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS031")).IsFalse();
    }

    /// <summary>
    ///     Tests that AnalyzeTypeDeclaration_PartialClassAnyDeclarationMatchesFileName_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AnalyzeTypeDeclaration_PartialClassAnyDeclarationMatchesFileName_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        var firstSourceFile = new SourceFile
        {
            Source = """
                     namespace MyApp {
                         public partial class Foo { }
                     }
                     """,
            FilePath = "Foo.cs"
        };
        var secondSourceFile = new SourceFile
        {
            Source = """
                     namespace MyApp {
                         public partial class Foo {
                             public void Extra() { }
                         }
                     }
                     """,
            FilePath = "FooExtra.cs"
        };
        IReadOnlyList<SourceFile> sourceFiles = [firstSourceFile, secondSourceFile];

        var analyzer = new FileNameMismatchAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, sourceFiles, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS031")).IsFalse();
    }

    /// <summary>
    ///     Tests that AnalyzeTypeDeclaration_PartialClassInMatchingFile_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AnalyzeTypeDeclaration_PartialClassInMatchingFile_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public partial class Foo { }
                              }
                              """;

        var analyzer = new FileNameMismatchAnalyzer();
        var options = new AnalysisOptions
{
    FilePath = "Foo.cs"
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS031")).IsFalse();
    }

    /// <summary>
    ///     Tests that AnalyzeTypeDeclaration_PartialClassNoDeclarationMatchesFileName_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AnalyzeTypeDeclaration_PartialClassNoDeclarationMatchesFileName_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        var firstSourceFile = new SourceFile
        {
            Source = """
                     namespace MyApp {
                         public partial class Foo { }
                     }
                     """,
            FilePath = "FooA.cs"
        };
        var secondSourceFile = new SourceFile
        {
            Source = """
                     namespace MyApp {
                         public partial class Foo {
                             public void Extra() { }
                         }
                     }
                     """,
            FilePath = "FooB.cs"
        };
        IReadOnlyList<SourceFile> sourceFiles = [firstSourceFile, secondSourceFile];

        var analyzer = new FileNameMismatchAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, sourceFiles, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.CountId(diagnostics, "ATXCS031")).IsEqualTo(2);
    }
}
