using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Composition;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.CodeFixes.LanguageFeatures;

/// <summary>
///     Converts the expression body reported by ATXCS075 into a block body.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ExpressionBodiedMethodCodeFixProvider))]
[Shared]
public sealed class ExpressionBodiedMethodCodeFixProvider : CodeFixProvider
{
    private const string IndentStep = "    ";
    private const string Title = "Convert to a block body";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticIds.CSharp.ExpressionBodiedMethod];

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);
        if (root is null || semanticModel is null)
        {
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
            if (!HasFlaggedMember(node, out var member))
            {
                continue;
            }

            if (semanticModel.GetDeclaredSymbol(member.Node, context.CancellationToken) is not IMethodSymbol symbol)
            {
                continue;
            }

            var wrapInReturn = !HasVoidLikeReturn(symbol, semanticModel.Compilation);
            var action = CodeAction.Create(
                Title,
                cancellationToken => ConvertToBlockAsync(context.Document, member, wrapInReturn, cancellationToken),
                Title);
            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private string BuildReplacement(ExpressionSyntax expression, bool wrapInReturn, string baseIndent, string lineBreak)
    {
        var builder = new StringBuilder();
        builder.Append(lineBreak).Append(baseIndent).Append('{');
        builder.Append(lineBreak).Append(baseIndent).Append(IndentStep);
        if (wrapInReturn)
        {
            builder.Append("return ");
        }

        builder.Append(expression.ToString()).Append(';');
        builder.Append(lineBreak).Append(baseIndent).Append('}');
        return builder.ToString();
    }

    private async Task<Document> ConvertToBlockAsync(
        Document document,
        FlaggedMember member,
        bool wrapInReturn,
        CancellationToken cancellationToken)
    {
        var text = await document.GetTextAsync(cancellationToken);
        var previousToken = member.ExpressionBody.ArrowToken.GetPreviousToken();
        var baseIndent = GetIndentation(text, member.Node.SpanStart);
        var lineBreak = GetLineBreak(text);
        var replacement = BuildReplacement(member.ExpressionBody.Expression, wrapInReturn, baseIndent, lineBreak);
        var span = TextSpan.FromBounds(previousToken.Span.End, member.SemicolonToken.Span.End);
        var newText = text.Replace(span, replacement);
        return document.WithText(newText);
    }

    private string GetIndentation(SourceText text, int position)
    {
        var line = text.Lines.GetLineFromPosition(position);
        var builder = new StringBuilder();
        for (var offset = line.Start; offset < line.End; offset++)
        {
            var character = text[offset];
            if (character != ' ' && character != '\t')
            {
                break;
            }

            builder.Append(character);
        }

        return builder.ToString();
    }

    private string GetLineBreak(SourceText text)
    {
        foreach (var line in text.Lines)
        {
            if (line.EndIncludingLineBreak > line.End)
            {
                var breakSpan = TextSpan.FromBounds(line.End, line.EndIncludingLineBreak);
                return text.ToString(breakSpan);
            }
        }

        return "\n";
    }

    private bool HasFlaggedMember(SyntaxNode node, out FlaggedMember member)
    {
        var candidate = node.FirstAncestorOrSelf<SyntaxNode>(ancestor => ancestor is MethodDeclarationSyntax or LocalFunctionStatementSyntax);

        if (candidate is MethodDeclarationSyntax { ExpressionBody: not null } method)
        {
            member = new FlaggedMember(method, method.ExpressionBody, method.SemicolonToken);
            return true;
        }

        if (candidate is LocalFunctionStatementSyntax { ExpressionBody: not null } localFunction)
        {
            member = new FlaggedMember(localFunction, localFunction.ExpressionBody, localFunction.SemicolonToken);
            return true;
        }

        member = new FlaggedMember(null!, null!, default);
        return false;
    }

    private bool HasVoidLikeReturn(IMethodSymbol method, Compilation compilation)
    {
        if (method.ReturnsVoid)
        {
            return true;
        }

        if (!method.IsAsync)
        {
            return false;
        }

        var taskType = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
        var valueTaskType = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask");
        return SymbolEqualityComparer.Default.Equals(method.ReturnType, taskType)
            || SymbolEqualityComparer.Default.Equals(method.ReturnType, valueTaskType);
    }

    /// <summary>
    ///     The flagged member's declaration node together with its expression body and semicolon token.
    /// </summary>
    private readonly struct FlaggedMember
    {
        /// <summary>
        ///     Gets the expression body being converted to a block.
        /// </summary>
        public ArrowExpressionClauseSyntax ExpressionBody { get; }

        /// <summary>
        ///     Gets the method or local function declaration that owns the expression body.
        /// </summary>
        public SyntaxNode Node { get; }

        /// <summary>
        ///     Gets the semicolon token that terminates the expression body.
        /// </summary>
        public SyntaxToken SemicolonToken { get; }

        public FlaggedMember(SyntaxNode node, ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
        {
            Node = node;
            ExpressionBody = expressionBody;
            SemicolonToken = semicolonToken;
        }
    }
}
