using Automaticks.CSharp.Formatting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.CodeFixes.Formatting;

/// <summary>
///     Reorders the members of a type into canonical order, settling both ATXCS042 and ATXCS064.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TypeMemberOrderCodeFixProvider))]
[Shared]
public sealed class TypeMemberOrderCodeFixProvider : CodeFixProvider
{
    private const string Title = "Sort the type members into canonical order";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds =>
    [
        DiagnosticIds.CSharp.TypeMemberOrder,
        DiagnosticIds.CSharp.TypeMemberWithinGroupOrder
    ];

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
            var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
            var typeDeclaration = token.Parent?.FirstAncestorOrSelf<TypeDeclarationSyntax>();
            if (typeDeclaration is null)
            {
                continue;
            }

            var action = CodeAction.Create(
                Title,
                cancellationToken => SortMembersAsync(context.Document, typeDeclaration, cancellationToken),
                Title);
            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private List<MemberDeclarationSyntax> BuildOrderedMembers(
        TypeDeclarationSyntax typeDeclaration,
        SemanticModel semanticModel)
    {
        var isInterface = typeDeclaration is InterfaceDeclarationSyntax;
        var ordered = new List<MemberDeclarationSyntax>();
        foreach (var member in typeDeclaration.Members)
        {
            var rank = MemberRankCalculator.Compute(member, isInterface, semanticModel);
            var position = ordered.Count;
            while (position > 0)
            {
                var previous = MemberRankCalculator.Compute(ordered[position - 1], isInterface, semanticModel);
                if (previous.CompareTo(rank) <= 0)
                {
                    break;
                }

                position--;
            }

            ordered.Insert(position, member);
        }

        return ordered;
    }

    private async Task<Document> SortMembersAsync(
        Document document,
        TypeDeclarationSyntax typeDeclaration,
        CancellationToken cancellationToken)
    {
        var root = (await document.GetSyntaxRootAsync(cancellationToken))!;
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
        if (root is null || semanticModel is null)
        {
            return document;
        }

        var ordered = BuildOrderedMembers(typeDeclaration, semanticModel);
        var newTypeDeclaration = typeDeclaration.WithMembers(SyntaxFactory.List(ordered));
        var newRoot = root.ReplaceNode(typeDeclaration, newTypeDeclaration);
        return document.WithSyntaxRoot(newRoot);
    }
}
