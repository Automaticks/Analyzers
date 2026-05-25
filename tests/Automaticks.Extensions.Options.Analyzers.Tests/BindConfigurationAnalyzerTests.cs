using Automaticks.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.Extensions.Options.Analyzers.Tests;

public sealed class BindConfigurationAnalyzerTests
{
    [Test]
    public async Task Analyze_BindConfigurationOnOptionsBuilder_ReportsDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BindConfigurationAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXEO049")).IsTrue();
    }

    [Test]
    public async Task Analyze_FluentAddOptionsBindConfiguration_ReportsDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BindConfigurationAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXEO049")).IsTrue();
    }

    [Test]
    public async Task Analyze_ConfigureWithGetRequiredSection_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BindConfigurationAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXEO049")).IsFalse();
    }

    [Test]
    public async Task Analyze_BindConfigurationOnUnrelatedType_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BindConfigurationAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXEO049")).IsFalse();
    }

    [Test]
    public async Task Analyze_ConfigureWithGetSection_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BindConfigurationAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXEO049")).IsFalse();
    }
}
