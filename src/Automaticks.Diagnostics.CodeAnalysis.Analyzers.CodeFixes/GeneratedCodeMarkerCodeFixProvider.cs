using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Diagnostics.CodeAnalysis.CodeFixes;

/// <summary>
///     Deletes the generated-code marker comment reported by ATXDC021 so the file is analyzed again.
///     Diagnostics raised for a generated file name are not fixable here, because resolving those
///     means renaming the file.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GeneratedCodeMarkerCodeFixProvider))]
[Shared]
public sealed class GeneratedCodeMarkerCodeFixProvider : CodeFixProvider
{
    private const string Title = "Remove the generated-code marker comment";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds =>
    [
        DiagnosticIds.DiagnosticsCodeAnalysis.GeneratedCodeMarker
    ];

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
            if (!HasCommentTrivia(trivia))
            {
                continue;
            }

            var action = CodeAction.Create(
                Title,
                cancellationToken => RemoveTriviaAsync(context.Document, start, cancellationToken),
                Title);
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

    private bool HasCommentTrivia(SyntaxTrivia trivia)
    {
        return trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
            || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia);
    }

    private async Task<Document> RemoveTriviaAsync(
        Document document,
        int triviaStart,
        CancellationToken cancellationToken)
    {
        var root = (await document.GetSyntaxRootAsync(cancellationToken))!;
        var trivia = root.FindTrivia(triviaStart);
        var token = trivia.Token;
        var leading = token.LeadingTrivia;
        var index = leading.IndexOf(trivia);
        var kept = BuildTriviaWithoutLine(leading, index);
        var newToken = token.WithLeadingTrivia(kept);
        var newRoot = root.ReplaceToken(token, newToken);
        return document.WithSyntaxRoot(newRoot);
    }
}
