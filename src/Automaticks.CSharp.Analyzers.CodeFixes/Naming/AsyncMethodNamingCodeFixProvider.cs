using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.CodeFixes.Naming;

/// <summary>
///     Appends the <c>Async</c> suffix to the method reported by ATXCS003, renaming every
///     reference across the solution.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AsyncMethodNamingCodeFixProvider))]
[Shared]
public sealed class AsyncMethodNamingCodeFixProvider : CodeFixProvider
{
    private const string AsyncSuffix = "Async";
    private const string Title = "Append the Async suffix";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticIds.CSharp.AsyncMethodNaming];

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
        if (root is null)
        {
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
            var method = node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            if (method is null)
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

        var newName = symbol.Name + AsyncSuffix;
        var options = new SymbolRenameOptions();
        return await Renamer.RenameSymbolAsync(solution, symbol, options, newName, cancellationToken);
    }
}
