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
    ///     Tests that Analyze_ActivatorCreateInstanceWithStringArguments_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ActivatorCreateInstanceWithStringArguments_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public static class Bar {
                                      public static object? Create(string assemblyName, string typeName)
                                      {
                                          return Activator.CreateInstance(assemblyName, typeName);
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
    ///     Tests that Analyze_AliasedNonReflectionType_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AliasedNonReflectionType_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using Assembly = System.String;
                              namespace MyApp {
                                  public class Foo {
                                      public Assembly Describe() { return string.Empty; }
                                  }
                              }
                              """;

        var analyzer = new ReflectionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXRF030")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_AliasedReflectionType_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AliasedReflectionType_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using Info = System.Reflection.MethodInfo;
                              namespace MyApp {
                                  public class Foo {
                                      public Info? Describe() { return null; }
                                  }
                              }
                              """;

        var analyzer = new ReflectionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXRF030")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_BannedNameOnLocalVariable_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BannedNameOnLocalVariable_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Describe() { var Assembly = 1; return Assembly; }
                                  }
                              }
                              """;

        var analyzer = new ReflectionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXRF030")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_BannedNameOnTypeParameter_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BannedNameOnTypeParameter_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo<MethodInfo> {
                                      public MethodInfo? Describe() { return default; }
                                  }
                              }
                              """;

        var analyzer = new ReflectionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXRF030")).IsFalse();
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
    ///     Tests that Analyze_CreateInstanceOnUserType_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CreateInstanceOnUserType_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public static class Activator {
                                      public static object? CreateInstance(Type type) { return null; }
                                  }
                                  public static class Bar {
                                      public static object? Make(Type type) { return Activator.CreateInstance(type); }
                                  }
                              }
                              """;

        var analyzer = new ReflectionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXRF030")).IsFalse();
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
    ///     Tests that Analyze_ReflectionNamespaceMethodCall_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ReflectionNamespaceMethodCall_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public object? Run(System.Reflection.MethodBase method) { return method.GetParameters(); }
                                  }
                              }
                              """;

        var analyzer = new ReflectionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

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

    /// <summary>
    ///     Tests that Analyze_UnresolvableInvocation_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_UnresolvableInvocation_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Run() { Missing.Method(); }
                                  }
                              }
                              """;

        var analyzer = new ReflectionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXRF030")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_UserDefinedGetMethodsCall_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_UserDefinedGetMethodsCall_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Catalog {
                                      public object[] GetMethods() { return new object[0]; }
                                  }
                                  public class Foo {
                                      public object[] Run(Catalog catalog) { return catalog.GetMethods(); }
                                  }
                              }
                              """;

        var analyzer = new ReflectionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXRF030")).IsFalse();
    }
}
