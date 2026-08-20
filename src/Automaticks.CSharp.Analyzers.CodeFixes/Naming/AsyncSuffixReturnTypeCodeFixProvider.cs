using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.CodeFixes.Naming;

/// <summary>
///     Strips the Async suffix from the method reported by ATXCS009.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AsyncSuffixReturnTypeCodeFixProvider))]
[Shared]
public sealed class AsyncSuffixReturnTypeCodeFixProvider : CodeFixProvider
{
    private const string AsyncSuffix = "Async";
    private const string Title = "Remove the Async suffix";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticIds.CSharp.AsyncSuffixReturnType];

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
            var method = node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            if (method is null)
            {
                continue;
            }

            var name = method.Identifier.ValueText;
            if (name.Length <= AsyncSuffix.Length || !name.EndsWith(AsyncSuffix, StringComparison.Ordinal))
            {
                continue;
            }

            var action = CodeAction.Create(
                Title,
                cancellationToken => RenameAsync(context.Document, method, cancellationToken),
                Title);
            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private async Task<Solution> RenameAsync(
        Document document,
        MethodDeclarationSyntax method,
        CancellationToken cancellationToken)
    {
        var solution = document.Project.Solution;
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
        if (semanticModel is null)
        {
            return solution;
        }

        var symbol = semanticModel.GetDeclaredSymbol(method, cancellationToken);
        if (symbol is null)
        {
            return solution;
        }

        var newName = symbol.Name.Substring(0, symbol.Name.Length - AsyncSuffix.Length);
        var options = new SymbolRenameOptions();
        return await Renamer.RenameSymbolAsync(solution, symbol, options, newName, cancellationToken);
    }
}
