using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.Naming;

/// <summary>
///     Flags mutable static state declared in a non-static class.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class StaticMemberInNonStaticClassAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The <c>.editorconfig</c> key listing extra type names treated as mutable.
    /// </summary>
    public const string MutableTypesKey = "automaticks.static_member_mutable_types";
    private static readonly ImmutableArray<string> DefaultMutableTypes;
    private static readonly DiagnosticDescriptor Rule;

    static StaticMemberInNonStaticClassAnalyzer()
    {
        Rule = new(
            DiagnosticIds.CSharp.StaticMemberInNonStaticClass,
            "Mutable static state must not exist in a non-static class",
            "{0} '{1}' is mutable static state in non-static class '{2}'. Make it a readonly field of an immutable type, or move it to a dedicated static class.",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "Mutable static state in an instantiable class is shared across every instance and every thread, which is where state-management bugs hide. A field is reported when it is not `readonly`, or when it is `readonly` but its type has mutable contents, because `readonly` freezes the reference and not the object. Prefer an immutable type such as `ImmutableArray<T>` or `ImmutableHashSet<T>`. Additional mutable type names can be declared through `automaticks.static_member_mutable_types` in `.editorconfig`. Constants and computed properties hold no state and are never reported.");
        DefaultMutableTypes =
        [
            "ArrayList",
            "Collection",
            "ConcurrentBag",
            "ConcurrentDictionary",
            "ConcurrentQueue",
            "ConcurrentStack",
            "Dictionary",
            "HashSet",
            "Hashtable",
            "LinkedList",
            "List",
            "ObservableCollection",
            "Queue",
            "SortedDictionary",
            "SortedList",
            "SortedSet",
            "Stack",
            "StringBuilder"
        ];
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

        var isReadOnly = field.Modifiers.Any(SyntaxKind.ReadOnlyKeyword);
        if (isReadOnly && !HasMutableType(context, field.Declaration.Type))
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

        if (!HasStoredState(context, property))
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
    ///     Reduces a type reference to its bare name so generic and qualified forms match.
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

    /// <summary>
    ///     Returns whether the type carries mutable contents that <c>readonly</c> cannot freeze.
    /// </summary>
    private bool HasMutableType(SyntaxNodeAnalysisContext context, TypeSyntax type)
    {
        if (type is ArrayTypeSyntax)
        {
            return true;
        }

        var mutableTypes = ConfigurableTypeList.Read(context, MutableTypesKey, DefaultMutableTypes);
        return mutableTypes.Contains(GetSimpleTypeName(type));
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

    /// <summary>
    ///     Returns whether the property stores state rather than computing a value.
    /// </summary>
    private bool HasStoredState(SyntaxNodeAnalysisContext context, PropertyDeclarationSyntax property)
    {
        if (property.ExpressionBody is not null || property.AccessorList is null)
        {
            return false;
        }

        foreach (var accessor in property.AccessorList.Accessors)
        {
            if (accessor.IsKind(SyntaxKind.SetAccessorDeclaration)
                || accessor.IsKind(SyntaxKind.InitAccessorDeclaration))
            {
                return true;
            }

            if (accessor.Body is not null || accessor.ExpressionBody is not null)
            {
                return false;
            }
        }

        return HasMutableType(context, property.Type);
    }
}
