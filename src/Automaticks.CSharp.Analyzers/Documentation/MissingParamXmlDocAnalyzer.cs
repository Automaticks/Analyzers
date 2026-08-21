using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.Documentation;

/// <summary>
///     Flags public, protected, and protected-internal methods, constructors, delegates, and indexers missing a <c>&lt;param&gt;</c> XML documentation element.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MissingParamXmlDocAnalyzer : DiagnosticAnalyzer
{
    private const string InheritDocTag = "inheritdoc";
    private const string NameAttributeKey = "name";
    private const string ParamTag = "param";

    /// <summary>
    ///     The diagnostic rule reported when a public or protected parameterised member is missing a <c>&lt;param name="…"&gt;</c> element.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static MissingParamXmlDocAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.MissingParamXmlDoc,
            "Public member parameter is missing a <param> XML documentation element",
            "'{0}' is missing a <param name=\"{1}\"> XML documentation element. A code fix is available (dotnet format analyzers --diagnostics ATXCS052).",
            "CSharp",
            DiagnosticSeverity.Warning,
            true,
            "Add a `/// <param name=\"{paramName}\">Description.</param>` element to the existing XML doc comment. Every public, protected, and protected-internal method, constructor, delegate, or indexer must document each parameter. Alternatively, replace the entire XML doc block with `/// <inheritdoc/>` to inherit all documentation from the base or interface.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(
            AnalyzeNode,
            SyntaxKind.MethodDeclaration,
            SyntaxKind.ConstructorDeclaration,
            SyntaxKind.DelegateDeclaration,
            SyntaxKind.IndexerDeclaration);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [Rule];
        }
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var node = context.Node;
        var parameters = GetParameters(node);

        if (parameters.Count == 0)
        {
            return;
        }

        if (!CanRequireDocumentation(node))
        {
            return;
        }

        if (DocumentationCommentText.HasInheritDoc(node))
        {
            return;
        }

        var documentedNames = DocumentationCommentText.CollectParamNames(node);

        foreach (var parameter in parameters)
        {
            var paramName = parameter.Identifier.ValueText;

            if (!documentedNames.Contains(paramName))
            {
                var memberName = GetMemberName(node);
                context.ReportDiagnostic(
                    Diagnostic.Create(Rule, parameter.Identifier.GetLocation(), memberName, paramName));
            }
        }
    }

    private bool CanBePubliclyAccessed(SyntaxNode node)
    {
        var ancestor = node.Parent;

        while (ancestor is BaseTypeDeclarationSyntax ancestorDecl)
        {
            foreach (var modifier in ancestorDecl.Modifiers)
            {
                if (modifier.IsKind(SyntaxKind.PrivateKeyword))
                {
                    return false;
                }
            }

            ancestor = ancestor.Parent;
        }

        return true;
    }

    private bool CanRequireDocumentation(SyntaxNode node)
    {
        var canRequireDocumentation = !HasOverrideModifier(node) &&
               !HasExplicitInterfaceSpecifier(node) &&
               HasPublicOrProtectedAccess(node) &&
               CanBePubliclyAccessed(node);
        return canRequireDocumentation;
    }

    private string GetMemberName(SyntaxNode node)
    {
        if (node is MethodDeclarationSyntax method)
        {
            return method.Identifier.ValueText;
        }

        if (node is ConstructorDeclarationSyntax ctor)
        {
            return ctor.Identifier.ValueText;
        }

        if (node is DelegateDeclarationSyntax delegateDecl)
        {
            return delegateDecl.Identifier.ValueText;
        }

        return "this[]";
    }

    private SyntaxTokenList GetModifiers(SyntaxNode node)
    {
        if (node is BaseMethodDeclarationSyntax methodDecl)
        {
            return methodDecl.Modifiers;
        }

        if (node is DelegateDeclarationSyntax delegateDecl)
        {
            return delegateDecl.Modifiers;
        }

        return (node as IndexerDeclarationSyntax)!.Modifiers;
    }

    private SeparatedSyntaxList<ParameterSyntax> GetParameters(SyntaxNode node)
    {
        if (node is MethodDeclarationSyntax method)
        {
            return method.ParameterList.Parameters;
        }

        if (node is ConstructorDeclarationSyntax ctor)
        {
            return ctor.ParameterList.Parameters;
        }

        if (node is DelegateDeclarationSyntax delegateDecl)
        {
            return delegateDecl.ParameterList.Parameters;
        }

        return (node as IndexerDeclarationSyntax)!.ParameterList.Parameters;
    }

    private bool HasExplicitInterfaceSpecifier(SyntaxNode node)
    {
        if (node is MethodDeclarationSyntax method)
        {
            return method.ExplicitInterfaceSpecifier != null;
        }

        if (node is IndexerDeclarationSyntax indexer)
        {
            return indexer.ExplicitInterfaceSpecifier != null;
        }

        return false;
    }

    private bool HasOverrideModifier(SyntaxNode node)
    {
        var modifiers = GetModifiers(node);
        foreach (var modifier in modifiers)
        {
            if (modifier.IsKind(SyntaxKind.OverrideKeyword))
            {
                return true;
            }
        }

        return false;
    }

    private bool HasPublicOrProtectedAccess(SyntaxNode node)
    {
        var modifiers = GetModifiers(node);
        var hasPrivate = false;
        var hasPublicOrProtected = false;

        foreach (var modifier in modifiers)
        {
            if (modifier.IsKind(SyntaxKind.PrivateKeyword))
            {
                hasPrivate = true;
            }
            else if (modifier.IsKind(SyntaxKind.PublicKeyword) || modifier.IsKind(SyntaxKind.ProtectedKeyword))
            {
                hasPublicOrProtected = true;
            }
        }

        if (hasPrivate)
        {
            return false;
        }

        if (hasPublicOrProtected)
        {
            return true;
        }

        return node.Parent is InterfaceDeclarationSyntax;
    }
}
