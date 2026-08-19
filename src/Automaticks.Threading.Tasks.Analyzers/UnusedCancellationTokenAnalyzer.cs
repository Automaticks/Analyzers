using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Automaticks.Threading.Tasks;

/// <summary>
///     Flags a method or local function that declares a CancellationToken parameter its body never reads.
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
        context.RegisterCompilationStartAction(RegisterPerCompilation);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeLocalFunction(SyntaxNodeAnalysisContext context, INamedTypeSymbol? tokenType, ConcurrentDictionary<SyntaxTree, bool> aliasTrees)
    {
        var localFunction = (context.Node as LocalFunctionStatementSyntax)!;
        if (!HasCancellationTokenParameterSyntax(localFunction.ParameterList, aliasTrees))
        {
            return;
        }

        var unused = CollectUnusedTokens(context, localFunction.ParameterList, GetBody(localFunction.Body, localFunction.ExpressionBody), tokenType);
        if (unused is null)
        {
            return;
        }

        Report(context, unused);
    }

    private void AnalyzeMethod(SyntaxNodeAnalysisContext context, INamedTypeSymbol? tokenType, ConcurrentDictionary<SyntaxTree, bool> aliasTrees)
    {
        var method = (context.Node as MethodDeclarationSyntax)!;
        if (!HasCancellationTokenParameterSyntax(method.ParameterList, aliasTrees))
        {
            return;
        }

        if (HasTestAttribute(method))
        {
            return;
        }

        var unused = CollectUnusedTokens(context, method.ParameterList, GetBody(method.Body, method.ExpressionBody), tokenType);
        if (unused is null)
        {
            return;
        }

        if (HasInheritedSignature(context.SemanticModel.GetDeclaredSymbol(method)!))
        {
            return;
        }

        Report(context, unused);
    }

    private List<ParameterSyntax>? CollectUnusedTokens(SyntaxNodeAnalysisContext context, ParameterListSyntax parameterList, SyntaxNode? body, INamedTypeSymbol? tokenType)
    {
        if (body is null)
        {
            return null;
        }

        List<ParameterSyntax>? unused = null;
        foreach (var parameter in parameterList.Parameters)
        {
            if (!HasCancellationTokenType(parameter, context.SemanticModel, tokenType))
            {
                continue;
            }

            if (HasReferenceInBody(parameter, body))
            {
                continue;
            }

            if (unused is null)
            {
                var created = new List<ParameterSyntax>();
                unused = created;
            }

            unused.Add(parameter);
        }

        return unused;
    }

    private SyntaxNode? GetBody(BlockSyntax? body, ArrowExpressionClauseSyntax? expressionBody)
    {
        if (body is not null)
        {
            return body;
        }

        return expressionBody;
    }

    private string GetRightmostName(TypeSyntax type)
    {
        var candidate = type;
        if (candidate is NullableTypeSyntax nullable)
        {
            candidate = nullable.ElementType;
        }

        if (candidate is QualifiedNameSyntax qualified)
        {
            return qualified.Right.Identifier.ValueText;
        }

        if (candidate is IdentifierNameSyntax identifier)
        {
            return identifier.Identifier.ValueText;
        }

        return string.Empty;
    }

    private bool HasAliasInTree(SyntaxTree tree)
    {
        var root = tree.GetRoot();

        foreach (var node in root.DescendantNodes(candidate => candidate is CompilationUnitSyntax or BaseNamespaceDeclarationSyntax))
        {
            if (node is UsingDirectiveSyntax { Alias: not null })
            {
                return true;
            }
        }

        return false;
    }

    private bool HasCancellationTokenParameterSyntax(ParameterListSyntax parameterList, ConcurrentDictionary<SyntaxTree, bool> aliasTrees)
    {
        foreach (var parameter in parameterList.Parameters)
        {
            if (parameter.Type is not null && GetRightmostName(parameter.Type) == "CancellationToken")
            {
                return true;
            }
        }

        return HasUsingAlias(parameterList.SyntaxTree, aliasTrees);
    }

    private bool HasCancellationTokenType(ParameterSyntax parameter, SemanticModel semanticModel, INamedTypeSymbol? tokenType)
    {
        if (parameter.Type is null)
        {
            return false;
        }

        var parameterType = semanticModel.GetTypeInfo(parameter.Type).Type;
        return SymbolEqualityComparer.Default.Equals(parameterType, tokenType);
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

    private bool HasUsingAlias(SyntaxTree tree, ConcurrentDictionary<SyntaxTree, bool> aliasTrees)
    {
        if (aliasTrees.TryGetValue(tree, out var hasAlias))
        {
            return hasAlias;
        }

        hasAlias = HasAliasInTree(tree);
        aliasTrees[tree] = hasAlias;
        return hasAlias;
    }

    private void RegisterPerCompilation(CompilationStartAnalysisContext compilationContext)
    {
        var tokenType = compilationContext.Compilation.GetTypeByMetadataName("System.Threading.CancellationToken");
        var aliasTrees = new ConcurrentDictionary<SyntaxTree, bool>();
        compilationContext.RegisterSyntaxNodeAction(
            context => AnalyzeLocalFunction(context, tokenType, aliasTrees),
            SyntaxKind.LocalFunctionStatement);
        compilationContext.RegisterSyntaxNodeAction(
            context => AnalyzeMethod(context, tokenType, aliasTrees),
            SyntaxKind.MethodDeclaration);
    }

    private void Report(SyntaxNodeAnalysisContext context, List<ParameterSyntax> parameters)
    {
        foreach (var parameter in parameters)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, parameter.GetLocation(), parameter.Identifier.Text));
        }
    }
}
