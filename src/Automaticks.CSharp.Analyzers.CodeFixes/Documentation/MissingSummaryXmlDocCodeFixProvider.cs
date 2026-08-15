using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Composition;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.CodeFixes.Documentation;

/// <summary>
///     Inserts an empty <c>&lt;summary&gt;</c> documentation block for the member reported by
///     ATXCS051. The block is deliberately left blank so the fix supplies structure without
///     claiming the member has been described.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MissingSummaryXmlDocCodeFixProvider))]
[Shared]
public sealed class MissingSummaryXmlDocCodeFixProvider : CodeFixProvider
{
    private const string ContentPrefix = "///     ";
    private const string SummaryClose = "/// </summary>";
    private const string SummaryOpen = "/// <summary>";
    private const string Title = "Add an empty <summary> documentation block";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticIds.CSharp.MissingSummaryXmlDoc];

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
            var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
            var member = node.FirstAncestorOrSelf<MemberDeclarationSyntax>();
            if (member is null)
            {
                continue;
            }

            var action = CodeAction.Create(
                Title,
                cancellationToken => InsertSummaryAsync(context.Document, member, cancellationToken),
                Title);
            context.RegisterCodeFix(action, diagnostic);
        }
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

    private async Task<Document> InsertSummaryAsync(
        Document document,
        MemberDeclarationSyntax member,
        CancellationToken cancellationToken)
    {
        var text = await document.GetTextAsync(cancellationToken);
        var line = text.Lines.GetLineFromPosition(member.SpanStart);
        var indentSpan = TextSpan.FromBounds(line.Start, member.SpanStart);
        var indent = text.ToString(indentSpan);
        var lineBreak = GetLineBreak(text);
        var builder = new StringBuilder();
        builder.Append(indent).Append(SummaryOpen).Append(lineBreak);
        builder.Append(indent).Append(ContentPrefix).Append(lineBreak);
        builder.Append(indent).Append(SummaryClose).Append(lineBreak);
        var insertionPoint = new TextSpan(line.Start, 0);
        var newText = text.Replace(insertionPoint, builder.ToString());
        return document.WithText(newText);
    }
}
