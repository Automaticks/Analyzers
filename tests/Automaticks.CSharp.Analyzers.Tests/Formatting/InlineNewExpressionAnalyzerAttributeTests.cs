using Automaticks.CSharp.Formatting;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Formatting;

/// <summary>
///     Tests that InlineNewExpressionAnalyzer exempts creations inside attribute arguments.
/// </summary>
public class InlineNewExpressionAnalyzerAttributeTests
{
    /// <summary>
    ///     Tests that Analyze_ArrayCreationInAttributeArgument_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ArrayCreationInAttributeArgument_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public sealed class TagsAttribute : Attribute {
                                      public TagsAttribute(int[] values) { Values = values; }
                                      public int[] Values { get; }
                                  }
                                  [Tags(new int[] { 1, 2 })]
                                  public class Foo { }
                              }
                              """;

        var analyzer = new InlineNewExpressionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.InlineNewExpression)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ImplicitArrayCreationInAttributeArgument_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ImplicitArrayCreationInAttributeArgument_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public sealed class TagsAttribute : Attribute {
                                      public TagsAttribute(int[] values) { Values = values; }
                                      public int[] Values { get; }
                                  }
                                  [Tags(new[] { 1, 2 })]
                                  public class Foo { }
                              }
                              """;

        var analyzer = new InlineNewExpressionAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.InlineNewExpression)).IsFalse();
    }
}
