using Automaticks.Extensions.Options.Analyzers.Tests.Stubs;
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
    ///     Tests that no fix is offered when the AddOptions-shaped call has the wrong generic arity.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountOfferedActions_AddOptionsWithWrongArity_OffersNoFix(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace Microsoft.Extensions.Options {
                                  public class OptionsBuilder<T> {
                                      public OptionsBuilder<T> BindConfiguration(string section) => this;
                                  }
                              }
                              namespace MyApp {
                                  public class MyOptions {}
                                  public static class FakeExtensions {
                                      public static Microsoft.Extensions.Options.OptionsBuilder<T1> AddOptions<T1, T2>(this object services) {
                                          return new Microsoft.Extensions.Options.OptionsBuilder<T1>();
                                      }
                                  }
                                  public class Startup {
                                      public void Configure(object services) {
                                          services.AddOptions<MyOptions, string>().BindConfiguration("MyOptions");
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
    ///     Tests that no fix is offered when the AddOptions-shaped call itself has no receiver.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountOfferedActions_BareAddOptionsChainedCall_OffersNoFix(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace Microsoft.Extensions.Options {
                                  public class OptionsBuilder<T> {
                                      public OptionsBuilder<T> BindConfiguration(string section) => this;
                                  }
                              }
                              namespace MyApp {
                                  public class MyOptions {}
                                  public class Startup {
                                      public Microsoft.Extensions.Options.OptionsBuilder<MyOptions> GetBuilder() {
                                          return new Microsoft.Extensions.Options.OptionsBuilder<MyOptions>();
                                      }
                                      public void Configure() {
                                          GetBuilder().BindConfiguration("MyOptions");
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
    ///     Tests that no fix is offered when the diagnostic sits on a bare invocation without a receiver.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountOfferedActions_BareInvocationDiagnostic_OffersNoFix(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Startup {
                                      public void BindConfiguration(string section) {}
                                      public void Register() {
                                          BindConfiguration("section");
                                      }
                                  }
                              }
                              """;

        var analyzer = new BindConfigurationAtBareInvocationAnalyzer();
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
    ///     Tests that no fix is offered when the diagnostic does not sit on any invocation.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountOfferedActions_DiagnosticNotOnInvocation_OffersNoFix(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Startup {
                                  }
                              }
                              """;

        var analyzer = new BindConfigurationAtClassDeclarationAnalyzer();
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
    ///     Tests that no fix is offered when the call is inside a constructor rather than a method.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountOfferedActions_InvocationInsideConstructor_OffersNoFix(CancellationToken cancellationToken)
    {
        var source = FrameworkStubs + """

                                      namespace MyApp {
                                          using Microsoft.Extensions.Configuration;
                                          using Microsoft.Extensions.DependencyInjection;
                                          public class MyOptions {}
                                          public class Startup {
                                              public Startup(IServiceCollection services, IConfiguration configuration) {
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
    ///     Tests that no fix is offered when a parameter in the enclosing method has no syntactic type.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountOfferedActions_MalformedParameterMissingType_OffersNoFix(CancellationToken cancellationToken)
    {
        var source = FrameworkStubs + """

                                      namespace MyApp {
                                          using Microsoft.Extensions.DependencyInjection;
                                          public class MyOptions {}
                                          public class Startup {
                                              public void Register(IServiceCollection services, configuration) {
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
    ///     Tests that no fix is offered when the AddOptions-shaped call is a plain (non-generic) member access.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountOfferedActions_NonGenericReceiverCall_OffersNoFix(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace Microsoft.Extensions.Options {
                                  public class OptionsBuilder<T> {
                                      public OptionsBuilder<T> BindConfiguration(string section) => this;
                                  }
                              }
                              namespace MyApp {
                                  public class MyOptions {}
                                  public class BuilderProvider {
                                      public Microsoft.Extensions.Options.OptionsBuilder<MyOptions> GetBuilder() {
                                          return new Microsoft.Extensions.Options.OptionsBuilder<MyOptions>();
                                      }
                                  }
                                  public class Startup {
                                      public void Configure(BuilderProvider provider) {
                                          provider.GetBuilder().BindConfiguration("MyOptions");
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
    ///     Tests that no fix is offered when the receiver comes from a call that is not named AddOptions.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountOfferedActions_ReceiverNotFromAddOptionsCall_OffersNoFix(CancellationToken cancellationToken)
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

    /// <summary>
    ///     Tests that no fix is offered when the configuration parameter's type does not resolve.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountOfferedActions_UndeclaredConfigurationParameterType_OffersNoFix(CancellationToken cancellationToken)
    {
        var source = FrameworkStubs + """

                                      namespace MyApp {
                                          using Microsoft.Extensions.DependencyInjection;
                                          public class MyOptions {}
                                          public class Startup {
                                              public void Register(IServiceCollection services, UndeclaredConfigType configuration) {
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
    ///     Tests that no fix is offered when BindConfiguration is called with the wrong argument count.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountOfferedActions_ZeroArgumentCall_OffersNoFix(CancellationToken cancellationToken)
    {
        var source = FrameworkStubs + """

                                      namespace MyApp {
                                          using Microsoft.Extensions.Configuration;
                                          using Microsoft.Extensions.DependencyInjection;
                                          public class MyOptions {}
                                          public class Startup {
                                              public void Register(IServiceCollection services, IConfiguration configuration) {
                                                  services.AddOptions<MyOptions>().BindConfiguration();
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
    ///     Tests that GetFixAllProvider returns a non-null batch fixer.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetFixAllProvider_Called_ReturnsBatchFixer(CancellationToken cancellationToken)
    {
        var provider = new BindConfigurationCodeFixProvider();
        var fixAllProvider = provider.GetFixAllProvider();

        await Assert.That(fixAllProvider).IsNotNull();
    }
}
