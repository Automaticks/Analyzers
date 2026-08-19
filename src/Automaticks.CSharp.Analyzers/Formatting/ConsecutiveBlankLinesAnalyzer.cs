using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.Formatting;

/// <summary>
///     Flags any location in a C# source file where two or more consecutive blank lines appear.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ConsecutiveBlankLinesAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when two or more consecutive blank lines are found anywhere in a file.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static ConsecutiveBlankLinesAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.ConsecutiveBlankLines,
            "Consecutive blank lines are forbidden",
            "Remove the extra blank line — at most one blank line is allowed between any two constructs. A code fix is available (dotnet format analyzers --diagnostics ATXCS044).",
            "Style",
            DiagnosticSeverity.Error,
            true,
            "Remove consecutive blank lines until at most one blank line separates any two constructs. Scan the flagged location and delete the extra empty line(s).");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var sourceText = context.Tree.GetText(context.CancellationToken);
        var consecutiveBlankCount = 0;

        foreach (var line in sourceText.Lines)
        {
            if (string.IsNullOrWhiteSpace(line.ToString()))
            {
                consecutiveBlankCount++;
                if (consecutiveBlankCount == 2)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, Location.Create(context.Tree, line.Span)));
                }
            }
            else
            {
                consecutiveBlankCount = 0;
            }
        }
    }
}
