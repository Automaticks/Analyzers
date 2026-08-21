using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.CodeFixes.LanguageFeatures;

/// <summary>
///     Deletes the no-op discard statement reported by ATXCS068.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NoOpDiscardStatementCodeFixProvider))]
[Shared]
public sealed class NoOpDiscardStatementCodeFixProvider : CodeFixProvider
{
    private const string Title = "Remove the no-op discard statement";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds
    {
        get
        {
            return [DiagnosticIds.CSharp.NoOpDiscardStatement];
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
            var statement = node.FirstAncestorOrSelf<ExpressionStatementSyntax>()!;
            var action = CodeAction.Create(
                Title,
                cancellationToken => RemoveStatementAsync(context.Document, statement, cancellationToken),
                Title);
            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private async Task<Document> RemoveStatementAsync(
        Document document,
        ExpressionStatementSyntax statement,
        CancellationToken cancellationToken)
    {
        var root = (await document.GetSyntaxRootAsync(cancellationToken))!;
        var newRoot = root.RemoveNode(statement, SyntaxRemoveOptions.KeepNoTrivia)!;
        return document.WithSyntaxRoot(newRoot);
    }
}
