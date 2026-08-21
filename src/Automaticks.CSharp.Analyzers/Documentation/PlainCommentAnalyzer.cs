using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace Automaticks.CSharp.Documentation;

/// <summary>
///     Flags plain single-line (//) and block (/* */) comments.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PlainCommentAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a plain single-line or block comment is found.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static PlainCommentAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.PlainComment,
            "Plain comment is not allowed",
            "Plain comments (//) and block comments (/* */) are forbidden. Remove the comment or, if context is needed, use an XML documentation comment (///). A code fix is available (dotnet format analyzers --diagnostics ATXCS041).",
            "CSharp",
            DiagnosticSeverity.Warning,
            true,
            "Remove the `//` or `/* */` comment. Code must be self-documenting through clear naming and structure. If contextual explanation is genuinely needed for a public or protected API member, convert the comment to an XML documentation comment (`///`). Inline implementation comments that explain *why* non-obvious code exists should instead be expressed by extracting the code into a well-named method.");
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
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [Rule];
        }
    }

    private void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);

        foreach (var trivia in root.DescendantTrivia())
        {
            if ((trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                 trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)) &&
                !HasDocumentationComment(trivia))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, trivia.GetLocation()));
            }
        }
    }

    private bool HasDocumentationComment(SyntaxTrivia trivia)
    {
        var text = trivia.ToString().TrimStart();
        return text.StartsWith("///", StringComparison.Ordinal);
    }
}
