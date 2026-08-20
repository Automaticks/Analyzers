using Automaticks.CSharp.CodeFixes.Naming;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes.Naming;

/// <summary>
///     Tests InternalModifierCodeFixProvider against declarations that already carry a modifier.
/// </summary>
public class InternalModifierCodeFixProviderModifierAnchorTests
{
    /// <summary>
    ///     Tests that public is placed ahead of an existing modifier.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFix_StaticClass_AddsPublicBeforeStatic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  static class Helper { }
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

        await Assert.That(fixedSource).Contains("public static class Helper");
    }
}
