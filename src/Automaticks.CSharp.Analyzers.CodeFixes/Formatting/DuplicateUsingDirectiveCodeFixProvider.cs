using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.CodeFixes.Formatting;

/// <summary>
///     Removes the duplicate <c>using</c> directive reported by ATXCS046.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DuplicateUsingDirectiveCodeFixProvider))]
[Shared]
public sealed class DuplicateUsingDirectiveCodeFixProvider : CodeFixProvider
{
    private const string Title = "Remove the duplicate using directive";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticIds.CSharp.DuplicateUsingDirective];

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
            var usingDirective = node.FirstAncestorOrSelf<UsingDirectiveSyntax>();
            if (usingDirective is null)
            {
                continue;
            }

            var action = CodeAction.Create(
                Title,
                cancellationToken => RemoveDirectiveAsync(context.Document, usingDirective, cancellationToken),
                Title);
            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private async Task<Document> RemoveDirectiveAsync(
        Document document,
        UsingDirectiveSyntax usingDirective,
        CancellationToken cancellationToken)
    {
        var root = (await document.GetSyntaxRootAsync(cancellationToken))!;
        var newRoot = root.RemoveNode(usingDirective, SyntaxRemoveOptions.KeepNoTrivia);
        if (newRoot is null)
        {
            return document;
        }

        return document.WithSyntaxRoot(newRoot);
    }
}
