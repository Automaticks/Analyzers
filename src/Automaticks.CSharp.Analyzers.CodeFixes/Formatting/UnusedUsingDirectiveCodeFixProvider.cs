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
///     Removes the unused <c>using</c> directive reported by ATXCS048.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UnusedUsingDirectiveCodeFixProvider))]
[Shared]
public sealed class UnusedUsingDirectiveCodeFixProvider : CodeFixProvider
{
    private const string Title = "Remove the unused using directive";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds
    {
        get
        {
            return [DiagnosticIds.CSharp.UnusedUsingDirective];
        }
    }

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
        var newRoot = root.RemoveNode(usingDirective, SyntaxRemoveOptions.KeepNoTrivia)!;
        return document.WithSyntaxRoot(newRoot);
    }
}
