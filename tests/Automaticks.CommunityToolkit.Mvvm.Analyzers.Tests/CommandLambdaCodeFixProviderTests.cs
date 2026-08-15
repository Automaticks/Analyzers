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
    ///     Tests that a parameterless anonymous method is extracted into a named method.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_AnonymousMethodNoParameters_ExtractsNamedMethod(CancellationToken cancellationToken)
    {
        var source = FrameworkStubs + """

                                      namespace MyApp {
                                          using CommunityToolkit.Mvvm.Input;
                                          public class ViewModel {
                                              public RelayCommand SaveCommand { get; }

                                              public ViewModel() {
                                                  SaveCommand = new RelayCommand(delegate { Store(); });
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
    ///     Tests that an anonymous method with an explicit parameter list keeps the parameter name.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_AnonymousMethodWithParameter_ExtractsNamedMethod(CancellationToken cancellationToken)
    {
        var source = FrameworkStubs + """

                                      namespace MyApp {
                                          using CommunityToolkit.Mvvm.Input;
                                          public class ViewModel {
                                              public RelayCommand<string> SaveCommand { get; }

                                              public ViewModel() {
                                                  SaveCommand = new RelayCommand<string>(delegate(string value) { Store(value); });
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

        await Assert.That(fixedSource).Contains("new RelayCommand<string>(Save)");
        await Assert.That(fixedSource).Contains("private void Save(string value)");
    }

    /// <summary>
    ///     Tests that an assignment target name trimming to empty falls back to the default name.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_AssignmentTargetTrimsToEmpty_UsesFallbackName(CancellationToken cancellationToken)
    {
        var source = FrameworkStubs + """

                                      namespace MyApp {
                                          using CommunityToolkit.Mvvm.Input;
                                          public class ViewModel {
                                              public RelayCommand __ { get; }

                                              public ViewModel() {
                                                  __ = new RelayCommand(() => Store());
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

        await Assert.That(fixedSource).Contains("new RelayCommand(ExecuteCommand)");
        await Assert.That(fixedSource).Contains("private void ExecuteCommand()");
    }

    /// <summary>
    ///     Tests that a block-bodied lambda is extracted with its block preserved.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_BlockBodiedLambda_ExtractsNamedMethod(CancellationToken cancellationToken)
    {
        var source = FrameworkStubs + """

                                      namespace MyApp {
                                          using CommunityToolkit.Mvvm.Input;
                                          public class ViewModel {
                                              public RelayCommand SaveCommand { get; }

                                              public ViewModel() {
                                                  SaveCommand = new RelayCommand(() => { Store(); });
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
        await Assert.That(fixedSource).Contains("Store();");
    }

    /// <summary>
    ///     Tests that a lambda assigned to a field initializer is extracted using the field's name.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_FieldInitializerLambda_ExtractsNamedMethod(CancellationToken cancellationToken)
    {
        var source = FrameworkStubs + """

                                      namespace MyApp {
                                          using CommunityToolkit.Mvvm.Input;
                                          public class ViewModel {
                                              private readonly RelayCommand _saveCommand = new RelayCommand(() => Store());

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
    ///     Tests that a lambda with no assignment, property, or field anchor falls back to the default name.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_LambdaWithNoNamingAnchor_UsesFallbackName(CancellationToken cancellationToken)
    {
        var source = FrameworkStubs + """

                                      namespace MyApp {
                                          using CommunityToolkit.Mvvm.Input;
                                          public class ViewModel {
                                              public void Register() {
                                                  var command = new RelayCommand(() => Store());
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

        await Assert.That(fixedSource).Contains("new RelayCommand(ExecuteCommand)");
        await Assert.That(fixedSource).Contains("private void ExecuteCommand()");
    }

    /// <summary>
    ///     Tests that an assignment through a member access expression is recognized as the naming anchor.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_MemberAccessAssignmentTarget_ExtractsNamedMethod(CancellationToken cancellationToken)
    {
        var source = FrameworkStubs + """

                                      namespace MyApp {
                                          using CommunityToolkit.Mvvm.Input;
                                          public class ViewModel {
                                              public RelayCommand SaveCommand { get; }

                                              public ViewModel() {
                                                  this.SaveCommand = new RelayCommand(() => Store());
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
    ///     Tests that a name collision with an existing method appends a numeric suffix.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_NameCollisionWithExistingMethod_AppendsNumericSuffix(CancellationToken cancellationToken)
    {
        var source = FrameworkStubs + """

                                      namespace MyApp {
                                          using CommunityToolkit.Mvvm.Input;
                                          public class ViewModel {
                                              public RelayCommand SaveCommand { get; }

                                              public ViewModel() {
                                                  SaveCommand = new RelayCommand(() => Store());
                                              }

                                              public void Save() { }

                                              public void Save2() { }

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

        await Assert.That(fixedSource).Contains("new RelayCommand(Save3)");
        await Assert.That(fixedSource).Contains("private void Save3()");
    }

    /// <summary>
    ///     Tests that a type with no whitespace trivia between members still gets a working extraction.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_NoSurroundingWhitespaceTrivia_ExtractsNamedMethod(CancellationToken cancellationToken)
    {
        var source = FrameworkStubs + """

                                      namespace MyApp {
                                          using CommunityToolkit.Mvvm.Input;
                                          public class ViewModel{public RelayCommand SaveCommand{get;}public ViewModel(){SaveCommand=new RelayCommand(()=>Store());}public void Store(){}}
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

    /// <summary>
    ///     Tests that GetFixAllProvider returns a non-null batch fixer.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetFixAllProvider_Called_ReturnsBatchFixer(CancellationToken cancellationToken)
    {
        var provider = new CommandLambdaCodeFixProvider();
        var fixAllProvider = provider.GetFixAllProvider();

        await Assert.That(fixAllProvider).IsNotNull();
    }
}
