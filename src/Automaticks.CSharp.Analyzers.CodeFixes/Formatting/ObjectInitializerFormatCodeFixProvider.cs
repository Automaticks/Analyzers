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

namespace Automaticks.CSharp.CodeFixes.Formatting;

/// <summary>
///     Rewrites the initializer reported by ATXCS059 with one member per line and the braces
///     on their own lines.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ObjectInitializerFormatCodeFixProvider))]
[Shared]
public sealed class ObjectInitializerFormatCodeFixProvider : CodeFixProvider
{
    private const string IndentStep = "    ";
    private const string Title = "Put each initializer member on its own line";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds
    {
        get
        {
            return [DiagnosticIds.CSharp.ObjectInitializerFormat];
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
            var initializer = node.FirstAncestorOrSelf<InitializerExpressionSyntax>()!;
            var action = CodeAction.Create(
                Title,
                cancellationToken => ReformatAsync(context.Document, initializer, cancellationToken),
                Title);
            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private string BuildReplacement(InitializerExpressionSyntax initializer, string baseIndent, string lineBreak)
    {
        var builder = new StringBuilder();
        builder.Append(lineBreak).Append(baseIndent).Append('{');
        var lastIndex = initializer.Expressions.Count - 1;
        for (var index = 0; index <= lastIndex; index++)
        {
            builder.Append(lineBreak).Append(baseIndent).Append(IndentStep);
            builder.Append(initializer.Expressions[index].ToString());
            if (index < lastIndex)
            {
                builder.Append(',');
            }
        }

        builder.Append(lineBreak).Append(baseIndent).Append('}');
        return builder.ToString();
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

    private async Task<Document> ReformatAsync(
        Document document,
        InitializerExpressionSyntax initializer,
        CancellationToken cancellationToken)
    {
        var text = await document.GetTextAsync(cancellationToken);
        var previousToken = initializer.OpenBraceToken.GetPreviousToken();
        var baseIndent = GetIndentation(text, previousToken.SpanStart);
        var lineBreak = GetLineBreak(text);
        var replacement = BuildReplacement(initializer, baseIndent, lineBreak);
        var span = TextSpan.FromBounds(previousToken.Span.End, initializer.CloseBraceToken.Span.End);
        var newText = text.Replace(span, replacement);
        return document.WithText(newText);
    }
}
