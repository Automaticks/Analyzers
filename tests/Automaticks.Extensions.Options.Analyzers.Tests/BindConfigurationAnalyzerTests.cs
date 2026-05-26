using Automaticks.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Extensions.Options.Analyzers.Tests;

/// <summary>
///     Tests for BindConfigurationAnalyzer.
/// </summary>
public sealed class BindConfigurationAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_BindConfigurationOnOptionsBuilder_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BindConfigurationOnOptionsBuilder_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace Microsoft.Extensions.Options {
                                  public class OptionsBuilder<T> {}
                                  public static class OptionsBuilderExtensions {
                                      public static OptionsBuilder<T> BindConfiguration<T>(this OptionsBuilder<T> builder, string section) => builder;
                                  }
                              }
                              namespace MyApp {
                                  public class MyOptions {}
                                  public class Startup {
                                      public void Configure(Microsoft.Extensions.Options.OptionsBuilder<MyOptions> builder) {
                                          builder.BindConfiguration("MyOptions");
                                      }
                                  }
                              }
                              """;

        var analyzer = new BindConfigurationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXEO049")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_BindConfigurationOnUnrelatedType_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BindConfigurationOnUnrelatedType_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class SomeBuilder {
                                      public SomeBuilder BindConfiguration(string section) => this;
                                  }
                                  public class Startup {
                                      public void Configure(SomeBuilder builder) {
                                          builder.BindConfiguration("section");
                                      }
                                  }
                              }
                              """;

        var analyzer = new BindConfigurationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXEO049")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ConfigureWithGetRequiredSection_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConfigureWithGetRequiredSection_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace Microsoft.Extensions.Options {
                                  public class OptionsBuilder<T> {}
                              }
                              namespace Microsoft.Extensions.Configuration {
                                  public interface IConfiguration {
                                      IConfiguration GetRequiredSection(string key);
                                  }
                              }
                              namespace Microsoft.Extensions.DependencyInjection {
                                  public class IServiceCollection {}
                                  public static class OptionsConfigurationServiceCollectionExtensions {
                                      public static void Configure<T>(this IServiceCollection services, Microsoft.Extensions.Configuration.IConfiguration config) {}
                                  }
                              }
                              namespace MyApp {
                                  public class MyOptions {}
                                  public class Startup {
                                      public void Configure(
                                          Microsoft.Extensions.DependencyInjection.IServiceCollection services,
                                          Microsoft.Extensions.Configuration.IConfiguration configuration) {
                                          services.Configure<MyOptions>(configuration.GetRequiredSection("MyOptions"));
                                      }
                                  }
                              }
                              """;

        var analyzer = new BindConfigurationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXEO049")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ConfigureWithGetSection_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConfigureWithGetSection_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace Microsoft.Extensions.Options {
                                  public class OptionsBuilder<T> {}
                              }
                              namespace Microsoft.Extensions.Configuration {
                                  public interface IConfiguration {
                                      IConfiguration GetSection(string key);
                                  }
                              }
                              namespace Microsoft.Extensions.DependencyInjection {
                                  public class IServiceCollection {}
                                  public static class OptionsConfigurationServiceCollectionExtensions {
                                      public static void Configure<T>(this IServiceCollection services, Microsoft.Extensions.Configuration.IConfiguration config) {}
                                  }
                              }
                              namespace MyApp {
                                  public class MyOptions {}
                                  public class Startup {
                                      public void Configure(
                                          Microsoft.Extensions.DependencyInjection.IServiceCollection services,
                                          Microsoft.Extensions.Configuration.IConfiguration configuration) {
                                          services.Configure<MyOptions>(configuration.GetSection("MyOptions"));
                                      }
                                  }
                              }
                              """;

        var analyzer = new BindConfigurationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXEO049")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_FluentAddOptionsBindConfiguration_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FluentAddOptionsBindConfiguration_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace Microsoft.Extensions.Options {
                                  public class OptionsBuilder<T> {
                                      public OptionsBuilder<T> BindConfiguration(string section) => this;
                                  }
                              }
                              namespace MyApp {
                                  public class MyOptions {}
                                  public class OptionsFactory {
                                      public static Microsoft.Extensions.Options.OptionsBuilder<T> Create<T>()
                                          => new Microsoft.Extensions.Options.OptionsBuilder<T>();
                                  }
                                  public class Startup {
                                      public void Configure() {
                                          OptionsFactory.Create<MyOptions>().BindConfiguration("MyOptions");
                                      }
                                  }
                              }
                              """;

        var analyzer = new BindConfigurationAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXEO049")).IsTrue();
    }
}
