using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.CodeFixes.LanguageFeatures.ExpressionBodies;

/// <summary>
///     Converts the expression body reported by ATXCS082 into a block body.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ExpressionBodiedDestructorCodeFixProvider))]
[Shared]
public sealed class ExpressionBodiedDestructorCodeFixProvider : CodeFixProvider
{
    private const string Title = "Convert to a block body";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds
    {
        get
        {
            return [DiagnosticIds.CSharp.ExpressionBodiedDestructor];
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
            var declaration = node.FirstAncestorOrSelf<DestructorDeclarationSyntax>()!;
            var action = CodeAction.Create(
                Title,
                cancellationToken => ConvertToBlockAsync(context.Document, declaration, cancellationToken),
                Title);
            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private async Task<Document> ConvertToBlockAsync(
        Document document,
        DestructorDeclarationSyntax declaration,
        CancellationToken cancellationToken)
    {
        var text = await document.GetTextAsync(cancellationToken);
        var expressionBody = declaration.ExpressionBody!;
        var baseIndent = ExpressionBodyBlockBuilder.GetIndentation(text, declaration.SpanStart);
        var lineBreak = ExpressionBodyBlockBuilder.GetLineBreak(text);
        var replacement = ExpressionBodyBlockBuilder.BuildStatementBlock(expressionBody, baseIndent, lineBreak);
        var previousToken = expressionBody.ArrowToken.GetPreviousToken();
        var span = TextSpan.FromBounds(previousToken.Span.End, declaration.SemicolonToken.Span.End);
        return document.WithText(text.Replace(span, replacement));
    }
}