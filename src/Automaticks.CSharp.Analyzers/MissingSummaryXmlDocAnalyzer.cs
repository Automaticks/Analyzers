using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace Automaticks.CSharp;

/// <summary>
///     Flags public, protected, and protected-internal members and type declarations
///     that are missing a <c>&lt;summary&gt;</c> or <c>&lt;inheritdoc/&gt;</c> XML
///     documentation comment. Override members are exempt.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MissingSummaryXmlDocAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a public or protected declaration is
    ///     missing a <c>&lt;summary&gt;</c> or <c>&lt;inheritdoc/&gt;</c> XML doc comment.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.CSharp.MissingSummaryXmlDoc,
        "Public member is missing a <summary> XML documentation comment",
        "'{0}' is missing a <summary> XML documentation comment",
        "CSharp",
        DiagnosticSeverity.Warning,
        true,
        "Add a `/// <summary>` XML documentation block immediately before the member or type declaration. Every public, protected, and protected-internal member and type must have a summary. Use `/// <inheritdoc/>` on the member to inherit documentation from a base class or interface instead of writing a new summary.");

    private const string SummaryTag = "summary";
    private const string InheritDocTag = "inheritdoc";

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(
            AnalyzeNode,
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKind.RecordDeclaration,
            SyntaxKind.RecordStructDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.EnumDeclaration,
            SyntaxKind.DelegateDeclaration,
            SyntaxKind.MethodDeclaration,
            SyntaxKind.ConstructorDeclaration,
            SyntaxKind.PropertyDeclaration,
            SyntaxKind.IndexerDeclaration,
            SyntaxKind.FieldDeclaration,
            SyntaxKind.EventFieldDeclaration,
            SyntaxKind.EventDeclaration,
            SyntaxKind.EnumMemberDeclaration);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var node = context.Node;

        if (!ShouldRequireDocumentation(node))
        {
            return;
        }

        var docComment = GetDocumentationComment(node);

        if (docComment != null && HasSummaryOrInheritDoc(docComment))
        {
            return;
        }

        var name = GetMemberName(node);
        context.ReportDiagnostic(Diagnostic.Create(Rule, GetNameLocation(node), name));
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

        if (node is PropertyDeclarationSyntax property)
        {
            return property.ExplicitInterfaceSpecifier != null;
        }

        if (node is EventDeclarationSyntax eventDecl)
        {
            return eventDecl.ExplicitInterfaceSpecifier != null;
        }

        if (node is IndexerDeclarationSyntax indexer)
        {
            return indexer.ExplicitInterfaceSpecifier != null;
        }

        return false;
    }

    private static bool IsPublicOrProtected(SyntaxNode node)
    {
        if (node is EnumMemberDeclarationSyntax)
        {
            return true;
        }

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
        if (node is BaseTypeDeclarationSyntax typeDecl)
        {
            return typeDecl.Modifiers;
        }

        if (node is DelegateDeclarationSyntax delegateDecl)
        {
            return delegateDecl.Modifiers;
        }

        if (node is BaseMethodDeclarationSyntax methodDecl)
        {
            return methodDecl.Modifiers;
        }

        if (node is BasePropertyDeclarationSyntax propertyDecl)
        {
            return propertyDecl.Modifiers;
        }

        if (node is BaseFieldDeclarationSyntax fieldDecl)
        {
            return fieldDecl.Modifiers;
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

    private static bool HasSummaryOrInheritDoc(DocumentationCommentTriviaSyntax docComment)
    {
        foreach (var node in docComment.Content)
        {
            if (node is XmlElementSyntax element)
            {
                var localName = element.StartTag.Name.LocalName.ValueText;
                if (localName.Equals(SummaryTag, StringComparison.Ordinal) ||
                    localName.Equals(InheritDocTag, StringComparison.Ordinal))
                {
                    return true;
                }
            }
            else if (node is XmlEmptyElementSyntax emptyElement)
            {
                var localName = emptyElement.Name.LocalName.ValueText;

                if (localName.Equals(InheritDocTag, StringComparison.Ordinal) ||
                    localName.Equals(SummaryTag, StringComparison.Ordinal))
                {
                    return true;
                }
            }
        }

        return false;
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

    private static Location GetNameLocation(SyntaxNode node)
    {
        if (node is BaseTypeDeclarationSyntax typeDecl)
        {
            return typeDecl.Identifier.GetLocation();
        }

        if (node is DelegateDeclarationSyntax delegateDecl)
        {
            return delegateDecl.Identifier.GetLocation();
        }

        if (node is MethodDeclarationSyntax method)
        {
            return method.Identifier.GetLocation();
        }

        if (node is ConstructorDeclarationSyntax ctor)
        {
            return ctor.Identifier.GetLocation();
        }

        if (node is PropertyDeclarationSyntax property)
        {
            return property.Identifier.GetLocation();
        }

        if (node is IndexerDeclarationSyntax indexer)
        {
            return indexer.ThisKeyword.GetLocation();
        }

        if (node is FieldDeclarationSyntax field && field.Declaration.Variables.Count > 0)
        {
            return field.Declaration.Variables[0].Identifier.GetLocation();
        }

        if (node is EventFieldDeclarationSyntax eventField && eventField.Declaration.Variables.Count > 0)
        {
            return eventField.Declaration.Variables[0].Identifier.GetLocation();
        }

        if (node is EventDeclarationSyntax eventDecl)
        {
            return eventDecl.Identifier.GetLocation();
        }

        if (node is EnumMemberDeclarationSyntax enumMember)
        {
            return enumMember.Identifier.GetLocation();
        }

        return node.GetLocation();
    }

    private static string GetMemberName(SyntaxNode node)
    {
        if (node is BaseTypeDeclarationSyntax typeDecl)
        {
            return typeDecl.Identifier.ValueText;
        }

        if (node is DelegateDeclarationSyntax delegateDecl)
        {
            return delegateDecl.Identifier.ValueText;
        }

        if (node is MethodDeclarationSyntax method)
        {
            return method.Identifier.ValueText;
        }

        if (node is ConstructorDeclarationSyntax ctor)
        {
            return ctor.Identifier.ValueText;
        }

        if (node is PropertyDeclarationSyntax property)
        {
            return property.Identifier.ValueText;
        }

        if (node is IndexerDeclarationSyntax)
        {
            return "this[]";
        }

        if (node is FieldDeclarationSyntax field && field.Declaration.Variables.Count > 0)
        {
            return field.Declaration.Variables[0].Identifier.ValueText;
        }

        if (node is EventFieldDeclarationSyntax eventField && eventField.Declaration.Variables.Count > 0)
        {
            return eventField.Declaration.Variables[0].Identifier.ValueText;
        }

        if (node is EventDeclarationSyntax eventDecl)
        {
            return eventDecl.Identifier.ValueText;
        }

        if (node is EnumMemberDeclarationSyntax enumMember)
        {
            return enumMember.Identifier.ValueText;
        }

        return "member";
    }
}
