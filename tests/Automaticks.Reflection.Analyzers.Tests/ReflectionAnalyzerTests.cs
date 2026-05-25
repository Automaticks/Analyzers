using Automaticks.Reflection;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.Reflection.Analyzers.Tests;

public class ReflectionAnalyzerTests
{
    [Test]
    public async Task Analyze_ActivatorCreateInstanceGeneric_ReportsNoDiagnostic()
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo { }
                                  public static class Bar {
                                      public static Foo Create()
                                      {
                                          return Activator.CreateInstance<Foo>()!;
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ReflectionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXRF030")).IsFalse();
    }

    [Test]
    public async Task Analyze_ActivatorCreateInstanceWithType_ReportsDiagnostic()
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public static class Foo {
                                      public static object Create(Type t)
                                      {
                                          return Activator.CreateInstance(t)!;
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ReflectionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXRF030")).IsTrue();
    }

    [Test]
    public async Task Analyze_BindingFlagsTypeReference_ReportsDiagnostic()
    {
        const string source = """
                              using System.Reflection;
                              namespace MyApp {
                                  public static class Foo {
                                      public static void Bar()
                                      {
                                          var flags = BindingFlags.Public | BindingFlags.Static;
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ReflectionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXRF030")).IsTrue();
    }

    [Test]
    public async Task Analyze_GetGenericTypeDefinitionCall_ReportsDiagnostic()
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public static class Foo {
                                      public static Type GetDefinition(Type t)
                                      {
                                          return t.GetGenericTypeDefinition();
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ReflectionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXRF030")).IsTrue();
    }

    [Test]
    public async Task Analyze_GetInterfacesCall_ReportsDiagnostic()
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public static class Foo {
                                      public static Type[] GetAll(Type t)
                                      {
                                          return t.GetInterfaces();
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ReflectionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXRF030")).IsTrue();
    }

    [Test]
    public async Task Analyze_GetMethodCall_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public static class Foo {
                                      public static void Bar()
                                      {
                                          var method = typeof(Foo).GetMethod("Bar");
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ReflectionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXRF030")).IsTrue();
    }

    [Test]
    public async Task Analyze_GetTypeMethod_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public System.Type GetFooType()
                                      {
                                          return GetType();
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ReflectionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXRF030")).IsFalse();
    }

    [Test]
    public async Task Analyze_IsGenericTypeProperty_ReportsNoDiagnostic()
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public static class Foo {
                                      public static bool Check(Type t)
                                      {
                                          return t.IsGenericType;
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ReflectionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXRF030")).IsFalse();
    }

    [Test]
    public async Task Analyze_MakeGenericTypeCall_ReportsDiagnostic()
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public static class Foo {
                                      public static Type Build(Type openGeneric, Type arg)
                                      {
                                          return openGeneric.MakeGenericType(arg);
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ReflectionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXRF030")).IsTrue();
    }

    [Test]
    public async Task Analyze_MethodInfoTypeReference_ReportsDiagnostic()
    {
        const string source = """
                              using System.Reflection;
                              namespace MyApp {
                                  public static class Foo {
                                      private static readonly MethodInfo Method = null!;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ReflectionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXRF030")).IsTrue();
    }

    [Test]
    public async Task Analyze_SuppressMessageOnClass_ReportsNoDiagnostic()
    {
        const string source = """
                              using System.Diagnostics.CodeAnalysis;
                              using System.Reflection;
                              namespace MyApp {
                                  [SuppressMessage("Architecture", "ATXRF030", Justification = "DI registration")]
                                  public static class ServiceRegistration {
                                      public static MethodInfo? GetInfo()
                                      {
                                          return typeof(ServiceRegistration).GetMethod("GetInfo");
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ReflectionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXRF030")).IsFalse();
    }

    [Test]
    public async Task Analyze_SuppressMessageOnMethod_ReportsNoDiagnostic()
    {
        const string source = """
                              using System.Diagnostics.CodeAnalysis;
                              using System.Reflection;
                              namespace MyApp {
                                  public static class Foo {
                                      [SuppressMessage("Architecture", "ATXRF030", Justification = "DI only")]
                                      public static MethodInfo? GetInfo()
                                      {
                                          return typeof(Foo).GetMethod("GetInfo");
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ReflectionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXRF030")).IsFalse();
    }

    [Test]
    public async Task Analyze_TypeofExpression_ReportsNoDiagnostic()
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public static class Foo {
                                      public static Type GetType()
                                      {
                                          return typeof(Foo);
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ReflectionAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXRF030")).IsFalse();
    }

    [Test]
    public async Task Analyze_ReflectionInUnrelatedMethodOfDiExtensionClass_ReportsDiagnostic()
    {
        const string source = """
                              using System;
                              using Microsoft.Extensions.DependencyInjection;
                              namespace MyApp {
                                  public static class ServiceExtensions {
                                      public static IServiceCollection AddMyService(this IServiceCollection services)
                                      {
                                          return services;
                                      }
                                      public static object CreateWithReflection(Type type)
                                      {
                                          return Activator.CreateInstance(type)!;
                                      }
                                  }
                              }
                              """;

        var externalRef = AnalyzerTestRunner.CompileToReference("""
                                                                 namespace Microsoft.Extensions.DependencyInjection {
                                                                     public interface IServiceCollection { }
                                                                 }
                                                                 """);
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ReflectionAnalyzer(), source, [externalRef]);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXRF030")).IsTrue();
    }
}
