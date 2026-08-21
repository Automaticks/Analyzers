using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.CodeFixes.LanguageFeatures;

/// <summary>
///     Removes the redundant null guard reported by ATXCS014. The rule reports three shapes:
///     an <c>if</c> guard, a <c>ThrowIfNull</c> call, and a coalesce throw.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RedundantNullCheckCodeFixProvider))]
[Shared]
public sealed class RedundantNullCheckCodeFixProvider : CodeFixProvider
{
    private const string Title = "Remove the redundant null check";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds
    {
        get
        {
            return [DiagnosticIds.CSharp.RedundantNullCheck];
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
            var action = CodeAction.Create(
                Title,
                cancellationToken => RemoveGuardAsync(context.Document, node, cancellationToken),
                Title);
            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private async Task<Document> RemoveGuardAsync(
        Document document,
        SyntaxNode node,
        CancellationToken cancellationToken)
    {
        var root = (await document.GetSyntaxRootAsync(cancellationToken))!;
        if (node is BinaryExpressionSyntax coalesce && coalesce.IsKind(SyntaxKind.CoalesceExpression))
        {
            var replacement = coalesce.Left.WithTriviaFrom(coalesce);
            return document.WithSyntaxRoot(root.ReplaceNode(coalesce, replacement));
        }

        var statement = node.FirstAncestorOrSelf<StatementSyntax>();
        if (statement is null)
        {
            return document;
        }

        var newRoot = root.RemoveNode(statement, SyntaxRemoveOptions.KeepNoTrivia)!;
        return document.WithSyntaxRoot(newRoot);
    }
}
