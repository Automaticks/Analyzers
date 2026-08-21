using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.LanguageFeatures;

/// <summary>
///     Flags namespace-qualified type references and requires the simple type name with a <c>using</c> directive instead.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class QualifiedTypeReferenceAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when the simple name would collide with a different type already visible at that position.
    /// </summary>
    public static readonly DiagnosticDescriptor CollisionRule;

    /// <summary>
    ///     The diagnostic rule reported when the simple name is unambiguous, whether already in scope or resolvable by adding a <c>using</c>.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static QualifiedTypeReferenceAnalyzer()
    {
        var collisionRule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.NamespaceQualifiedTypeReference,
            "Namespace-qualified type reference collides with another type",
            "Namespace-qualified reference to '{0}' cannot be simplified to '{1}' because '{2}' already binds to that name here. Rename '{0}' or '{2}' instead of qualifying.",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "Two different types share the simple name used at this location, so the simple name alone is ambiguous. Qualification must never be used to resolve a naming collision: rename one of the two colliding types so that its simple name is unique, then remove the qualification.");
        CollisionRule = collisionRule;

        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.NamespaceQualifiedTypeReference,
            "Namespace-qualified type reference is forbidden",
            "Namespace-qualified reference to '{0}' is forbidden. Use '{1}' instead. A code fix is available (dotnet format analyzers --diagnostics ATXCS072).",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "Replace the namespace-qualified type reference with the simple type name, adding a `using` directive for the namespace if one is not already present. Qualification hides intent behind noise; the simple name combined with a `using` directive is exactly as unambiguous and far easier to read.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.QualifiedName, SyntaxKind.SimpleMemberAccessExpression);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [CollisionRule, Rule];
        }
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var node = context.Node;

        if (node.IsPartOfStructuredTrivia())
        {
            return;
        }

        if (HasUsingDirectiveAncestor(node) || HasNameOfAncestor(node))
        {
            return;
        }

        if (!HasQualificationParts(node, out var parts))
        {
            return;
        }

        if (context.SemanticModel.GetSymbolInfo(node).Symbol is not INamedTypeSymbol targetType)
        {
            return;
        }

        if (context.SemanticModel.GetSymbolInfo(parts.Left).Symbol is not INamespaceSymbol)
        {
            return;
        }

        var simpleNameText = parts.SimpleName.Identifier.ValueText;
        var visibleSymbols = context.SemanticModel.LookupNamespacesAndTypes(node.SpanStart, null, simpleNameText);
        var targetDisplayName = targetType.ToDisplayString();

        if (HasMatchingType(visibleSymbols, targetType))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, node.GetLocation(), targetDisplayName, simpleNameText));
            return;
        }

        if (visibleSymbols.Length == 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, node.GetLocation(), targetDisplayName, simpleNameText));
            return;
        }

        var collidingDisplayName = visibleSymbols[0].ToDisplayString();
        context.ReportDiagnostic(Diagnostic.Create(
            CollisionRule,
            node.GetLocation(),
            targetDisplayName,
            simpleNameText,
            collidingDisplayName));
    }

    private bool HasMatchingType(ImmutableArray<ISymbol> visibleSymbols, INamedTypeSymbol targetType)
    {
        foreach (var visibleSymbol in visibleSymbols)
        {
            if (visibleSymbol is INamedTypeSymbol namedType
                && SymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, targetType.OriginalDefinition))
            {
                return true;
            }
        }

        return false;
    }

    private bool HasNameOfAncestor(SyntaxNode node)
    {
        foreach (var ancestor in node.Ancestors())
        {
            if (ancestor is InvocationExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.Text: "nameof" } })
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Splits a reference into its left part and simple name for either syntax shape.
    /// </summary>
    private bool HasQualificationParts(SyntaxNode node, out QualificationParts parts)
    {
        switch (node)
        {
            case QualifiedNameSyntax qualifiedName:
                parts = new QualificationParts(qualifiedName.Left, qualifiedName.Right);
                return true;
            case MemberAccessExpressionSyntax memberAccess:
                parts = new QualificationParts(memberAccess.Expression, memberAccess.Name);
                return true;
            default:
                parts = new QualificationParts(null!, null!);
                return false;
        }
    }

    private bool HasUsingDirectiveAncestor(SyntaxNode node)
    {
        return node.FirstAncestorOrSelf<UsingDirectiveSyntax>() is not null;
    }

    /// <summary>
    ///     The left (namespace) part and simple name part of a qualified reference.
    /// </summary>
    private readonly struct QualificationParts
    {
        /// <summary>
        ///     Gets the left-hand side of the qualification, expected to resolve to a namespace.
        /// </summary>
        public SyntaxNode Left { get; }

        /// <summary>
        ///     Gets the simple type name on the right-hand side of the qualification.
        /// </summary>
        public SimpleNameSyntax SimpleName { get; }

        public QualificationParts(SyntaxNode left, SimpleNameSyntax simpleName)
        {
            Left = left;
            SimpleName = simpleName;
        }
    }
}
