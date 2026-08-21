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
///     Converts the expression body reported by ATXCS076 into a block body.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ExpressionBodiedLocalFunctionCodeFixProvider))]
[Shared]
public sealed class ExpressionBodiedLocalFunctionCodeFixProvider : CodeFixProvider
{
    private const string Title = "Convert to a block body";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds
    {
        get
        {
            return [DiagnosticIds.CSharp.ExpressionBodiedLocalFunction];
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
        var semanticModel = (await context.Document.GetSemanticModelAsync(context.CancellationToken))!;
        foreach (var diagnostic in context.Diagnostics)
        {
            var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
            var declaration = node.FirstAncestorOrSelf<LocalFunctionStatementSyntax>()!;
            var symbol = (semanticModel.GetDeclaredSymbol(declaration, context.CancellationToken) as IMethodSymbol)!;
            var isReturnWrapped = !ExpressionBodyBlockBuilder.HasVoidLikeReturn(symbol, semanticModel.Compilation);
            var action = CodeAction.Create(
                Title,
                cancellationToken => ConvertToBlockAsync(context.Document, declaration, isReturnWrapped, cancellationToken),
                Title);
            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private async Task<Document> ConvertToBlockAsync(
        Document document,
        LocalFunctionStatementSyntax declaration,
        bool isReturnWrapped,
        CancellationToken cancellationToken)
    {
        var text = await document.GetTextAsync(cancellationToken);
        var expressionBody = declaration.ExpressionBody!;
        var baseIndent = ExpressionBodyBlockBuilder.GetIndentation(text, declaration.SpanStart);
        var lineBreak = ExpressionBodyBlockBuilder.GetLineBreak(text);
        var replacement = isReturnWrapped
            ? ExpressionBodyBlockBuilder.BuildReturnBlock(expressionBody, baseIndent, lineBreak)
            : ExpressionBodyBlockBuilder.BuildStatementBlock(expressionBody, baseIndent, lineBreak);
        var previousToken = expressionBody.ArrowToken.GetPreviousToken();
        var span = TextSpan.FromBounds(previousToken.Span.End, declaration.SemicolonToken.Span.End);
        return document.WithText(text.Replace(span, replacement));
    }
}