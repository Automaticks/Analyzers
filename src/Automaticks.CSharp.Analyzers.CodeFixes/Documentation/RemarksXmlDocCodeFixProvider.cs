using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.CodeFixes.Documentation;

/// <summary>
///     Removes the forbidden <c>&lt;remarks&gt;</c> documentation element reported by ATXCS038.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemarksXmlDocCodeFixProvider))]
[Shared]
public sealed class RemarksXmlDocCodeFixProvider : CodeFixProvider
{
    private const string Title = "Remove the <remarks> element";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticIds.CSharp.RemarksXmlDoc];

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
            var node = root.FindNode(diagnostic.Location.SourceSpan, findInsideTrivia: true, getInnermostNodeForTie: true);
            var element = FindRemarksElement(node);
            if (element is null)
            {
                continue;
            }

            var action = CodeAction.Create(
                Title,
                cancellationToken => RemoveElementAsync(context.Document, element, cancellationToken),
                Title);
            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private XmlNodeSyntax? FindRemarksElement(SyntaxNode node)
    {
        var element = node.FirstAncestorOrSelf<XmlElementSyntax>();
        if (element is not null)
        {
            return element;
        }

        return node.FirstAncestorOrSelf<XmlEmptyElementSyntax>();
    }

    private async Task<Document> RemoveElementAsync(
        Document document,
        XmlNodeSyntax element,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken);
        if (root is null)
        {
            return document;
        }

        if (element.Parent is not DocumentationCommentTriviaSyntax docComment)
        {
            return document;
        }

        var index = docComment.Content.IndexOf(element);
        var trimmed = docComment.Content.RemoveAt(index);
        if (index > 0 && trimmed.Count >= index && trimmed[index - 1] is XmlTextSyntax)
        {
            trimmed = trimmed.RemoveAt(index - 1);
        }

        var newDocComment = docComment.WithContent(trimmed);
        var newRoot = root.ReplaceNode(docComment, newDocComment);
        return document.WithSyntaxRoot(newRoot);
    }
}
