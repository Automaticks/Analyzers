using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.Documentation;

/// <summary>
///     Flags public, protected, and protected-internal members and type declarations that are missing a &lt;summary&gt; or &lt;inheritdoc/&gt; XML documen...
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MissingSummaryXmlDocAnalyzer : DiagnosticAnalyzer
{
    private const string InheritDocTag = "inheritdoc";
    private const string SummaryTag = "summary";

    /// <summary>
    ///     The diagnostic rule reported when a public or protected declaration is
    ///     missing a <c>&lt;summary&gt;</c> or <c>&lt;inheritdoc/&gt;</c> XML doc comment.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static MissingSummaryXmlDocAnalyzer()
    {
        Rule = new(
            DiagnosticIds.CSharp.MissingSummaryXmlDoc,
            "Public member is missing a <summary> XML documentation comment",
            "'{0}' is missing a <summary> XML documentation comment. A code fix is available (dotnet format analyzers --diagnostics ATXCS051).",
            "CSharp",
            DiagnosticSeverity.Warning,
            true,
            "Add a `/// <summary>` XML documentation block immediately before the member or type declaration. Every public, protected, and protected-internal member and type must have a summary. Use `/// <inheritdoc/>` on the member to inherit documentation from a base class or interface instead of writing a new summary.");
    }

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

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var node = context.Node;

        if (!HasDocumentationRequirement(node))
        {
            return;
        }

        if (DocumentationCommentText.HasSummaryOrInheritDoc(node))
        {
            return;
        }

        var name = GetMemberName(node);
        context.ReportDiagnostic(Diagnostic.Create(Rule, GetNameLocation(node), name));
    }

    private string GetMemberName(SyntaxNode node)
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

        var enumMember = (node as EnumMemberDeclarationSyntax)!;
        return enumMember.Identifier.ValueText;
    }

    private SyntaxTokenList GetModifiers(SyntaxNode node)
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

    private Location GetNameLocation(SyntaxNode node)
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

        var enumMember = (node as EnumMemberDeclarationSyntax)!;
        return enumMember.Identifier.GetLocation();
    }

    private bool HasDocumentationRequirement(SyntaxNode node)
    {
        var hasDocumentationRequirement = !HasOverrideModifier(node) &&
               !HasExplicitInterfaceImplementation(node) &&
               HasPublicOrProtected(node) &&
               HasPubliclyAccessibleContext(node);
        return hasDocumentationRequirement;
    }

    private bool HasExplicitInterfaceImplementation(SyntaxNode node)
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

    private bool HasPubliclyAccessibleContext(SyntaxNode node)
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

    private bool HasPublicOrProtected(SyntaxNode node)
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
}
