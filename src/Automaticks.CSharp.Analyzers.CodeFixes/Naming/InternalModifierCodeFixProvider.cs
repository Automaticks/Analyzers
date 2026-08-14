using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.CodeFixes.Naming;

/// <summary>
///     Replaces the forbidden <c>internal</c> accessibility reported by ATXCS013 with <c>public</c>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InternalModifierCodeFixProvider))]
[Shared]
public sealed class InternalModifierCodeFixProvider : CodeFixProvider
{
    private const string Title = "Make the declaration public";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticIds.CSharp.InternalModifier];

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
            var declaration = node.FirstAncestorOrSelf<MemberDeclarationSyntax>();
            if (declaration is null)
            {
                continue;
            }

            var action = CodeAction.Create(
                Title,
                cancellationToken => MakePublicAsync(context.Document, declaration, cancellationToken),
                Title);
            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private MemberDeclarationSyntax BuildPublicDeclaration(MemberDeclarationSyntax declaration)
    {
        var modifiers = declaration.Modifiers;
        var internalIndex = modifiers.IndexOf(SyntaxKind.InternalKeyword);
        if (internalIndex >= 0)
        {
            var internalToken = modifiers[internalIndex];
            var replacement = SyntaxFactory.Token(SyntaxKind.PublicKeyword).WithTriviaFrom(internalToken);
            return declaration.WithModifiers(modifiers.Replace(internalToken, replacement));
        }

        var anchor = GetAnchorToken(declaration);
        var publicToken = SyntaxFactory
            .Token(SyntaxKind.PublicKeyword)
            .WithLeadingTrivia(anchor.LeadingTrivia)
            .WithTrailingTrivia(SyntaxFactory.Space);
        var stripped = declaration.ReplaceToken(anchor, anchor.WithLeadingTrivia(SyntaxTriviaList.Empty));
        return stripped.WithModifiers(stripped.Modifiers.Insert(0, publicToken));
    }

    private SyntaxToken GetAnchorToken(MemberDeclarationSyntax declaration)
    {
        if (declaration.Modifiers.Count > 0)
        {
            return declaration.Modifiers[0];
        }

        foreach (var token in declaration.ChildTokens())
        {
            return token;
        }

        return declaration.GetFirstToken();
    }

    private async Task<Document> MakePublicAsync(
        Document document,
        MemberDeclarationSyntax declaration,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken);
        if (root is null)
        {
            return document;
        }

        var newDeclaration = BuildPublicDeclaration(declaration);
        var newRoot = root.ReplaceNode(declaration, newDeclaration);
        return document.WithSyntaxRoot(newRoot);
    }
}
