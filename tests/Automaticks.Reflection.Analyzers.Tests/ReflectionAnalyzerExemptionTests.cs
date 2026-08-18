using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Reflection.Analyzers.Tests;

/// <summary>
///     Tests for the auto-exemption contexts recognised by ReflectionAnalyzer.
/// </summary>
public class ReflectionAnalyzerExemptionTests
{
    /// <summary>
    ///     Tests that Analyze_DispatchProxyStaticHelperWithMethodInfo_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_DispatchProxyStaticHelperWithMethodInfo_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Reflection;
                              namespace MyApp {
                                  public static class ProxyHelper {
                                      public static string Describe(MethodInfo method) { return method.Name; }
                                  }
                              }
                              """;

        var analyzer = new ReflectionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXRF030")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_DispatchProxySubclass_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_DispatchProxySubclass_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Reflection;
                              namespace MyApp {
                                  public class LoggingProxy : DispatchProxy {
                                      protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) { return null; }
                                  }
                              }
                              """;

        var analyzer = new ReflectionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXRF030")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_DispatchProxySubclassIndirect_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_DispatchProxySubclassIndirect_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Reflection;
                              namespace MyApp {
                                  public abstract class ProxyBase : DispatchProxy { }
                                  public class LoggingProxy : ProxyBase {
                                      protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) { return null; }
                                  }
                              }
                              """;

        var analyzer = new ReflectionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXRF030")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_InstanceHelperWithMethodInfo_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_InstanceHelperWithMethodInfo_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Reflection;
                              namespace MyApp {
                                  public class ProxyHelper {
                                      public string Describe(MethodInfo method) { return method.Name; }
                                  }
                              }
                              """;

        var analyzer = new ReflectionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXRF030")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_NonStaticServiceCollectionClass_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NonStaticServiceCollectionClass_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace Microsoft.Extensions.DependencyInjection {
                                  public interface IServiceCollection { }
                              }
                              namespace MyApp {
                                  using Microsoft.Extensions.DependencyInjection;
                                  public class Registration {
                                      public void AddAll(IServiceCollection services, Type type) { type.GetMethods(); }
                                  }
                              }
                              """;

        var analyzer = new ReflectionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXRF030")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ReflectionInFieldInitializer_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ReflectionInFieldInitializer_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      private static readonly object? Instance = Activator.CreateInstance(typeof(Foo));
                                  }
                              }
                              """;

        var analyzer = new ReflectionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXRF030")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ReflectionInStructMember_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ReflectionInStructMember_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Reflection;
                              namespace MyApp {
                                  public struct Holder {
                                      public MethodInfo? Describe() { return null; }
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
    ///     Tests that Analyze_ServiceCollectionExtensionMethod_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ServiceCollectionExtensionMethod_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace Microsoft.Extensions.DependencyInjection {
                                  public interface IServiceCollection { }
                              }
                              namespace MyApp {
                                  using Microsoft.Extensions.DependencyInjection;
                                  public static class Registration {
                                      public static void AddAll(IServiceCollection services, Type type) { type.GetMethods(); }
                                  }
                              }
                              """;

        var analyzer = new ReflectionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXRF030")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ServiceCollectionTypePresentButUnrelatedMethod_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ServiceCollectionTypePresentButUnrelatedMethod_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace Microsoft.Extensions.DependencyInjection {
                                  public interface IServiceCollection { }
                              }
                              namespace MyApp {
                                  public static class Registration {
                                      public static void Inspect(Type type) { type.GetMethods(); }
                                  }
                              }
                              """;

        var analyzer = new ReflectionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXRF030")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_TypeExtensionMethod_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TypeExtensionMethod_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public static class TypeExtensions {
                                      public static object[] Describe(this Type type) { return type.GetMethods(); }
                                  }
                              }
                              """;

        var analyzer = new ReflectionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXRF030")).IsFalse();
    }
}
