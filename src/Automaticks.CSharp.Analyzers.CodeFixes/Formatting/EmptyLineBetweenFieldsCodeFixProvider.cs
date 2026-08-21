using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.CodeFixes.Formatting;

/// <summary>
///     Deletes the blank line between adjacent field declarations reported by ATXCS039.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EmptyLineBetweenFieldsCodeFixProvider))]
[Shared]
public sealed class EmptyLineBetweenFieldsCodeFixProvider : CodeFixProvider
{
    private const string Title = "Remove the blank line between the fields";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds
    {
        get
        {
            return [DiagnosticIds.CSharp.EmptyLineBetweenFields];
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
                cancellationToken => RemoveBlankLineAsync(context.Document, start, cancellationToken),
                Title);
            context.RegisterCodeFix(action, diagnostic);
        }

        return Task.CompletedTask;
    }

    private async Task<Document> RemoveBlankLineAsync(
        Document document,
        int position,
        CancellationToken cancellationToken)
    {
        var text = await document.GetTextAsync(cancellationToken);
        var line = text.Lines.GetLineFromPosition(position);
        var span = TextSpan.FromBounds(line.Start, line.EndIncludingLineBreak);
        var newText = text.Replace(span, string.Empty);
        return document.WithText(newText);
    }
}
