using Automaticks.CSharp.CodeFixes.LanguageFeatures.ExpressionBodies;
using Automaticks.CSharp.LanguageFeatures.ExpressionBodies;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes.LanguageFeatures;

/// <summary>
///     Tests for ExpressionBodiedMethodCodeFixProvider.
/// </summary>
public class ExpressionBodiedMethodCodeFixProviderTests
{
    /// <summary>
    ///     Tests that ApplyFix_AsyncTaskMethod_OmitsReturnStatement.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_AsyncTaskMethod_OmitsReturnStatement(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading.Tasks;

                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                      public async Task RunAsync() => await Bar();

                                      private static Task Bar()
                                      {
                                          return Task.CompletedTask;
                                      }
                                  }
                              }
                              """;

        var analyzer = new ExpressionBodiedMethodAnalyzer();
        var provider = new ExpressionBodiedMethodCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("await Bar();");
        await Assert.That(fixedSource).DoesNotContain("return await Bar();");
    }

    /// <summary>
    ///     Tests that ApplyFix_AsyncTaskOfIntMethod_WrapsInReturnStatement.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_AsyncTaskOfIntMethod_WrapsInReturnStatement(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Threading.Tasks;

                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                      public async Task<int> GetValueAsync() => await LoadAsync();

                                      private static Task<int> LoadAsync()
                                      {
                                          return Task.FromResult(1);
                                      }
                                  }
                              }
                              """;

        var analyzer = new ExpressionBodiedMethodAnalyzer();
        var provider = new ExpressionBodiedMethodCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("return await LoadAsync();");
    }

    /// <summary>
    ///     Tests that ApplyFix_LocalFunctionExpressionBody_WrapsInReturnStatement.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_LocalFunctionExpressionBody_WrapsInReturnStatement(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                      public bool HasValue(int value)
                                      {
                                          bool IsPositive(int candidate) => candidate > 0;
                                          return IsPositive(value);
                                      }
                                  }
                              }
                              """;

        var analyzer = new ExpressionBodiedMethodAnalyzer();
        var provider = new ExpressionBodiedMethodCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("return candidate > 0;");
    }

    /// <summary>
    ///     Tests that ApplyFix_MethodWithXmlDoc_PreservesDocComment.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_MethodWithXmlDoc_PreservesDocComment(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                      /// <summary>
                                      ///     Determines whether the value is positive.
                                      /// </summary>
                                      public bool HasValue(int value) => value > 0;
                                  }
                              }
                              """;

        var analyzer = new ExpressionBodiedMethodAnalyzer();
        var provider = new ExpressionBodiedMethodCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("Determines whether the value is positive.");
        await Assert.That(fixedSource).Contains("return value > 0;");
    }

    /// <summary>
    ///     Tests that ApplyFix_ValueReturningMethod_WrapsInReturnStatement.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_ValueReturningMethod_WrapsInReturnStatement(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp
                              {
                                  public enum SkillRateKind
                                  {
                                      Base,
                                      StaminaMultiplier
                                  }

                                  public class Foo
                                  {
                                      public static bool HasAdded(SkillRateKind kind) =>
                                          kind is not SkillRateKind.Base and not SkillRateKind.StaminaMultiplier;
                                  }
                              }
                              """;

        var analyzer = new ExpressionBodiedMethodAnalyzer();
        var provider = new ExpressionBodiedMethodCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);
        var normalized = fixedSource.Replace("\r\n", "\n");
        const string expectedBlock = "        public static bool HasAdded(SkillRateKind kind)\n        {\n            return kind is not SkillRateKind.Base and not SkillRateKind.StaminaMultiplier;\n        }";

        await Assert.That(normalized).Contains(expectedBlock);
        await Assert.That(normalized).DoesNotContain("=>");
    }

    /// <summary>
    ///     Tests that ApplyFix_VoidMethod_OmitsReturnStatement.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_VoidMethod_OmitsReturnStatement(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                      private int _count;

                                      public void Increment() => _count++;
                                  }
                              }
                              """;

        var analyzer = new ExpressionBodiedMethodAnalyzer();
        var provider = new ExpressionBodiedMethodCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("_count++;");
        await Assert.That(fixedSource).DoesNotContain("return _count++;");
    }

    /// <summary>
    ///     Tests that CountOfferedActions_ForExpressionBodiedMethod_ReportsOne.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountOfferedActions_ForExpressionBodiedMethod_ReportsOne(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                      public bool HasValue(int value) => value > 0;
                                  }
                              }
                              """;

        var analyzer = new ExpressionBodiedMethodAnalyzer();
        var provider = new ExpressionBodiedMethodCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var offeredActions = await CodeFixTestRunner.CountOfferedActionsAsync(request, cancellationToken);

        await Assert.That(offeredActions).IsEqualTo(1);
    }

    /// <summary>
    ///     Tests that GetFixAllProvider_Always_ReturnsBatchFixer.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetFixAllProvider_Always_ReturnsBatchFixer(CancellationToken cancellationToken)
    {
        var provider = new ExpressionBodiedMethodCodeFixProvider();
        var fixAllProvider = provider.GetFixAllProvider();

        await Assert.That(fixAllProvider).IsEqualTo(WellKnownFixAllProviders.BatchFixer);
    }
}
