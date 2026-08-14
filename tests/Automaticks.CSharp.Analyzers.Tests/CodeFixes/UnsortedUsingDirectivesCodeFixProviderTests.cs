using Automaticks.CSharp.CodeFixes.Formatting;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes;

/// <summary>
///     Tests for UnsortedUsingDirectivesCodeFixProvider.
/// </summary>
public class UnsortedUsingDirectivesCodeFixProviderTests
{
    /// <summary>
    ///     Tests that alias and static directives keep their original slots.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_AliasAndStaticDirectives_LeavesThemInPlace(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Text;
                              using System;
                              using static System.Math;
                              using Shorthand = System.Console;

                              namespace MyApp {
                                  public class Foo { }
                              }
                              """;

        var analyzer = new UnsortedUsingDirectivesAnalyzer();
        var provider = new UnsortedUsingDirectivesCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("using static System.Math;");
        await Assert.That(fixedSource).Contains("using Shorthand = System.Console;");
    }

    /// <summary>
    ///     Tests that a single application leaves no unsorted directive behind.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_UnsortedDirectives_LeavesNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Text;
                              using System.IO;
                              using System;

                              namespace MyApp {
                                  public class Foo { }
                              }
                              """;

        var analyzer = new UnsortedUsingDirectivesAnalyzer();
        var provider = new UnsortedUsingDirectivesCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);
        var verifyRequest = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = fixedSource
        };
        var remaining = await CodeFixTestRunner.CountFixableAsync(verifyRequest, cancellationToken);

        await Assert.That(remaining).IsEqualTo(0);
    }

    /// <summary>
    ///     Tests that the directives end up in ascending alphabetical order.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_UnsortedDirectives_OrdersAlphabetically(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Text;
                              using System;

                              namespace MyApp {
                                  public class Foo { }
                              }
                              """;

        var analyzer = new UnsortedUsingDirectivesAnalyzer();
        var provider = new UnsortedUsingDirectivesCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);
        var systemIndex = fixedSource.IndexOf("using System;", System.StringComparison.Ordinal);
        var textIndex = fixedSource.IndexOf("using System.Text;", System.StringComparison.Ordinal);

        await Assert.That(systemIndex).IsLessThan(textIndex);
    }

    /// <summary>
    ///     Tests that already sorted directives offer no fix.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountFixable_SortedDirectives_ReportsZero(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              using System.Text;

                              namespace MyApp {
                                  public class Foo { }
                              }
                              """;

        var analyzer = new UnsortedUsingDirectivesAnalyzer();
        var provider = new UnsortedUsingDirectivesCodeFixProvider();
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
