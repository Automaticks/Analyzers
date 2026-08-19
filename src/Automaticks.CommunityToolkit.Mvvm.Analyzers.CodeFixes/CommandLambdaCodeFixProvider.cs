using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CommunityToolkit.Mvvm.CodeFixes;

/// <summary>
///     Extracts the command lambda reported by ATXMV001 into a named method and passes the method group instead.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CommandLambdaCodeFixProvider))]
[Shared]
public sealed class CommandLambdaCodeFixProvider : CodeFixProvider
{
    private const string CommandSuffix = "Command";
    private const string FallbackName = "ExecuteCommand";
    private const string Title = "Extract the lambda into a named method";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticIds.ModelViewViewModel.CommandLambda];

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
            var argument = node.FirstAncestorOrSelf<ArgumentSyntax>();
            var lambda = argument?.Expression as AnonymousFunctionExpressionSyntax
                ?? node.FirstAncestorOrSelf<AnonymousFunctionExpressionSyntax>();
            var typeDeclaration = lambda?.FirstAncestorOrSelf<TypeDeclarationSyntax>();
            if (lambda is null || typeDeclaration is null || HasOuterCapture(lambda, semanticModel))
            {
                continue;
            }

            var action = CodeAction.Create(
                Title,
                cancellationToken => ExtractAsync(context.Document, lambda, cancellationToken),
                Title);
            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private SyntaxTriviaList BuildMemberLeadingTrivia(TypeDeclarationSyntax typeDeclaration)
    {
        foreach (var member in typeDeclaration.Members)
        {
            foreach (var trivia in member.GetLeadingTrivia())
            {
                if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
                {
                    return SyntaxFactory.TriviaList(
                        SyntaxFactory.ElasticCarriageReturnLineFeed,
                        SyntaxFactory.ElasticCarriageReturnLineFeed,
                        trivia);
                }
            }
        }

        return SyntaxFactory.TriviaList(
            SyntaxFactory.ElasticCarriageReturnLineFeed,
            SyntaxFactory.ElasticCarriageReturnLineFeed);
    }

    private string BuildMethodText(
        AnonymousFunctionExpressionSyntax lambda,
        IMethodSymbol invoke,
        SemanticModel semanticModel,
        string name)
    {
        var position = lambda.SpanStart;
        var returnType = invoke.ReturnType.ToMinimalDisplayString(semanticModel, position);
        var parameters = BuildParameterList(lambda, invoke, semanticModel);
        var builder = new StringBuilder();
        builder.Append("private ").Append(returnType).Append(' ').Append(name).Append('(').Append(parameters).Append(')');
        if (lambda.Body is BlockSyntax block)
        {
            builder.Append(block.ToFullString());
            return builder.ToString();
        }

        var isVoid = invoke.ReturnsVoid;
        builder.Append(" { ");
        builder.Append(isVoid ? string.Empty : "return ");
        builder.Append(lambda.Body.ToString()).Append("; }");
        return builder.ToString();
    }

    private string BuildParameterList(
        AnonymousFunctionExpressionSyntax lambda,
        IMethodSymbol invoke,
        SemanticModel semanticModel)
    {
        var names = GetParameterNames(lambda);
        var builder = new StringBuilder();
        for (var index = 0; index < invoke.Parameters.Length; index++)
        {
            if (index > 0)
            {
                builder.Append(", ");
            }

            var typeText = invoke.Parameters[index].Type.ToMinimalDisplayString(semanticModel, lambda.SpanStart);
            var parameterName = index < names.Count ? names[index] : invoke.Parameters[index].Name;
            builder.Append(typeText).Append(' ').Append(parameterName);
        }

        return builder.ToString();
    }

    private async Task<Document> ExtractAsync(
        Document document,
        AnonymousFunctionExpressionSyntax lambda,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken);
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
        var typeDeclaration = lambda.FirstAncestorOrSelf<TypeDeclarationSyntax>();
        if (root is null || semanticModel is null || typeDeclaration is null)
        {
            return document;
        }

        if (semanticModel.GetTypeInfo(lambda, cancellationToken).ConvertedType is not INamedTypeSymbol delegateType ||
            delegateType.DelegateInvokeMethod is not { } invoke)
        {
            return document;
        }

        var name = GetUniqueName(typeDeclaration, GetPreferredName(lambda));
        var methodText = BuildMethodText(lambda, invoke, semanticModel, name);
        if (SyntaxFactory.ParseMemberDeclaration(methodText) is not MemberDeclarationSyntax method)
        {
            return document;
        }

        var methodGroup = SyntaxFactory.IdentifierName(name).WithTriviaFrom(lambda);
        var updatedType = typeDeclaration.ReplaceNode(lambda, methodGroup);
        var spacedMethod = method
            .WithLeadingTrivia(BuildMemberLeadingTrivia(typeDeclaration))
            .WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed);
        updatedType = updatedType.AddMembers(spacedMethod);
        var newRoot = root.ReplaceNode(typeDeclaration, updatedType);
        return document.WithSyntaxRoot(newRoot);
    }

    private string? GetAssignmentTargetName(AnonymousFunctionExpressionSyntax lambda)
    {
        var assignment = lambda.FirstAncestorOrSelf<AssignmentExpressionSyntax>();
        return assignment?.Left switch
        {
            IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.ValueText,
            _ => null
        };
    }

    private List<string> GetParameterNames(AnonymousFunctionExpressionSyntax lambda)
    {
        var names = new List<string>();
        if (lambda is SimpleLambdaExpressionSyntax simple)
        {
            names.Add(simple.Parameter.Identifier.ValueText);
            return names;
        }

        var parameterList = lambda switch
        {
            ParenthesizedLambdaExpressionSyntax parenthesized => parenthesized.ParameterList,
            AnonymousMethodExpressionSyntax anonymous => anonymous.ParameterList,
            _ => null
        };

        if (parameterList is null)
        {
            return names;
        }

        foreach (var parameter in parameterList.Parameters)
        {
            names.Add(parameter.Identifier.ValueText);
        }

        return names;
    }

    private string GetPreferredName(AnonymousFunctionExpressionSyntax lambda)
    {
        var candidate = GetAssignmentTargetName(lambda);
        if (candidate is null)
        {
            var owner = lambda.FirstAncestorOrSelf<PropertyDeclarationSyntax>();
            candidate = owner?.Identifier.ValueText;
        }

        if (candidate is null)
        {
            var field = lambda.FirstAncestorOrSelf<FieldDeclarationSyntax>();
            if (field is not null && field.Declaration.Variables.Count > 0)
            {
                candidate = field.Declaration.Variables[0].Identifier.ValueText;
            }
        }

        if (candidate is null || candidate.Length == 0)
        {
            return FallbackName;
        }

        candidate = candidate.TrimStart('_');
        if (candidate.Length > CommandSuffix.Length && candidate.EndsWith(CommandSuffix, StringComparison.Ordinal))
        {
            candidate = candidate.Substring(0, candidate.Length - CommandSuffix.Length);
        }

        if (candidate.Length == 0)
        {
            return FallbackName;
        }

        return char.ToUpperInvariant(candidate[0]) + candidate.Substring(1);
    }

    private string GetUniqueName(TypeDeclarationSyntax typeDeclaration, string preferred)
    {
        var taken = new HashSet<string>(StringComparer.Ordinal);
        foreach (var member in typeDeclaration.Members)
        {
            if (member is MethodDeclarationSyntax method)
            {
                taken.Add(method.Identifier.ValueText);
            }
        }

        if (!taken.Contains(preferred))
        {
            return preferred;
        }

        var suffix = 2;
        while (taken.Contains(preferred + suffix))
        {
            suffix++;
        }

        return preferred + suffix;
    }

    private bool HasOuterCapture(AnonymousFunctionExpressionSyntax lambda, SemanticModel semanticModel)
    {
        var dataFlow = lambda.Body switch
        {
            BlockSyntax block => semanticModel.AnalyzeDataFlow(block),
            ExpressionSyntax expression => semanticModel.AnalyzeDataFlow(expression),
            _ => null
        };

        if (dataFlow is null || !dataFlow.Succeeded)
        {
            return true;
        }

        var ownParameters = new HashSet<string>(StringComparer.Ordinal);
        foreach (var parameterName in GetParameterNames(lambda))
        {
            ownParameters.Add(parameterName);
        }

        foreach (var symbol in dataFlow.DataFlowsIn)
        {
            if (symbol is IParameterSymbol { IsThis: true })
            {
                continue;
            }

            if (symbol is ILocalSymbol or IParameterSymbol && !ownParameters.Contains(symbol.Name))
            {
                return true;
            }
        }

        return false;
    }
}
