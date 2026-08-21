using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.CodeFixes.Documentation;

/// <summary>
///     Inserts the blank line before an XML doc comment required by ATXCS054.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MissingBlankLineBeforeXmlDocCodeFixProvider))]
[Shared]
public sealed class MissingBlankLineBeforeXmlDocCodeFixProvider : CodeFixProvider
{
    private const string Title = "Add a blank line before the XML doc comment";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds
    {
        get
        {
            return [DiagnosticIds.CSharp.MissingBlankLineBeforeXmlDoc];
        }
    }

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc />
    public override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        foreach (var diagnostic in context.Diagnostics)
        {
            var start = diagnostic.Location.SourceSpan.Start;
            var action = CodeAction.Create(
                Title,
                cancellationToken => InsertBlankLineAsync(context.Document, start, cancellationToken),
                Title);
            context.RegisterCodeFix(action, diagnostic);
        }

        return Task.CompletedTask;
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

    private async Task<Document> InsertBlankLineAsync(
        Document document,
        int position,
        CancellationToken cancellationToken)
    {
        var text = await document.GetTextAsync(cancellationToken);
        if (position >= text.Length && text.Length > 0)
        {
            position = text.Length - 1;
        }

        var line = text.Lines.GetLineFromPosition(position);
        var insertionPoint = new TextSpan(line.Start, 0);
        var newText = text.Replace(insertionPoint, GetLineBreak(text));
        return document.WithText(newText);
    }
}
