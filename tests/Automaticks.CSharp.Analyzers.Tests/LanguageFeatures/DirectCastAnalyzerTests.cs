using Automaticks.CSharp;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures;

/// <summary>
///     Tests for DirectCastAnalyzer.
/// </summary>
public class DirectCastAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_DirectCastToClass_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_DirectCastToClass_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Animal { }
                                  public class Dog : Animal { }
                                  public class Shelter {
                                      public Dog GetDog(Animal animal)
                                      {
                                          return (Dog)animal;
                                      }
                                  }
                              }
                              """;

        var analyzer = new DirectCastAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS029")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_DirectCastToInterface_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_DirectCastToInterface_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo { }
                                  public class Bar : IFoo { }
                                  public class Baz {
                                      public IFoo Cast(object obj)
                                      {
                                          return (IFoo)obj;
                                      }
                                  }
                              }
                              """;

        var analyzer = new DirectCastAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS029")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_DirectCastToObjectType_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_DirectCastToObjectType_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public object Box(string value)
                                      {
                                          return (object)value;
                                      }
                                  }
                              }
                              """;

        var analyzer = new DirectCastAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS029")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_EnumCast_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_EnumCast_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public enum Color { Red, Green, Blue }
                                  public class Foo {
                                      public Color FromByte(byte value)
                                      {
                                          return (Color)value;
                                      }
                                  }
                              }
                              """;

        var analyzer = new DirectCastAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS029")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_IntCast_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_IntCast_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Truncate(double value)
                                      {
                                          return (int)value;
                                      }
                                  }
                              }
                              """;

        var analyzer = new DirectCastAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS029")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_MultipleDirectCastsInSameFile_ReportsMultipleDiagnostics.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MultipleDirectCastsInSameFile_ReportsMultipleDiagnostics(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo { }
                                  public class Bar : IFoo { }
                                  public class Baz : IFoo { }
                                  public class Caster {
                                      public IFoo CastBar(object obj)
                                      {
                                          return (Bar)obj;
                                      }

                                      public IFoo CastBaz(object obj)
                                      {
                                          return (Baz)obj;
                                      }
                                  }
                              }
                              """;

        var analyzer = new DirectCastAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.CountId(diagnostics, "ATXCS029")).IsEqualTo(2);
    }

    /// <summary>
    ///     Tests that Analyze_PatternMatching_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PatternMatching_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Animal { }
                                  public class Dog : Animal { }
                                  public class Shelter {
                                      public Dog? GetDog(Animal animal)
                                      {
                                          if (animal is Dog dog)
                                          {
                                              return dog;
                                          }
                                          return null;
                                      }
                                  }
                              }
                              """;

        var analyzer = new DirectCastAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS029")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_StructCast_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StructCast_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public struct Point { public int X; public int Y; }
                                  public class Foo {
                                      public float ToFloat(int value)
                                      {
                                          return (float)value;
                                      }
                                  }
                              }
                              """;

        var analyzer = new DirectCastAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS029")).IsFalse();
    }
}
