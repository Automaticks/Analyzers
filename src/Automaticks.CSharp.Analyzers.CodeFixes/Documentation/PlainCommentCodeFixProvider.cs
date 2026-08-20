using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.CodeFixes.Documentation;

/// <summary>
///     Deletes the plain comment reported by ATXCS041.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PlainCommentCodeFixProvider))]
[Shared]
public sealed class PlainCommentCodeFixProvider : CodeFixProvider
{
    private const string Title = "Remove the comment";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticIds.CSharp.PlainComment];

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = (await context.Document.GetSyntaxRootAsync(context.CancellationToken))!;
        foreach (var diagnostic in context.Diagnostics)
        {
            var start = diagnostic.Location.SourceSpan.Start;
            var trivia = root.FindTrivia(start);
            if (trivia.SpanStart != start)
            {
                continue;
            }

            var action = CodeAction.Create(
                Title,
                cancellationToken => RemoveCommentAsync(context.Document, start, cancellationToken),
                Title);
            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private List<SyntaxTrivia> BuildTriviaWithoutLine(SyntaxTriviaList triviaList, int index)
    {
        var skipFrom = index;
        if (index > 0 && triviaList[index - 1].IsKind(SyntaxKind.WhitespaceTrivia))
        {
            skipFrom = index - 1;
        }

        var skipTo = index;
        if (index + 1 < triviaList.Count && triviaList[index + 1].IsKind(SyntaxKind.EndOfLineTrivia))
        {
            skipTo = index + 1;
        }

        var kept = new List<SyntaxTrivia>();
        for (var position = 0; position < triviaList.Count; position++)
        {
            if (position >= skipFrom && position <= skipTo)
            {
                continue;
            }

            kept.Add(triviaList[position]);
        }

        return kept;
    }

    private async Task<Document> RemoveCommentAsync(
        Document document,
        int triviaStart,
        CancellationToken cancellationToken)
    {
        var root = (await document.GetSyntaxRootAsync(cancellationToken))!;
        var trivia = root.FindTrivia(triviaStart);
        var token = trivia.Token;
        var leadingIndex = token.LeadingTrivia.IndexOf(trivia);
        if (leadingIndex >= 0)
        {
            var keptLeading = BuildTriviaWithoutLine(token.LeadingTrivia, leadingIndex);
            var withLeading = token.WithLeadingTrivia(keptLeading);
            return document.WithSyntaxRoot(root.ReplaceToken(token, withLeading));
        }

        var trailingIndex = token.TrailingTrivia.IndexOf(trivia);
        var keptTrailing = BuildTriviaWithoutLine(token.TrailingTrivia, trailingIndex);
        var withTrailing = token.WithTrailingTrivia(keptTrailing);
        return document.WithSyntaxRoot(root.ReplaceToken(token, withTrailing));
    }
}
