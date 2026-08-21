using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
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
            if (!HasFlaggedMember(node, semanticModel, context.CancellationToken, out var member))
            {
                continue;
            }

            var action = CodeAction.Create(
                Title,
                cancellationToken => ConvertToBlockAsync(context.Document, member, cancellationToken),
                Title);
            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private string BuildReplacement(FlaggedMember member, string baseIndent, string lineBreak)
    {
        var builder = new StringBuilder();
        var bodyIndent = member.IsGetterWrapped ? baseIndent + IndentStep + IndentStep : baseIndent + IndentStep;
        builder.Append(lineBreak).Append(baseIndent).Append('{');
        if (member.IsGetterWrapped)
        {
            builder.Append(lineBreak).Append(baseIndent).Append(IndentStep).Append("get");
            builder.Append(lineBreak).Append(baseIndent).Append(IndentStep).Append('{');
        }

        builder.Append(lineBreak).Append(bodyIndent);
        if (member.IsReturnWrapped)
        {
            builder.Append("return ");
        }

        builder.Append(member.ExpressionBody.Expression.ToString()).Append(';');
        if (member.IsGetterWrapped)
        {
            builder.Append(lineBreak).Append(baseIndent).Append(IndentStep).Append('}');
        }

        builder.Append(lineBreak).Append(baseIndent).Append('}');
        return builder.ToString();
    }

    private bool CanWrapInReturn(SyntaxNode owner, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        if (owner is ConstructorDeclarationSyntax or DestructorDeclarationSyntax)
        {
            return false;
        }

        if (owner is AccessorDeclarationSyntax accessor)
        {
            return accessor.Keyword.IsKind(SyntaxKind.GetKeyword);
        }

        if (owner is PropertyDeclarationSyntax or IndexerDeclarationSyntax)
        {
            return true;
        }

        var symbol = semanticModel.GetDeclaredSymbol(owner, cancellationToken) as IMethodSymbol;
        return symbol is not null && !HasVoidLikeReturn(symbol, semanticModel.Compilation);
    }

    private async Task<Document> ConvertToBlockAsync(
        Document document,
        FlaggedMember member,
        CancellationToken cancellationToken)
    {
        var text = await document.GetTextAsync(cancellationToken);
        var previousToken = member.ExpressionBody.ArrowToken.GetPreviousToken();
        var baseIndent = GetIndentation(text, member.Node.SpanStart);
        var lineBreak = GetLineBreak(text);
        var replacement = BuildReplacement(member, baseIndent, lineBreak);
        var span = TextSpan.FromBounds(previousToken.Span.End, GetSemicolonToken(member.Node).Span.End);
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

    private SyntaxToken GetSemicolonToken(SyntaxNode owner)
    {
        if (owner is MethodDeclarationSyntax method)
        {
            return method.SemicolonToken;
        }

        if (owner is LocalFunctionStatementSyntax localFunction)
        {
            return localFunction.SemicolonToken;
        }

        if (owner is PropertyDeclarationSyntax property)
        {
            return property.SemicolonToken;
        }

        if (owner is IndexerDeclarationSyntax indexer)
        {
            return indexer.SemicolonToken;
        }

        if (owner is OperatorDeclarationSyntax operatorDeclaration)
        {
            return operatorDeclaration.SemicolonToken;
        }

        if (owner is ConversionOperatorDeclarationSyntax conversionOperator)
        {
            return conversionOperator.SemicolonToken;
        }

        if (owner is ConstructorDeclarationSyntax constructor)
        {
            return constructor.SemicolonToken;
        }

        if (owner is DestructorDeclarationSyntax destructor)
        {
            return destructor.SemicolonToken;
        }

        var accessor = (owner as AccessorDeclarationSyntax)!;
        return accessor.SemicolonToken;
    }

    private bool HasFlaggedMember(SyntaxNode node, SemanticModel semanticModel, CancellationToken cancellationToken, out FlaggedMember member)
    {
        var arrow = node.FirstAncestorOrSelf<ArrowExpressionClauseSyntax>();
        if (arrow is null)
        {
            member = default;
            return false;
        }

        var owner = arrow.Parent!;
        var isReturnWrapped = CanWrapInReturn(owner, semanticModel, cancellationToken);
        member = new FlaggedMember(owner, arrow, isReturnWrapped, HasGetterWrapper(owner));
        return true;
    }

    private bool HasGetterWrapper(SyntaxNode owner)
    {
        return owner is PropertyDeclarationSyntax or IndexerDeclarationSyntax;
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
        ///     Gets a value indicating whether the block must be wrapped in a get accessor.
        /// </summary>
        public bool IsGetterWrapped { get; }

        /// <summary>
        ///     Gets a value indicating whether the expression must be preceded by return.
        /// </summary>
        public bool IsReturnWrapped { get; }

        /// <summary>
        ///     Gets the declaration that owns the expression body.
        /// </summary>
        public SyntaxNode Node { get; }

        public FlaggedMember(SyntaxNode node, ArrowExpressionClauseSyntax expressionBody, bool isReturnWrapped, bool isGetterWrapped)
        {
            Node = node;
            ExpressionBody = expressionBody;
            IsReturnWrapped = isReturnWrapped;
            IsGetterWrapped = isGetterWrapped;
        }
    }
}
