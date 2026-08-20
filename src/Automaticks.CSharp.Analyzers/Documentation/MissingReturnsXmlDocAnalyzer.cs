using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.Documentation;

/// <summary>
///     Flags public, protected, and protected-internal methods with a non-void return type that are missing a &lt;returns&gt; XML documentation element.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MissingReturnsXmlDocAnalyzer : DiagnosticAnalyzer
{
    private const string InheritDocTag = "inheritdoc";
    private const string ReturnsTag = "returns";

    /// <summary>
    ///     The diagnostic rule reported when a public or protected non-<c>void</c>
    ///     method is missing a <c>&lt;returns&gt;</c> XML documentation element.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static MissingReturnsXmlDocAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.MissingReturnsXmlDoc,
            "Public non-void method is missing a <returns> XML documentation element",
            "'{0}' has a non-void return type but is missing a <returns> XML documentation element. A code fix is available (dotnet format analyzers --diagnostics ATXCS053).",
            "CSharp",
            DiagnosticSeverity.Warning,
            true,
            "Add a `/// <returns>Description of what is returned.</returns>` element to the existing XML doc comment. Every public, protected, and protected-internal non-void method must document its return value. Alternatively, use `/// <inheritdoc/>` to inherit documentation from a base or interface member.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MethodDeclaration);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var method = (context.Node as MethodDeclarationSyntax)!;
        if (method.ReturnType is PredefinedTypeSyntax predefined &&
            predefined.Keyword.IsKind(SyntaxKind.VoidKeyword))
        {
            return;
        }

        if (!HasReturnsRequirement(method))
        {
            return;
        }

        if (DocumentationCommentText.HasReturnsOrInheritDoc(method))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(Rule, method.Identifier.GetLocation(), method.Identifier.ValueText));
    }

    private bool HasOverrideModifier(MethodDeclarationSyntax method)
    {
        foreach (var modifier in method.Modifiers)
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

    private bool HasPublicOrProtectedAccess(MethodDeclarationSyntax method)
    {
        var hasPrivate = false;
        var hasPublicOrProtected = false;

        foreach (var modifier in method.Modifiers)
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

        return method.Parent is InterfaceDeclarationSyntax;
    }

    private bool HasReturnsRequirement(MethodDeclarationSyntax method)
    {
        var hasReturnsRequirement = !HasOverrideModifier(method) &&
               method.ExplicitInterfaceSpecifier == null &&
               HasPublicOrProtectedAccess(method) &&
               HasPubliclyAccessibleContext(method);
        return hasReturnsRequirement;
    }
}
