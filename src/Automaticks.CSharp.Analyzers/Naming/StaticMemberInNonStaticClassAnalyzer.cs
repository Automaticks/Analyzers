using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.Naming;

/// <summary>
///     Flags static fields and static properties declared in a non-static class.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class StaticMemberInNonStaticClassAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The <c>.editorconfig</c> key listing type names exempt from this rule.
    /// </summary>
    public const string ExcludedTypesKey = "automaticks.static_member_excluded_types";
    private static readonly DiagnosticDescriptor Rule;
    private readonly string[] DefaultExcludedTypes;

    static StaticMemberInNonStaticClassAnalyzer()
    {
        Rule = new(
            DiagnosticIds.CSharp.StaticMemberInNonStaticClass,
            "Static fields and properties must only exist in static classes",
            "{0} '{1}' is static but is declared in non-static class '{2}'. Move it to a dedicated static class, or exempt its type through '" + ExcludedTypesKey + "'.",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "Move the static member to a dedicated `public static class`. Static state in an instantiable class cannot participate in dependency injection and is hard to substitute in tests. Types that are inherently static configuration, such as Roslyn's `DiagnosticDescriptor`, are exempt by default and the list is configurable through `automaticks.static_member_excluded_types` in `.editorconfig`. Constants declared with `const` are not reported.");
    }

    /// <summary>
    ///     Initializes the lookup tables used during analysis.
    /// </summary>
    public StaticMemberInNonStaticClassAnalyzer()
    {
        DefaultExcludedTypes = ["DiagnosticDescriptor", "SuppressionDescriptor"];
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeField, SyntaxKind.FieldDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeProperty, SyntaxKind.PropertyDeclaration);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeField(SyntaxNodeAnalysisContext context)
    {
        var field = (context.Node as FieldDeclarationSyntax)!;
        if (!HasStaticMemberInNonStaticClass(field.Modifiers, field.Parent, out var containingClass))
        {
            return;
        }

        if (HasExcludedType(context, field.Declaration.Type))
        {
            return;
        }

        foreach (var declarator in field.Declaration.Variables)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Rule,
                declarator.Identifier.GetLocation(),
                "Field",
                declarator.Identifier.Text,
                containingClass.Identifier.Text));
        }
    }

    private void AnalyzeProperty(SyntaxNodeAnalysisContext context)
    {
        var property = (context.Node as PropertyDeclarationSyntax)!;
        if (!HasStaticMemberInNonStaticClass(property.Modifiers, property.Parent, out var containingClass))
        {
            return;
        }

        if (HasExcludedType(context, property.Type))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            Rule,
            property.Identifier.GetLocation(),
            "Property",
            property.Identifier.Text,
            containingClass.Identifier.Text));
    }

    /// <summary>
    ///     Reduces a type reference to its bare name, so <c>ImmutableHashSet&lt;string&gt;</c> and
    ///     <c>System.Text.RegularExpressions.Regex</c> match the configured entries.
    /// </summary>
    private string GetSimpleTypeName(TypeSyntax type)
    {
        var current = type;

        while (true)
        {
            switch (current)
            {
                case NullableTypeSyntax nullable:
                    current = nullable.ElementType;
                    continue;
                case ArrayTypeSyntax array:
                    current = array.ElementType;
                    continue;
                case QualifiedNameSyntax qualified:
                    current = qualified.Right;
                    continue;
                case GenericNameSyntax generic:
                    return generic.Identifier.ValueText;
                case IdentifierNameSyntax identifier:
                    return identifier.Identifier.ValueText;
                default:
                    return current.ToString();
            }
        }
    }

    private bool HasExcludedType(SyntaxNodeAnalysisContext context, TypeSyntax type)
    {
        var excluded = ConfigurableTypeList.Read(context, ExcludedTypesKey, DefaultExcludedTypes);
        return excluded.Contains(GetSimpleTypeName(type));
    }

    private bool HasStaticMemberInNonStaticClass(
        SyntaxTokenList modifiers,
        SyntaxNode? parent,
        out ClassDeclarationSyntax containingClass)
    {
        containingClass = default!;

        if (!modifiers.Any(SyntaxKind.StaticKeyword))
        {
            return false;
        }

        if (parent is not ClassDeclarationSyntax declaration
            || declaration.Modifiers.Any(SyntaxKind.StaticKeyword))
        {
            return false;
        }

        containingClass = declaration;
        return true;
    }
}
