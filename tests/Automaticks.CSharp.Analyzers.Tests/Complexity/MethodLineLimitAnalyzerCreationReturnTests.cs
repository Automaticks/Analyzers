using Automaticks.CSharp.Complexity;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Complexity;

/// <summary>
///     Tests which returned creation expressions exempt a long method from the line limit.
/// </summary>
public class MethodLineLimitAnalyzerCreationReturnTests
{
    /// <summary>
    ///     Tests that returning an implicit creation with an initializer is exempt.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ImplicitCreationWithInitializer_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        var diagnostics = await AnalyzeReturnAsync("return new() { Size = 1 };", cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.MethodLineLimit)).IsFalse();
    }

    /// <summary>
    ///     Tests that returning an implicit creation without an initializer is not exempt.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ImplicitCreationWithoutInitializer_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        var diagnostics = await AnalyzeReturnAsync("return new();", cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.MethodLineLimit)).IsTrue();
    }

    /// <summary>
    ///     Tests that returning a named creation without an initializer is not exempt.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NamedCreationWithoutInitializer_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        var diagnostics = await AnalyzeReturnAsync("return new Box();", cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.MethodLineLimit)).IsTrue();
    }

    private async Task<System.Collections.Immutable.ImmutableArray<Microsoft.CodeAnalysis.Diagnostic>> AnalyzeReturnAsync(
        string returnStatement,
        CancellationToken cancellationToken)
    {
        var padding = new StringBuilder();
        for (var index = 0; index < 60; index++)
        {
            padding.Append("            // pad\n");
        }

        var source = "namespace MyApp {\n"
                     + "    public class Box { public int Size { get; set; } }\n"
                     + "    public class Shape {\n"
                     + "        public Box Make() {\n"
                     + padding
                     + "            " + returnStatement + "\n"
                     + "        }\n"
                     + "    }\n"
                     + "}\n";
        var analyzer = new MethodLineLimitAnalyzer();
        return await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);
    }
}
