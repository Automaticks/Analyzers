using Automaticks.CSharp.CodeFixes.Formatting;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes.Formatting;

/// <summary>
///     Tests for TypeMemberOrderCodeFixProvider.
/// </summary>
public class TypeMemberOrderCodeFixProviderTests
{
    /// <summary>
    ///     Tests that a member keeps its documentation when it moves.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_DocumentedMember_KeepsDocumentationAttached(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Work() { }

                                      /// <summary>
                                      ///     The size.
                                      /// </summary>
                                      public int Size { get; set; }
                                  }
                              }
                              """;

        var analyzer = new TypeMemberOrderAnalyzer();
        var provider = new TypeMemberOrderCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);
        var summaryIndex = fixedSource.IndexOf("The size.", System.StringComparison.Ordinal);
        var propertyIndex = fixedSource.IndexOf("public int Size", System.StringComparison.Ordinal);

        await Assert.That(summaryIndex).IsLessThan(propertyIndex);
    }

    /// <summary>
    ///     Tests that a field declared after a method is moved ahead of it.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_FieldAfterMethod_MovesFieldFirst(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Work() { }

                                      private int size;
                                  }
                              }
                              """;

        var analyzer = new TypeMemberOrderAnalyzer();
        var provider = new TypeMemberOrderCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);
        var fieldIndex = fixedSource.IndexOf("private int size;", System.StringComparison.Ordinal);
        var methodIndex = fixedSource.IndexOf("public void Work()", System.StringComparison.Ordinal);

        await Assert.That(fieldIndex).IsLessThan(methodIndex);
    }

    /// <summary>
    ///     Tests that the reordered type no longer reports either ordering rule.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_MisorderedType_LeavesNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Work() { }

                                      public int Size { get; set; }

                                      private const int Limit = 3;

                                      private int size;
                                  }
                              }
                              """;

        var analyzer = new TypeMemberOrderAnalyzer();
        var provider = new TypeMemberOrderCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyAllFixesAsync(request, cancellationToken);
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
    ///     Tests that within group ordering is corrected alphabetically.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_WithinGroupDisorder_SortsAlphabetically(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Zebra() { }

                                      public void Alpha() { }
                                  }
                              }
                              """;

        var analyzer = new TypeMemberOrderAnalyzer();
        var provider = new TypeMemberOrderCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);
        var alphaIndex = fixedSource.IndexOf("public void Alpha()", System.StringComparison.Ordinal);
        var zebraIndex = fixedSource.IndexOf("public void Zebra()", System.StringComparison.Ordinal);

        await Assert.That(alphaIndex).IsLessThan(zebraIndex);
    }

    /// <summary>
    ///     Tests that a correctly ordered type is never offered a fix.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountFixable_CanonicallyOrderedType_ReportsZero(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private const int Limit = 3;

                                      private int size;

                                      public int Size { get; set; }

                                      public void Work() { }
                                  }
                              }
                              """;

        var analyzer = new TypeMemberOrderAnalyzer();
        var provider = new TypeMemberOrderCodeFixProvider();
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
