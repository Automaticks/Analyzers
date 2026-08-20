using Automaticks.CommunityToolkit.Mvvm.Analyzers.Tests.Stubs;
using Automaticks.CommunityToolkit.Mvvm.CodeFixes;
using Microsoft.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CommunityToolkit.Mvvm.Analyzers.Tests;

/// <summary>
///     Tests that CommandLambdaCodeFixProvider covers its trivia, return-type, and parameter-count branches.
/// </summary>
public sealed class CommandLambdaCodeFixProviderBranchesTests
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
    ///     Tests that a parameterless anonymous method targeting a delegate with a parameter uses the delegate's parameter name.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_AnonymousMethodOmittingParametersWithTypedDelegate_UsesDelegateParameterName(CancellationToken cancellationToken)
    {
        var source = FrameworkStubs + """

                                      namespace MyApp {
                                          using CommunityToolkit.Mvvm.Input;
                                          public class ViewModel {
                                              public RelayCommand<string> SaveCommand { get; }

                                              public ViewModel() {
                                                  SaveCommand = new RelayCommand<string>(delegate { Store(); });
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

        await Assert.That(fixedSource).Contains("new RelayCommand<string>(Save)");
        await Assert.That(fixedSource).Contains("private void Save(string obj)");
    }

    /// <summary>
    ///     Tests that a first member preceded by a blank line still yields a properly spaced extraction.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_BlankLineBeforeFirstMember_ExtractsNamedMethod(CancellationToken cancellationToken)
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
    ///     Tests that a lambda used directly in an expression-bodied property is named after the property.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_ExpressionBodiedPropertyLambda_ExtractsNamedMethod(CancellationToken cancellationToken)
    {
        var source = FrameworkStubs + """

                                      namespace MyApp {
                                          using CommunityToolkit.Mvvm.Input;
                                          public class ViewModel {
                                              public RelayCommand SaveCommand => new RelayCommand(() => Store());

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
    ///     Tests that a lambda referencing a static field (not a captured local or parameter) is still extracted.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_LambdaReferencingStaticField_ExtractsNamedMethod(CancellationToken cancellationToken)
    {
        var source = FrameworkStubs + """

                                      namespace MyApp {
                                          using CommunityToolkit.Mvvm.Input;
                                          public class ViewModel {
                                              private static int _counter;

                                              public RelayCommand SaveCommand { get; }

                                              public ViewModel() {
                                                  SaveCommand = new RelayCommand(() => Store(_counter));
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
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("new RelayCommand(Save)");
        await Assert.That(fixedSource).Contains("private void Save()");
        await Assert.That(fixedSource).Contains("Store(_counter)");
    }

    /// <summary>
    ///     Tests that an expression-bodied lambda whose delegate returns a value is extracted with a return statement.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_NonVoidExpressionBodiedLambda_ExtractsNamedMethod(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace CommunityToolkit.Mvvm.Input {
                                  using System;
                                  public class RelayCommand {
                                      public RelayCommand(Func<int> execute) { }
                                  }
                              }

                              namespace MyApp {
                                  using CommunityToolkit.Mvvm.Input;
                                  public class ViewModel {
                                      public RelayCommand SaveCommand { get; }

                                      public ViewModel() {
                                          SaveCommand = new RelayCommand(() => GetValue());
                                      }

                                      public int GetValue() => 42;
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
        await Assert.That(fixedSource).Contains("private int Save()");
        await Assert.That(fixedSource).Contains("return GetValue();");
    }

    /// <summary>
    ///     Tests that a lambda taking two parameters keeps both parameter names, separated by a comma.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_TwoParameterLambda_KeepsBothParameterNames(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace CommunityToolkit.Mvvm.Input {
                                  using System;
                                  public class RelayCommand<T> {
                                      public RelayCommand(Action<T, int> execute) { }
                                  }
                              }

                              namespace MyApp {
                                  using CommunityToolkit.Mvvm.Input;
                                  public class ViewModel {
                                      public RelayCommand<string> SaveCommand { get; }

                                      public ViewModel() {
                                          SaveCommand = new RelayCommand<string>((value, count) => Store(value, count));
                                      }

                                      public void Store(string value, int count) { }
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
        await Assert.That(fixedSource).Contains("private void Save(string value, int count)");
    }

    /// <summary>
    ///     Tests that no fix is offered when the diagnostic has no enclosing lambda at all.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountOfferedActions_DiagnosticWithNoLambdaAncestor_OffersNoFix(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class ViewModel {
                                  }
                              }
                              """;

        var analyzer = new CommandLambdaAtClassDeclarationAnalyzer();
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
    ///     Tests that no fix is offered when the lambda sits in top-level statements outside any type declaration.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountOfferedActions_LambdaOutsideAnyTypeDeclaration_OffersNoFix(CancellationToken cancellationToken)
    {
        const string source = """
                              using CommunityToolkit.Mvvm.Input;

                              var command = new RelayCommand(() => Store());

                              void Store() { }

                              namespace CommunityToolkit.Mvvm.Input {
                                  using System;
                                  public class RelayCommand {
                                      public RelayCommand(Action execute) { }
                                  }
                              }
                              """;

        var analyzer = new CommandLambdaAnalyzer();
        var provider = new CommandLambdaCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source,
            OutputKind = OutputKind.ConsoleApplication
        };
        var offered = await CodeFixTestRunner.CountOfferedActionsAsync(request, cancellationToken);

        await Assert.That(offered).IsEqualTo(0);
    }
}
