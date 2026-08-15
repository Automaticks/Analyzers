using Automaticks.CommunityToolkit.Mvvm.CodeFixes;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CommunityToolkit.Mvvm.Analyzers.Tests;

/// <summary>
///     Tests for CommandLambdaCodeFixProvider.
/// </summary>
public sealed class CommandLambdaCodeFixProviderTests
{
    private const string FrameworkStubs = """
                                          namespace CommunityToolkit.Mvvm.Input {
                                              using System;
                                              public class RelayCommand {
                                                  public RelayCommand(Action execute) { }
                                              }
                                              public class RelayCommand<T> {
                                                  public RelayCommand(Action<T> execute) { }
                                              }
                                          }
                                          """;

    /// <summary>
    ///     Tests that a lambda taking an argument keeps the parameter name.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_GenericCommandLambda_KeepsParameterName(CancellationToken cancellationToken)
    {
        var source = FrameworkStubs + """

                                      namespace MyApp {
                                          using CommunityToolkit.Mvvm.Input;
                                          public class ViewModel {
                                              public RelayCommand<string> SaveCommand { get; }

                                              public ViewModel() {
                                                  SaveCommand = new RelayCommand<string>(value => Store(value));
                                              }

                                              public void Store(string value) { }
                                          }
                                      }
                                      """;

        var analyzer = new CommandLambdaAnalyzer();
        var provider = new CommandLambdaCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("private void Save(string value)");
    }

    /// <summary>
    ///     Tests that the lambda becomes a method group named after the command.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_PropertyCommandLambda_ExtractsNamedMethod(CancellationToken cancellationToken)
    {
        var source = FrameworkStubs + """

                                      namespace MyApp {
                                          using CommunityToolkit.Mvvm.Input;
                                          public class ViewModel {
                                              public RelayCommand SaveCommand { get; }

                                              public ViewModel() {
                                                  SaveCommand = new RelayCommand(() => Store());
                                              }

                                              public void Store() { }
                                          }
                                      }
                                      """;

        var analyzer = new CommandLambdaAnalyzer();
        var provider = new CommandLambdaCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("new RelayCommand(Save)");
        await Assert.That(fixedSource).Contains("private void Save()");
    }

    /// <summary>
    ///     Tests that a method group argument is never offered a fix.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountFixable_MethodGroupArgument_ReportsZero(CancellationToken cancellationToken)
    {
        var source = FrameworkStubs + """

                                      namespace MyApp {
                                          using CommunityToolkit.Mvvm.Input;
                                          public class ViewModel {
                                              public RelayCommand SaveCommand { get; }

                                              public ViewModel() {
                                                  var captured = 3;
                                                  SaveCommand = new RelayCommand(() => Store(captured));
                                              }

                                              public void Store(int value) { }
                                          }
                                      }
                                      """;

        var analyzer = new CommandLambdaAnalyzer();
        var provider = new CommandLambdaCodeFixProvider();
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
    ///     Tests that a lambda capturing a local is deliberately not offered a fix.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountOfferedActions_LambdaCapturingLocal_OffersNoFix(CancellationToken cancellationToken)
    {
        var source = FrameworkStubs + """

                                      namespace MyApp {
                                          using CommunityToolkit.Mvvm.Input;
                                          public class ViewModel {
                                              public RelayCommand SaveCommand { get; }

                                              public ViewModel() {
                                                  SaveCommand = new RelayCommand(Store);
                                              }

                                              public void Store() { }
                                          }
                                      }
                                      """;

        var analyzer = new CommandLambdaAnalyzer();
        var provider = new CommandLambdaCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var count = await CodeFixTestRunner.CountFixableAsync(request, cancellationToken);

        await Assert.That(count).IsEqualTo(0);
    }
}
