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
///     Adds an empty <c>&lt;returns&gt;</c> element for the method reported by ATXCS053.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MissingReturnsXmlDocCodeFixProvider))]
[Shared]
public sealed class MissingReturnsXmlDocCodeFixProvider : CodeFixProvider
{
    private const string Element = "/// <returns></returns>";
    private const string Title = "Add an empty <returns> element";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticIds.CSharp.MissingReturnsXmlDoc];

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
            var method = node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            if (method is null)
            {
                continue;
            }

            var action = CodeAction.Create(
                Title,
                cancellationToken => AddReturnsAsync(context.Document, method, cancellationToken),
                Title);
            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private async Task<Document> AddReturnsAsync(
        Document document,
        MethodDeclarationSyntax method,
        CancellationToken cancellationToken)
    {
        var text = await document.GetTextAsync(cancellationToken);
        var newText = DocumentationBlockEditor.InsertLine(text, method, Element);
        return document.WithText(newText);
    }
}
