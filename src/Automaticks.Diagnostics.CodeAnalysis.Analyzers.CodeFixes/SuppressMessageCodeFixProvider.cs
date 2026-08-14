using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Diagnostics.CodeAnalysis.CodeFixes;

/// <summary>
///     Removes the forbidden <c>[SuppressMessage]</c> attribute reported by ATXDC056.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SuppressMessageCodeFixProvider))]
[Shared]
public sealed class SuppressMessageCodeFixProvider : CodeFixProvider
{
    private const string Title = "Remove the [SuppressMessage] attribute";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticIds.DiagnosticsCodeAnalysis.SuppressMessage];

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
            var attribute = node.FirstAncestorOrSelf<AttributeSyntax>();
            if (attribute is null)
            {
                continue;
            }

            var action = CodeAction.Create(
                Title,
                cancellationToken => RemoveAttributeAsync(context.Document, attribute, cancellationToken),
                Title);
            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private async Task<Document> RemoveAttributeAsync(
        Document document,
        AttributeSyntax attribute,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken);
        if (root is null)
        {
            return document;
        }

        SyntaxNode target;
        if (attribute.Parent is AttributeListSyntax attributeList && attributeList.Attributes.Count == 1)
        {
            target = attributeList;
        }
        else
        {
            target = attribute;
        }

        var newRoot = root.RemoveNode(target, SyntaxRemoveOptions.KeepNoTrivia);
        if (newRoot is null)
        {
            return document;
        }

        return document.WithSyntaxRoot(newRoot);
    }
}
