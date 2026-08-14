using Automaticks.CSharp.CodeFixes.Naming;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes.Naming;

/// <summary>
///     Tests for InternalModifierCodeFixProvider.
/// </summary>
public class InternalModifierCodeFixProviderTests
{
    /// <summary>
    ///     Tests that repeated application makes every internal declaration public.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyAllFixes_SeveralInternalTypes_MakesEveryOnePublic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  internal class Foo { }

                                  internal class Bar { }
                              }
                              """;

        var analyzer = new InternalModifierAnalyzer();
        var provider = new InternalModifierCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyAllFixesAsync(request, cancellationToken);

        await Assert.That(fixedSource).DoesNotContain("internal");
        await Assert.That(fixedSource).Contains("public class Foo");
        await Assert.That(fixedSource).Contains("public class Bar");
    }

    /// <summary>
    ///     Tests that a type with no access modifier gains an explicit public modifier.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_ImplicitlyInternalType_InsertsPublicModifier(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  class Foo { }
                              }
                              """;

        var analyzer = new InternalModifierAnalyzer();
        var provider = new InternalModifierCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("public class Foo");
    }

    /// <summary>
    ///     Tests that an internal type keeps its attributes when made public.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_ImplicitlyInternalTypeWithAttribute_KeepsAttribute(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  [Obsolete("x")]
                                  class Foo { }
                              }
                              """;

        var analyzer = new InternalModifierAnalyzer();
        var provider = new InternalModifierCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("[Obsolete(\"x\")]");
        await Assert.That(fixedSource).Contains("public class Foo");
    }

    /// <summary>
    ///     Tests that an explicit internal member becomes public.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_InternalMethod_ReplacesModifierWithPublic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      internal void Bar() { }
                                  }
                              }
                              """;

        var analyzer = new InternalModifierAnalyzer();
        var provider = new InternalModifierCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("public void Bar()");
        await Assert.That(fixedSource).DoesNotContain("internal");
    }

    /// <summary>
    ///     Tests that a public declaration is never offered a fix.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountFixable_PublicType_ReportsZero(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo { }
                              }
                              """;

        var analyzer = new InternalModifierAnalyzer();
        var provider = new InternalModifierCodeFixProvider();
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
