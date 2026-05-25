using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace Automaticks.CSharp;

/// <summary>
///     Flags public, protected, and protected-internal methods, constructors, delegates,
///     and indexers that are missing a <c>&lt;param&gt;</c> XML documentation element
///     for one or more of their parameters. Override members and members using
///     <c>&lt;inheritdoc/&gt;</c> are exempt.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MissingParamXmlDocAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a public or protected parameterised member
    ///     is missing a <c>&lt;param name="…"&gt;</c> element for one or more parameters.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.CSharp.MissingParamXmlDoc,
        "Public member parameter is missing a <param> XML documentation element",
        "'{0}' is missing a <param name=\"{1}\"> XML documentation element",
        "CSharp",
        DiagnosticSeverity.Warning,
        true,
        "Add a `/// <param name=\"{paramName}\">Description.</param>` element to the existing XML doc comment. Every public, protected, and protected-internal method, constructor, delegate, or indexer must document each parameter. Alternatively, replace the entire XML doc block with `/// <inheritdoc/>` to inherit all documentation from the base or interface.");

    private const string ParamTag = "param";
    private const string InheritDocTag = "inheritdoc";
    private const string NameAttributeKey = "name";

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

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

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var node = context.Node;
        var parameters = GetParameters(node);

        if (parameters.Count == 0)
        {
            return;
        }

        if (!ShouldRequireDocumentation(node))
        {
            return;
        }

        var docComment = GetDocumentationComment(node);

        if (docComment != null && HasInheritDoc(docComment))
        {
            return;
        }

        foreach (var parameter in parameters)
        {
            var paramName = parameter.Identifier.ValueText;

            if (docComment == null || !HasParamElement(docComment, paramName))
            {
                var memberName = GetMemberName(node);
                context.ReportDiagnostic(
                    Diagnostic.Create(Rule, parameter.Identifier.GetLocation(), memberName, paramName));
            }
        }
    }

    private static SeparatedSyntaxList<ParameterSyntax> GetParameters(SyntaxNode node)
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

        if (node is IndexerDeclarationSyntax indexer)
        {
            return indexer.ParameterList.Parameters;
        }

        return SyntaxFactory.SeparatedList<ParameterSyntax>();
    }

    private static bool ShouldRequireDocumentation(SyntaxNode node)
    {
        return !HasOverrideModifier(node) &&
               !IsExplicitInterfaceImplementation(node) &&
               IsPublicOrProtected(node) &&
               IsInPubliclyAccessibleContext(node);
    }

    private static bool HasOverrideModifier(SyntaxNode node)
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

    private static bool IsExplicitInterfaceImplementation(SyntaxNode node)
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

    private static bool IsPublicOrProtected(SyntaxNode node)
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

    private static SyntaxTokenList GetModifiers(SyntaxNode node)
    {
        if (node is BaseMethodDeclarationSyntax methodDecl)
        {
            return methodDecl.Modifiers;
        }

        if (node is DelegateDeclarationSyntax delegateDecl)
        {
            return delegateDecl.Modifiers;
        }

        if (node is IndexerDeclarationSyntax indexer)
        {
            return indexer.Modifiers;
        }

        return SyntaxFactory.TokenList();
    }

    private static DocumentationCommentTriviaSyntax? GetDocumentationComment(SyntaxNode node)
    {
        foreach (var trivia in node.GetLeadingTrivia())
        {
            if (trivia.GetStructure() is DocumentationCommentTriviaSyntax docComment)
            {
                return docComment;
            }
        }

        return null;
    }

    private static bool HasInheritDoc(DocumentationCommentTriviaSyntax docComment)
    {
        foreach (var node in docComment.Content)
        {
            if (node is XmlElementSyntax element &&
                element.StartTag.Name.LocalName.ValueText.Equals(InheritDocTag, StringComparison.Ordinal))
            {
                return true;
            }

            if (node is XmlEmptyElementSyntax emptyElement &&
                emptyElement.Name.LocalName.ValueText.Equals(InheritDocTag, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasParamElement(DocumentationCommentTriviaSyntax docComment, string paramName)
    {
        foreach (var xmlNode in docComment.Content)
        {
            if (xmlNode is XmlElementSyntax element &&
                element.StartTag.Name.LocalName.ValueText.Equals(ParamTag, StringComparison.Ordinal) &&
                GetNameAttributeValue(element.StartTag.Attributes).Equals(paramName, StringComparison.Ordinal))
            {
                return true;
            }

            if (xmlNode is XmlEmptyElementSyntax emptyElement &&
                emptyElement.Name.LocalName.ValueText.Equals(ParamTag, StringComparison.Ordinal) &&
                GetNameAttributeValue(emptyElement.Attributes).Equals(paramName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static string GetNameAttributeValue(SyntaxList<XmlAttributeSyntax> attributes)
    {
        foreach (var attribute in attributes)
        {
            if (attribute is XmlNameAttributeSyntax nameAttr &&
                nameAttr.Name.LocalName.ValueText.Equals(NameAttributeKey, StringComparison.Ordinal))
            {
                return nameAttr.Identifier.Identifier.ValueText;
            }
        }

        return string.Empty;
    }

    private static bool IsInPubliclyAccessibleContext(SyntaxNode node)
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

    private static string GetMemberName(SyntaxNode node)
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

        if (node is IndexerDeclarationSyntax)
        {
            return "this[]";
        }

        return "member";
    }
}
