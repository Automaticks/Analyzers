using Automaticks.CSharp.CodeFixes.Naming;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes.Naming;

/// <summary>
///     Tests for BooleanMemberNamingCodeFixProvider.
/// </summary>
public class BooleanMemberNamingCodeFixProviderTests
{
    /// <summary>
    ///     Tests that a camel case field keeps its casing convention.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_CamelCaseField_KeepsCamelCasing(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private bool enabled;
                                  }
                              }
                              """;

        var analyzer = new BooleanMemberNamingAnalyzer();
        var provider = new BooleanMemberNamingCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("isEnabled");
    }

    /// <summary>
    ///     Tests that a property is renamed and references follow.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_PascalCaseProperty_RenamesReferences(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool Enabled { get; set; }

                                      public bool Read() { return Enabled; }
                                  }
                              }
                              """;

        var analyzer = new BooleanMemberNamingAnalyzer();
        var provider = new BooleanMemberNamingCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("public bool IsEnabled { get; set; }");
        await Assert.That(fixedSource).Contains("return IsEnabled;");
    }

    /// <summary>
    ///     Tests that an underscore prefixed field keeps the underscore.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_UnderscoreField_KeepsUnderscore(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private bool _enabled;
                                  }
                              }
                              """;

        var analyzer = new BooleanMemberNamingAnalyzer();
        var provider = new BooleanMemberNamingCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = source
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAsync(request, cancellationToken);

        await Assert.That(fixedSource).Contains("_isEnabled");
    }

    /// <summary>
    ///     Tests that an allowed prefix is never offered a fix.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CountFixable_AllowedPrefix_ReportsZero(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool IsEnabled { get; set; }
                                  }
                              }
                              """;

        var analyzer = new BooleanMemberNamingAnalyzer();
        var provider = new BooleanMemberNamingCodeFixProvider();
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
