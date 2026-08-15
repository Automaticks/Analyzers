using Automaticks.Extensions.Options.CodeFixes;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Extensions.Options.Analyzers.Tests;

/// <summary>
///     Tests for BindConfigurationCodeFixProvider.
/// </summary>
public sealed class BindConfigurationCodeFixProviderTests
{
    private const string FrameworkStubs = """
                                          namespace Microsoft.Extensions.Configuration {
                                              public interface IConfigurationSection {}
                                              public interface IConfiguration {}
                                              public static class ConfigurationExtensions {
                                                  public static IConfigurationSection GetRequiredSection(this IConfiguration configuration, string key) => null;
                                              }
                                          }
                                          namespace Microsoft.Extensions.Options {
                                              public class OptionsBuilder<T> {}
                                              public static class OptionsBuilderExtensions {
                                                  public static OptionsBuilder<T> BindConfiguration<T>(this OptionsBuilder<T> builder, string section) => builder;
                                              }
                                          }
                                          namespace Microsoft.Extensions.DependencyInjection {
                                              using Microsoft.Extensions.Configuration;
                                              using Microsoft.Extensions.Options;
                                              public interface IServiceCollection {}
                                              public static class ServiceCollectionExtensions {
                                                  public static OptionsBuilder<T> AddOptions<T>(this IServiceCollection services) => null;
                                                  public static IServiceCollection Configure<T>(this IServiceCollection services, IConfigurationSection section) => services;
                                              }
                                          }
                                          """;

    /// <summary>
    ///     Tests that the call is rewritten to Configure with GetRequiredSection.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_AddOptionsChainWithConfiguration_RewritesToConfigure(CancellationToken cancellationToken)
    {
        var source = FrameworkStubs + """

                                      namespace MyApp {
                                          using Microsoft.Extensions.Configuration;
                                          using Microsoft.Extensions.DependencyInjection;
                                          public class MyOptions {}
                                          public class Startup {
                                              public void Register(IServiceCollection services, IConfiguration configuration) {
                                                  services.AddOptions<MyOptions>().BindConfiguration("MyOptions");
                                              }
                                          }
                                      }
                                      """;

        var analyzer = new BindConfigurationAnalyzer();
        var provider = new BindConfigurationCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("services.Configure<MyOptions>(configuration.GetRequiredSection(\"MyOptions\"))");
        await Assert.That(fixedSource).DoesNotContain("BindConfiguration(\"MyOptions\")");
    }

    /// <summary>
    ///     Tests that no fix is offered when no configuration is in scope.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountOfferedActions_NoConfigurationInScope_OffersNoFix(CancellationToken cancellationToken)
    {
        var source = FrameworkStubs + """

                                      namespace MyApp {
                                          using Microsoft.Extensions.DependencyInjection;
                                          public class MyOptions {}
                                          public class Startup {
                                              public void Register(IServiceCollection services) {
                                                  services.AddOptions<MyOptions>().BindConfiguration("MyOptions");
                                              }
                                          }
                                      }
                                      """;

        var analyzer = new BindConfigurationAnalyzer();
        var provider = new BindConfigurationCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var offered = await CodeFixTestRunner.CountOfferedActionsAsync(request, cancellationToken);

        await Assert.That(offered).IsEqualTo(0);
    }

    /// <summary>
    ///     Tests that no fix is offered when the call is not an AddOptions chain.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountOfferedActions_StandaloneBuilder_OffersNoFix(CancellationToken cancellationToken)
    {
        var source = FrameworkStubs + """

                                      namespace MyApp {
                                          using Microsoft.Extensions.Configuration;
                                          using Microsoft.Extensions.Options;
                                          public class MyOptions {}
                                          public class Startup {
                                              public void Register(OptionsBuilder<MyOptions> builder, IConfiguration configuration) {
                                                  builder.BindConfiguration("MyOptions");
                                              }
                                          }
                                      }
                                      """;

        var analyzer = new BindConfigurationAnalyzer();
        var provider = new BindConfigurationCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var offered = await CodeFixTestRunner.CountOfferedActionsAsync(request, cancellationToken);

        await Assert.That(offered).IsEqualTo(0);
    }
}
