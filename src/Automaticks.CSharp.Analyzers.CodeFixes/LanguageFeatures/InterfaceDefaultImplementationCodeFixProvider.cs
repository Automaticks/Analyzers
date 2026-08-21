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

namespace Automaticks.CSharp.CodeFixes.LanguageFeatures;

/// <summary>
///     Strips the default implementation from the interface member reported by ATXCS061.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InterfaceDefaultImplementationCodeFixProvider))]
[Shared]
public sealed class InterfaceDefaultImplementationCodeFixProvider : CodeFixProvider
{
    private const string Title = "Remove the default implementation";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds
    {
        get
        {
            return [DiagnosticIds.CSharp.InterfaceDefaultImplementation];
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
            var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
            var member = token.Parent!.FirstAncestorOrSelf<MemberDeclarationSyntax>();
            if (member is not MethodDeclarationSyntax && member is not PropertyDeclarationSyntax)
            {
                continue;
            }

            var action = CodeAction.Create(
                Title,
                cancellationToken => RemoveBodyAsync(context.Document, member, cancellationToken),
                Title);
            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private PropertyDeclarationSyntax BuildContractProperty(PropertyDeclarationSyntax property)
    {
        var semicolon = SyntaxFactory.Token(SyntaxKind.SemicolonToken);
        if (property.ExpressionBody is not null)
        {
            var getAccessor = SyntaxFactory
                .AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithSemicolonToken(semicolon);
            var accessors = SyntaxFactory.SingletonList(getAccessor);
            var accessorList = SyntaxFactory.AccessorList(accessors);
            return property
                .WithExpressionBody(null)
                .WithSemicolonToken(default)
                .WithAccessorList(accessorList);
        }

        var rebuilt = new List<AccessorDeclarationSyntax>();
        foreach (var accessor in property.AccessorList!.Accessors)
        {
            rebuilt.Add(accessor.WithBody(null).WithExpressionBody(null).WithSemicolonToken(semicolon));
        }

        var newAccessorList = property.AccessorList.WithAccessors(SyntaxFactory.List(rebuilt));
        return property.WithAccessorList(newAccessorList);
    }

    private async Task<Document> RemoveBodyAsync(
        Document document,
        MemberDeclarationSyntax member,
        CancellationToken cancellationToken)
    {
        var root = (await document.GetSyntaxRootAsync(cancellationToken))!;
        MemberDeclarationSyntax replacement;
        if (member is MethodDeclarationSyntax method)
        {
            replacement = method
                .WithBody(null)
                .WithExpressionBody(null)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }
        else
        {
            replacement = BuildContractProperty((member as PropertyDeclarationSyntax)!);
        }

        var newRoot = root.ReplaceNode(member, replacement.WithTriviaFrom(member));
        return document.WithSyntaxRoot(newRoot);
    }
}
