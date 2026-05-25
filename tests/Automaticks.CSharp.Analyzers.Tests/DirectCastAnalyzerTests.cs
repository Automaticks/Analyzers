using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class DirectCastAnalyzerTests
{
    [Test]
    public async Task Analyze_DirectCastToClass_ReportsDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new DirectCastAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS029")).IsTrue();
    }

    [Test]
    public async Task Analyze_DirectCastToInterface_ReportsDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new DirectCastAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS029")).IsTrue();
    }

    [Test]
    public async Task Analyze_DirectCastToObjectType_ReportsDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new DirectCastAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS029")).IsTrue();
    }

    [Test]
    public async Task Analyze_EnumCast_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new DirectCastAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS029")).IsFalse();
    }

    [Test]
    public async Task Analyze_IntCast_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new DirectCastAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS029")).IsFalse();
    }

    [Test]
    public async Task Analyze_MultipleDirectCastsInSameFile_ReportsMultipleDiagnostics()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new DirectCastAnalyzer(), source);

        await Assert.That(diagnostics.Count(d => d.Id == "ATXCS029")).IsEqualTo(2);
    }

    [Test]
    public async Task Analyze_PatternMatching_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new DirectCastAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS029")).IsFalse();
    }

    [Test]
    public async Task Analyze_StructCast_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new DirectCastAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS029")).IsFalse();
    }
}
