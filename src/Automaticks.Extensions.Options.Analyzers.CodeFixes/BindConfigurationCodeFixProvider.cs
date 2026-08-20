using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Extensions.Options.CodeFixes;

/// <summary>
///     Rewrites AddOptions&lt;T&gt;().BindConfiguration("X") into Configure&lt;T&gt;(configuration.GetRequiredSection("X")) for ATXEO049.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BindConfigurationCodeFixProvider))]
[Shared]
public sealed class BindConfigurationCodeFixProvider : CodeFixProvider
{
    private const string ConfigurationNamespace = "Microsoft.Extensions.Configuration";
    private const string ConfigurationTypeName = "IConfiguration";
    private const string Title = "Use Configure with GetRequiredSection";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticIds.Options.BindConfiguration];

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);
        if (root is null || semanticModel is null)
        {
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
            var invocation = node.FirstAncestorOrSelf<InvocationExpressionSyntax>();
            if (invocation is null)
            {
                continue;
            }

            var replacement = BuildReplacement(invocation, semanticModel);
            if (replacement is null)
            {
                continue;
            }

            var action = CodeAction.Create(
                Title,
                cancellationToken => ReplaceAsync(context.Document, invocation, replacement, cancellationToken),
                Title);
            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private string? BuildReplacement(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
    {
        if (invocation.Expression is not MemberAccessExpressionSyntax bindAccess)
        {
            return null;
        }

        if (bindAccess.Expression is not InvocationExpressionSyntax addOptions)
        {
            return null;
        }

        if (addOptions.Expression is not MemberAccessExpressionSyntax addOptionsAccess)
        {
            return null;
        }

        if (addOptionsAccess.Name is not GenericNameSyntax { Identifier.ValueText: "AddOptions" } addOptionsName)
        {
            return null;
        }

        if (addOptionsName.TypeArgumentList.Arguments.Count != 1)
        {
            return null;
        }

        if (invocation.ArgumentList.Arguments.Count != 1)
        {
            return null;
        }

        var configurationName = FindConfigurationName(invocation, semanticModel);
        if (configurationName is null)
        {
            return null;
        }

        var services = addOptionsAccess.Expression.ToString();
        var optionsType = addOptionsName.TypeArgumentList.Arguments[0].ToString();
        var section = invocation.ArgumentList.Arguments[0].ToString();
        return $"{services}.Configure<{optionsType}>({configurationName}.GetRequiredSection({section}))";
    }

    private string? FindConfigurationName(SyntaxNode node, SemanticModel semanticModel)
    {
        var method = node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (method is null)
        {
            return null;
        }

        foreach (var parameter in method.ParameterList.Parameters)
        {
            if (parameter.Type is null)
            {
                continue;
            }

            var typeSymbol = semanticModel.GetTypeInfo(parameter.Type).Type;
            if (HasConfigurationType(typeSymbol))
            {
                return parameter.Identifier.ValueText;
            }
        }

        return null;
    }

    private bool HasConfigurationType(ITypeSymbol? typeSymbol)
    {
        if (typeSymbol is null)
        {
            return false;
        }

        if (!string.Equals(typeSymbol.Name, ConfigurationTypeName, StringComparison.Ordinal))
        {
            return false;
        }

        var namespaceName = typeSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        return string.Equals(namespaceName, ConfigurationNamespace, StringComparison.Ordinal);
    }

    private async Task<Document> ReplaceAsync(
        Document document,
        InvocationExpressionSyntax invocation,
        string replacementText,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken);
        if (root is null)
        {
            return document;
        }

        var replacement = SyntaxFactory.ParseExpression(replacementText).WithTriviaFrom(invocation);
        var newRoot = root.ReplaceNode(invocation, replacement);
        return document.WithSyntaxRoot(newRoot);
    }
}
