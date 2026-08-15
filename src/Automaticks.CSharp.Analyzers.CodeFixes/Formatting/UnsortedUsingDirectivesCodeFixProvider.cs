using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.CodeFixes.Formatting;

/// <summary>
///     Sorts the <c>using</c> directives flagged as out of order by ATXCS047.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UnsortedUsingDirectivesCodeFixProvider))]
[Shared]
public sealed class UnsortedUsingDirectivesCodeFixProvider : CodeFixProvider
{
    private const string Title = "Sort the using directives alphabetically";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticIds.CSharp.UnsortedUsingDirectives];

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
        if (root is not CompilationUnitSyntax compilationUnit)
        {
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            var action = CodeAction.Create(
                Title,
                cancellationToken => SortDirectivesAsync(context.Document, cancellationToken),
                Title);
            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private SyntaxList<UsingDirectiveSyntax> BuildSortedList(SyntaxList<UsingDirectiveSyntax> usings)
    {
        var sortableIndices = new List<int>();
        var sortable = new List<UsingDirectiveSyntax>();
        for (var index = 0; index < usings.Count; index++)
        {
            if (CanSort(usings[index]))
            {
                sortableIndices.Add(index);
                sortable.Add(usings[index]);
            }
        }

        sortable.Sort((left, right) => string.Compare(
            left.Name?.ToString() ?? string.Empty,
            right.Name?.ToString() ?? string.Empty,
            StringComparison.OrdinalIgnoreCase));

        var rebuilt = new List<UsingDirectiveSyntax>();
        foreach (var directive in usings)
        {
            rebuilt.Add(directive);
        }

        for (var position = 0; position < sortableIndices.Count; position++)
        {
            var targetIndex = sortableIndices[position];
            rebuilt[targetIndex] = sortable[position].WithTriviaFrom(usings[targetIndex]);
        }

        return SyntaxFactory.List(rebuilt);
    }

    private bool CanSort(UsingDirectiveSyntax directive)
    {
        return directive.Alias is null
               && !directive.StaticKeyword.IsKind(SyntaxKind.StaticKeyword)
               && !directive.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword);
    }

    private async Task<Document> SortDirectivesAsync(Document document, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken);
        if (root is not CompilationUnitSyntax compilationUnit)
        {
            return document;
        }

        var sorted = BuildSortedList(compilationUnit.Usings);
        var newRoot = compilationUnit.WithUsings(sorted);
        return document.WithSyntaxRoot(newRoot);
    }
}
