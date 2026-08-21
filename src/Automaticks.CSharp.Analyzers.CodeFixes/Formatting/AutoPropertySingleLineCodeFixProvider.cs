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
///     Collapses the multi-line auto-implemented property reported by ATXCS045 onto one line.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AutoPropertySingleLineCodeFixProvider))]
[Shared]
public sealed class AutoPropertySingleLineCodeFixProvider : CodeFixProvider
{
    private const string Title = "Put the property on a single line";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds
    {
        get
        {
            return [DiagnosticIds.CSharp.AutoPropertySingleLine];
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
            var property = node.FirstAncestorOrSelf<PropertyDeclarationSyntax>()!;
            var action = CodeAction.Create(
                Title,
                cancellationToken => MakeSingleLineAsync(context.Document, property, cancellationToken),
                Title);
            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private string CollapseWhitespace(string value)
    {
        var builder = new StringBuilder(value.Length);
        var pendingSpace = false;
        foreach (var character in value)
        {
            if (char.IsWhiteSpace(character))
            {
                pendingSpace = builder.Length > 0;
                continue;
            }

            if (pendingSpace)
            {
                builder.Append(' ');
                pendingSpace = false;
            }

            builder.Append(character);
        }

        return builder.ToString();
    }

    private async Task<Document> MakeSingleLineAsync(
        Document document,
        PropertyDeclarationSyntax property,
        CancellationToken cancellationToken)
    {
        var accessorList = property.AccessorList!;
        var startToken = property.Modifiers.Count > 0
            ? property.Modifiers[0]
            : property.Type.GetFirstToken();
        var span = TextSpan.FromBounds(startToken.SpanStart, accessorList.CloseBraceToken.Span.End);
        var text = await document.GetTextAsync(cancellationToken);
        var collapsed = CollapseWhitespace(text.ToString(span));
        var newText = text.Replace(span, collapsed);
        return document.WithText(newText);
    }
}
