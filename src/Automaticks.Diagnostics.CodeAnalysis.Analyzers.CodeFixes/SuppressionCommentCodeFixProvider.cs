using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Diagnostics.CodeAnalysis.CodeFixes;

/// <summary>
///     Deletes the forbidden suppression directives reported by ATXDC018 and ATXDC019.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SuppressionCommentCodeFixProvider))]
[Shared]
public sealed class SuppressionCommentCodeFixProvider : CodeFixProvider
{
    private const string PragmaTitle = "Remove the #pragma warning disable directive";
    private const string ReSharperTitle = "Remove the // ReSharper disable comment";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds =>
    [
        DiagnosticIds.DiagnosticsCodeAnalysis.SuppressionPragma,
        DiagnosticIds.DiagnosticsCodeAnalysis.SuppressionReSharper
    ];

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
        if (root is null)
        {
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            var start = diagnostic.Location.SourceSpan.Start;
            var trivia = root.FindTrivia(start);
            if (trivia.SpanStart != start)
            {
                continue;
            }

            var title = GetTitle(diagnostic.Id);
            var action = CodeAction.Create(
                title,
                cancellationToken => RemoveTriviaAsync(context.Document, start, cancellationToken),
                title);
            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private List<SyntaxTrivia> BuildTriviaWithoutLine(SyntaxTriviaList leading, int index)
    {
        var skipFrom = index;
        if (index > 0 && leading[index - 1].IsKind(SyntaxKind.WhitespaceTrivia))
        {
            skipFrom = index - 1;
        }

        var skipTo = index;
        if (index + 1 < leading.Count && leading[index + 1].IsKind(SyntaxKind.EndOfLineTrivia))
        {
            skipTo = index + 1;
        }

        var kept = new List<SyntaxTrivia>();
        for (var position = 0; position < leading.Count; position++)
        {
            if (position >= skipFrom && position <= skipTo)
            {
                continue;
            }

            kept.Add(leading[position]);
        }

        return kept;
    }

    private string GetTitle(string diagnosticId)
    {
        if (string.Equals(diagnosticId, DiagnosticIds.DiagnosticsCodeAnalysis.SuppressionPragma, StringComparison.Ordinal))
        {
            return PragmaTitle;
        }

        return ReSharperTitle;
    }

    private async Task<Document> RemoveTriviaAsync(
        Document document,
        int triviaStart,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken);
        if (root is null)
        {
            return document;
        }

        var trivia = root.FindTrivia(triviaStart);
        var token = trivia.Token;
        var leading = token.LeadingTrivia;
        var index = leading.IndexOf(trivia);
        if (index < 0)
        {
            var withoutTrivia = root.ReplaceTrivia(trivia, []);
            return document.WithSyntaxRoot(withoutTrivia);
        }

        var kept = BuildTriviaWithoutLine(leading, index);
        var newToken = token.WithLeadingTrivia(kept);
        var newRoot = root.ReplaceToken(token, newToken);
        return document.WithSyntaxRoot(newRoot);
    }
}
