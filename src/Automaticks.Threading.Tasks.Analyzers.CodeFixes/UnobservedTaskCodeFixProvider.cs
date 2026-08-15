using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Threading.Tasks.CodeFixes;

/// <summary>
///     Awaits the discarded task invocation reported by ATXTA010.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UnobservedTaskCodeFixProvider))]
[Shared]
public sealed class UnobservedTaskCodeFixProvider : CodeFixProvider
{
    private const string Title = "Await the returned task";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticIds.ThreadingTasks.UnobservedTask];

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
            var invocation = node.FirstAncestorOrSelf<InvocationExpressionSyntax>();
            if (invocation is null)
            {
                continue;
            }

            var statement = invocation.FirstAncestorOrSelf<ExpressionStatementSyntax>();
            if (statement is null || !HasAsyncEnclosingFunction(invocation))
            {
                continue;
            }

            var action = CodeAction.Create(
                Title,
                cancellationToken => AwaitInvocationAsync(context.Document, statement, invocation, cancellationToken),
                Title);
            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private async Task<Document> AwaitInvocationAsync(
        Document document,
        ExpressionStatementSyntax statement,
        InvocationExpressionSyntax invocation,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken);
        if (root is null)
        {
            return document;
        }

        var awaitExpression = SyntaxFactory.AwaitExpression(invocation.WithoutTrivia());
        var newStatement = SyntaxFactory
            .ExpressionStatement(awaitExpression)
            .WithTriviaFrom(statement);
        var newRoot = root.ReplaceNode(statement, newStatement);
        return document.WithSyntaxRoot(newRoot);
    }

    private bool HasAsyncEnclosingFunction(SyntaxNode node)
    {
        var current = node.Parent;
        while (current is not null)
        {
            if (current is MethodDeclarationSyntax method)
            {
                return method.Modifiers.IndexOf(SyntaxKind.AsyncKeyword) >= 0;
            }

            if (current is LocalFunctionStatementSyntax localFunction)
            {
                return localFunction.Modifiers.IndexOf(SyntaxKind.AsyncKeyword) >= 0;
            }

            if (current is AnonymousFunctionExpressionSyntax anonymousFunction)
            {
                return anonymousFunction.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword);
            }

            current = current.Parent;
        }

        return false;
    }
}
