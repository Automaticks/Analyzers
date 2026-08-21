using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.CodeFixes.Formatting;

/// <summary>
///     Inserts the blank line beside a property or indexer required by ATXCS040.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SingleBlankLineBetweenPropertiesCodeFixProvider))]
[Shared]
public sealed class SingleBlankLineBetweenPropertiesCodeFixProvider : CodeFixProvider
{
    private const string Title = "Add a blank line between the members";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds =>
        [DiagnosticIds.CSharp.SingleBlankLineBetweenProperties];

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
            var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
            var member = node.FirstAncestorOrSelf<MemberDeclarationSyntax>()!;
            var action = CodeAction.Create(
                Title,
                cancellationToken => InsertBlankLineAsync(context.Document, member, cancellationToken),
                Title);
            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private int GetBlockStart(SourceText text, MemberDeclarationSyntax member)
    {
        var position = member.SpanStart;
        foreach (var trivia in member.GetLeadingTrivia())
        {
            var isDocOrCommentTrivia = trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia) ||
                trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                trivia.IsKind(SyntaxKind.MultiLineCommentTrivia);
            if (isDocOrCommentTrivia)
            {
                position = trivia.SpanStart;
                break;
            }
        }

        return text.Lines.GetLineFromPosition(position).Start;
    }

    private string GetLineBreak(SourceText text)
    {
        foreach (var line in text.Lines)
        {
            if (line.EndIncludingLineBreak > line.End)
            {
                var breakSpan = TextSpan.FromBounds(line.End, line.EndIncludingLineBreak);
                return text.ToString(breakSpan);
            }
        }

        return "\n";
    }

    private async Task<Document> InsertBlankLineAsync(
        Document document,
        MemberDeclarationSyntax member,
        CancellationToken cancellationToken)
    {
        var text = await document.GetTextAsync(cancellationToken);
        var blockStart = GetBlockStart(text, member);
        var insertionPoint = new TextSpan(blockStart, 0);
        var newText = text.Replace(insertionPoint, GetLineBreak(text));
        return document.WithText(newText);
    }
}
