using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.CodeFixes.Formatting;

/// <summary>
///     Removes the empty initializer braces reported by ATXCS060.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ObjectInitializerEmptyBracesCodeFixProvider))]
[Shared]
public sealed class ObjectInitializerEmptyBracesCodeFixProvider : CodeFixProvider
{
    private const string Title = "Remove the empty initializer braces";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds
    {
        get
        {
            return [DiagnosticIds.CSharp.ObjectInitializerEmptyBraces];
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
            var initializer = node.FirstAncestorOrSelf<InitializerExpressionSyntax>();
            if (initializer!.Parent is not ObjectCreationExpressionSyntax creation)
            {
                continue;
            }

            var action = CodeAction.Create(
                Title,
                cancellationToken => RemoveInitializerAsync(context.Document, creation, cancellationToken),
                Title);
            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private async Task<Document> RemoveInitializerAsync(
        Document document,
        ObjectCreationExpressionSyntax creation,
        CancellationToken cancellationToken)
    {
        var root = (await document.GetSyntaxRootAsync(cancellationToken))!;
        var argumentList = creation.ArgumentList ?? SyntaxFactory.ArgumentList();
        var replacement = creation
            .WithType(creation.Type.WithoutTrailingTrivia())
            .WithArgumentList(argumentList)
            .WithInitializer(null)
            .WithTriviaFrom(creation);
        var newRoot = root.ReplaceNode(creation, replacement);
        return document.WithSyntaxRoot(newRoot);
    }
}
