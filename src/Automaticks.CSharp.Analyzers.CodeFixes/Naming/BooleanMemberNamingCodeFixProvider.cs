using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Rename;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.CodeFixes.Naming;

/// <summary>
///     Renames the boolean field or property reported by ATXCS062 so it starts with <c>is</c>.
///     The rule also accepts <c>allow</c>; the fix offers the <c>is</c> form because it suits
///     most members, and the developer can pick a different name by hand.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BooleanMemberNamingCodeFixProvider))]
[Shared]
public sealed class BooleanMemberNamingCodeFixProvider : CodeFixProvider
{
    private const string Prefix = "is";
    private const string Title = "Prefix the member name with 'is'";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticIds.CSharp.BooleanMemberNaming];

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
            var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
            var declaration = token.Parent;
            if (declaration is null)
            {
                continue;
            }

            var action = CodeAction.Create(
                Title,
                cancellationToken => RenameAsync(context.Document, declaration, cancellationToken),
                Title);
            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private async Task<Solution> RenameAsync(
        Document document,
        SyntaxNode declaration,
        CancellationToken cancellationToken)
    {
        var solution = document.Project.Solution;
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
        if (semanticModel is null)
        {
            return solution;
        }

        var symbol = semanticModel.GetDeclaredSymbol(declaration, cancellationToken);
        if (symbol is null)
        {
            return solution;
        }

        var newName = PrefixedIdentifier.Build(symbol.Name, Prefix);
        var options = new SymbolRenameOptions();
        return await Renamer.RenameSymbolAsync(solution, symbol, options, newName, cancellationToken);
    }
}
