using Automaticks.CSharp;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class FileNameMismatchAnalyzerTests
{
    [Test]
    public async Task AnalyzeTypeDeclaration_DottedFileName_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class OrderService { }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new FileNameMismatchAnalyzer(), source, "Order.Service.cs");

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS031")).IsTrue();
    }

    [Test]
    public async Task AnalyzeTypeDeclaration_EmptyFilePath_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new FileNameMismatchAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS031")).IsFalse();
    }

    [Test]
    public async Task AnalyzeTypeDeclaration_MatchingClassName_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new FileNameMismatchAnalyzer(), source, "Foo.cs");

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS031")).IsFalse();
    }

    [Test]
    public async Task AnalyzeTypeDeclaration_MatchingDelegateName_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public delegate void Handler(object sender);
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new FileNameMismatchAnalyzer(), source, "Handler.cs");

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS031")).IsFalse();
    }

    [Test]
    public async Task AnalyzeTypeDeclaration_MatchingEnumName_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public enum Color { Red, Green, Blue }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new FileNameMismatchAnalyzer(), source, "Color.cs");

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS031")).IsFalse();
    }

    [Test]
    public async Task AnalyzeTypeDeclaration_MatchingInterfaceName_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo { }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new FileNameMismatchAnalyzer(), source, "IFoo.cs");

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS031")).IsFalse();
    }

    [Test]
    public async Task AnalyzeTypeDeclaration_MatchingRecordName_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public record Order(int Id);
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new FileNameMismatchAnalyzer(), source, "Order.cs");

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS031")).IsFalse();
    }

    [Test]
    public async Task AnalyzeTypeDeclaration_MatchingStructName_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public struct Point { }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new FileNameMismatchAnalyzer(), source, "Point.cs");

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS031")).IsFalse();
    }

    [Test]
    public async Task AnalyzeTypeDeclaration_MismatchedClassName_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new FileNameMismatchAnalyzer(), source, "Bar.cs");

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS031")).IsTrue();
    }

    [Test]
    public async Task AnalyzeTypeDeclaration_NestedClass_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Bar {
                                      public class Foo { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new FileNameMismatchAnalyzer(), source, "Bar.cs");

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS031")).IsFalse();
    }

    [Test]
    public async Task AnalyzeTypeDeclaration_PartialClassAnyDeclarationMatchesFileName_ReportsNoDiagnostic()
    {
        IReadOnlyList<(string Source, string FilePath)> sourceFiles =
        [
            ("""
             namespace MyApp {
                 public partial class Foo { }
             }
             """, "Foo.cs"),
            ("""
             namespace MyApp {
                 public partial class Foo {
                     public void Extra() { }
                 }
             }
             """, "FooExtra.cs")
        ];

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new FileNameMismatchAnalyzer(), sourceFiles);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS031")).IsFalse();
    }

    [Test]
    public async Task AnalyzeTypeDeclaration_PartialClassInMatchingFile_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public partial class Foo { }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new FileNameMismatchAnalyzer(), source, "Foo.cs");

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS031")).IsFalse();
    }

    [Test]
    public async Task AnalyzeTypeDeclaration_PartialClassNoDeclarationMatchesFileName_ReportsDiagnostic()
    {
        IReadOnlyList<(string Source, string FilePath)> sourceFiles =
        [
            ("""
             namespace MyApp {
                 public partial class Foo { }
             }
             """, "FooA.cs"),
            ("""
             namespace MyApp {
                 public partial class Foo {
                     public void Extra() { }
                 }
             }
             """, "FooB.cs")
        ];

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new FileNameMismatchAnalyzer(), sourceFiles);

        await Assert.That(diagnostics.Count(d => d.Id == "ATXCS031")).IsEqualTo(2);
    }
}
