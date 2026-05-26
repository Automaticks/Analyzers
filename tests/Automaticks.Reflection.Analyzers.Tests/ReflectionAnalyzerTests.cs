using Automaticks.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Reflection.Analyzers.Tests;

/// <summary>
///     Tests for ReflectionAnalyzer.
/// </summary>
public class ReflectionAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_ActivatorCreateInstanceGeneric_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ActivatorCreateInstanceGeneric_ReportsNoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new ReflectionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXRF030")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ActivatorCreateInstanceWithType_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ActivatorCreateInstanceWithType_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new ReflectionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXRF030")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_BindingFlagsTypeReference_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BindingFlagsTypeReference_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new ReflectionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXRF030")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_GetGenericTypeDefinitionCall_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_GetGenericTypeDefinitionCall_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new ReflectionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXRF030")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_GetInterfacesCall_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_GetInterfacesCall_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new ReflectionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXRF030")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_GetMethodCall_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_GetMethodCall_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new ReflectionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXRF030")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_GetTypeMethod_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_GetTypeMethod_ReportsNoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new ReflectionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXRF030")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_IsGenericTypeProperty_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_IsGenericTypeProperty_ReportsNoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new ReflectionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXRF030")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_MakeGenericTypeCall_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MakeGenericTypeCall_ReportsDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new ReflectionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXRF030")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_MethodInfoTypeReference_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodInfoTypeReference_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Reflection;
                              namespace MyApp {
                                  public static class Foo {
                                      private static readonly MethodInfo Method = null!;
                                  }
                              }
                              """;

        var analyzer = new ReflectionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXRF030")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ReflectionInUnrelatedMethodOfDiExtensionClass_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ReflectionInUnrelatedMethodOfDiExtensionClass_ReportsDiagnostic(CancellationToken cancellationToken)
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
        var analyzer = new ReflectionAnalyzer();
        var options = new AnalysisOptions
{
    AdditionalReferences = [externalRef]
};
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXRF030")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_SuppressMessageOnClass_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SuppressMessageOnClass_ReportsNoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new ReflectionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXRF030")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_SuppressMessageOnMethod_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SuppressMessageOnMethod_ReportsNoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new ReflectionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXRF030")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_TypeofExpression_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TypeofExpression_ReportsNoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new ReflectionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXRF030")).IsFalse();
    }
}
