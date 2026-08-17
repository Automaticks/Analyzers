using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace Automaticks.Threading.Tasks;

/// <summary>
///     Flags a method or local function that declares a <c>CancellationToken</c> parameter its body
///     never reads. <c>ATXTA008</c> only checks the declared type of the last parameter, so adding a
///     token satisfies that rule even when the token is then ignored and cancellation silently
///     stops working.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UnusedCancellationTokenAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule;

    static UnusedCancellationTokenAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.ThreadingTasks.UnusedCancellationToken,
            "CancellationToken parameter is never used",
            "Parameter '{0}' is a CancellationToken that the body never reads. Forward it to the calls this method makes, or stop accepting it.",
            "Threading.Tasks",
            DiagnosticSeverity.Warning,
            true,
            "Forward the token to every cancellable call the body makes, or call `ThrowIfCancellationRequested()`. A token that is accepted and then dropped satisfies the signature rule while leaving the work impossible to stop.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeLocalFunction, SyntaxKind.LocalFunctionStatement);
        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeLocalFunction(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not LocalFunctionStatementSyntax localFunction)
        {
            return;
        }

        ReportUnusedTokens(context, localFunction.ParameterList, GetBody(localFunction.Body, localFunction.ExpressionBody));
    }

    private void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MethodDeclarationSyntax method)
        {
            return;
        }

        if (HasTestAttribute(method))
        {
            return;
        }

        if (context.SemanticModel.GetDeclaredSymbol(method) is IMethodSymbol symbol && HasInheritedSignature(symbol))
        {
            return;
        }

        ReportUnusedTokens(context, method.ParameterList, GetBody(method.Body, method.ExpressionBody));
    }

    private SyntaxNode? GetBody(BlockSyntax? body, ArrowExpressionClauseSyntax? expressionBody)
    {
        if (body is not null)
        {
            return body;
        }

        return expressionBody;
    }

    private bool HasCancellationTokenType(ParameterSyntax parameter, SemanticModel semanticModel)
    {
        if (parameter.Type is null)
        {
            return false;
        }

        var tokenType = semanticModel.Compilation.GetTypeByMetadataName("System.Threading.CancellationToken");
        if (tokenType is null)
        {
            return false;
        }

        var parameterType = semanticModel.GetTypeInfo(parameter.Type).Type;
        return parameterType is not null && SymbolEqualityComparer.Default.Equals(parameterType, tokenType);
    }

    private bool HasInheritedSignature(IMethodSymbol method)
    {
        if (method.IsOverride || !method.ExplicitInterfaceImplementations.IsEmpty)
        {
            return true;
        }

        foreach (var interfaceType in method.ContainingType.AllInterfaces)
        {
            foreach (var member in interfaceType.GetMembers(method.Name))
            {
                var implementation = method.ContainingType.FindImplementationForInterfaceMember(member);
                if (SymbolEqualityComparer.Default.Equals(implementation, method))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool HasReferenceInBody(ParameterSyntax parameter, SyntaxNode body)
    {
        var name = parameter.Identifier.Text;
        foreach (var node in body.DescendantNodes())
        {
            if (node is IdentifierNameSyntax identifierName && identifierName.Identifier.Text == name)
            {
                return true;
            }
        }

        return false;
    }

    private bool HasTestAttribute(MethodDeclarationSyntax method)
    {
        foreach (var attributeList in method.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                if (HasTestAttributeName(attribute.Name.ToString()))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool HasTestAttributeName(string name)
    {
        var bare = name.EndsWith("Attribute", StringComparison.Ordinal)
            ? name.Substring(0, name.Length - "Attribute".Length)
            : name;
        return bare is "Test" or "Fact" or "Theory" or "TestMethod";
    }

    private void ReportUnusedTokens(SyntaxNodeAnalysisContext context, ParameterListSyntax parameterList, SyntaxNode? body)
    {
        if (body is null)
        {
            return;
        }

        foreach (var parameter in parameterList.Parameters)
        {
            if (!HasCancellationTokenType(parameter, context.SemanticModel))
            {
                continue;
            }

            if (HasReferenceInBody(parameter, body))
            {
                continue;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, parameter.GetLocation(), parameter.Identifier.Text));
        }
    }
}
