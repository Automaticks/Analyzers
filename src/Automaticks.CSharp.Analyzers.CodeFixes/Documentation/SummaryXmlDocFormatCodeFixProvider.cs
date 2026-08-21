using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.CodeFixes.Documentation;

/// <summary>
///     Reformats the &lt;summary&gt; block reported by ATXCS050 so its prose starts on a new line indented by four spaces.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SummaryXmlDocFormatCodeFixProvider))]
[Shared]
public sealed class SummaryXmlDocFormatCodeFixProvider : CodeFixProvider
{
    private const string ContentIndent = "    ";
    private const string Title = "Reformat the <summary> block";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds
    {
        get
        {
            return [DiagnosticIds.CSharp.SummaryXmlDocFormat];
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
            var node = root.FindNode(diagnostic.Location.SourceSpan, findInsideTrivia: true, getInnermostNodeForTie: true);
            var element = node.FirstAncestorOrSelf<XmlElementSyntax>();
            if (element?.EndTag is null)
            {
                continue;
            }

            var action = CodeAction.Create(
                Title,
                cancellationToken => ReformatAsync(context.Document, element, cancellationToken),
                Title);
            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private string BuildSummary(List<string> prose, string indent, string lineBreak)
    {
        var builder = new StringBuilder();
        builder.Append(indent).Append("/// <summary>").Append(lineBreak);
        foreach (var line in prose)
        {
            builder.Append(indent).Append("/// ").Append(ContentIndent).Append(line).Append(lineBreak);
        }

        builder.Append(indent).Append("/// </summary>");
        return builder.ToString();
    }

    private List<string> ExtractProse(XmlElementSyntax element)
    {
        var prose = new List<string>();
        var raw = element.Content.ToFullString();
        var lines = raw.Split('\n');
        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim('\r', ' ', '\t');
            if (line.StartsWith("///", StringComparison.Ordinal))
            {
                line = line.Substring(3).Trim();
            }

            if (line.Length > 0)
            {
                prose.Add(line);
            }
        }

        return prose;
    }

    private async Task<Document> ReformatAsync(
        Document document,
        XmlElementSyntax element,
        CancellationToken cancellationToken)
    {
        var endTag = element.EndTag!;
        var text = await document.GetTextAsync(cancellationToken);
        var startLine = text.Lines.GetLineFromPosition(element.StartTag.SpanStart);
        var endLine = text.Lines.GetLineFromPosition(endTag.Span.End);
        var indentSpan = TextSpan.FromBounds(startLine.Start, element.StartTag.SpanStart - 4);
        var indent = indentSpan.Length > 0 ? text.ToString(indentSpan) : string.Empty;
        var lineBreak = startLine.EndIncludingLineBreak > startLine.End
            ? text.ToString(TextSpan.FromBounds(startLine.End, startLine.EndIncludingLineBreak))
            : "\n";
        var replacement = BuildSummary(ExtractProse(element), indent, lineBreak);
        var span = TextSpan.FromBounds(startLine.Start, endLine.End);
        return document.WithText(text.Replace(span, replacement));
    }
}
