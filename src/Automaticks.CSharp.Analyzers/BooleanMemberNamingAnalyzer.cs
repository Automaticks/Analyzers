using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp;

/// <summary>
///     Flags <c>bool</c> and <c>bool?</c> fields and properties whose names do not begin with
///     an allowed prefix: <c>is</c> or <c>allow</c> (case-insensitive).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BooleanMemberNamingAnalyzer : DiagnosticAnalyzer
{
    private static readonly string[] AllowedPrefixes = ["is", "allow"];

    /// <summary>
    ///     The diagnostic rule reported when a boolean field or property does not start with an allowed prefix.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.CSharp.BooleanMemberNaming,
        "Boolean fields and properties must use an allowed prefix",
        "'{0}' is a boolean {1} but its name does not start with an allowed prefix ('is' or 'allow', case-insensitive). Rename it to start with one of those prefixes (e.g. '{0}' \u2192 'is{0}' or 'allow{0}'). This convention makes boolean intent immediately clear at every call site and is required for codebase consistency.",
        "CSharp",
        DiagnosticSeverity.Error,
        true,
        "Rename the field or property so its name begins with 'is' or 'allow' (any casing). Examples: 'enabled' \u2192 'isEnabled', 'AllowRetry' is already valid. Overrides and interface implementations where renaming would break an external contract are exempt.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeField, SyntaxKind.FieldDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeProperty, SyntaxKind.PropertyDeclaration);
    }

    private static void AnalyzeField(SyntaxNodeAnalysisContext context)
    {
        var fieldDecl = (FieldDeclarationSyntax)context.Node;

        foreach (var variable in fieldDecl.Declaration.Variables)
        {
            var name = variable.Identifier.Text;
            if (HasAllowedPrefix(name))
            {
                continue;
            }

            var symbol = context.SemanticModel.GetDeclaredSymbol(variable) as IFieldSymbol;
            if (symbol is null)
            {
                continue;
            }

            if (!IsBooleanType(symbol.Type))
            {
                continue;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, variable.Identifier.GetLocation(), name, "field"));
        }
    }

    private static void AnalyzeProperty(SyntaxNodeAnalysisContext context)
    {
        var propDecl = (PropertyDeclarationSyntax)context.Node;
        var name = propDecl.Identifier.Text;

        if (HasAllowedPrefix(name))
        {
            return;
        }

        var symbol = context.SemanticModel.GetDeclaredSymbol(propDecl) as IPropertySymbol;
        if (symbol is null)
        {
            return;
        }

        if (!IsBooleanType(symbol.Type))
        {
            return;
        }

        if (symbol.IsOverride && IsExternalPropertyOverride(symbol))
        {
            return;
        }

        if (symbol.ExplicitInterfaceImplementations.Length > 0)
        {
            foreach (var ifaceProp in symbol.ExplicitInterfaceImplementations)
            {
                if (ifaceProp.DeclaringSyntaxReferences.IsEmpty)
                {
                    return;
                }
            }
        }

        if (IsImplicitExternalInterfacePropertyImplementation(symbol))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, propDecl.Identifier.GetLocation(), name, "property"));
    }

    private static bool HasAllowedPrefix(string name)
    {
        var start = 0;
        while (start < name.Length && name[start] == '_')
        {
            start++;
        }

        foreach (var prefix in AllowedPrefixes)
        {
            var remaining = name.Length - start;
            if (remaining >= prefix.Length &&
                string.Compare(name, start, prefix, 0, prefix.Length, System.StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsBooleanType(ITypeSymbol type)
    {
        if (type.SpecialType == SpecialType.System_Boolean)
        {
            return true;
        }

        if (type is INamedTypeSymbol { IsValueType: true, ConstructedFrom.SpecialType: SpecialType.System_Nullable_T } namedType)
        {
            return namedType.TypeArguments.Length == 1 &&
                   namedType.TypeArguments[0].SpecialType == SpecialType.System_Boolean;
        }

        return false;
    }

    private static bool IsExternalPropertyOverride(IPropertySymbol property)
    {
        var overridden = property.OverriddenProperty;
        while (overridden is not null)
        {
            if (overridden.DeclaringSyntaxReferences.IsEmpty)
            {
                return true;
            }

            overridden = overridden.OverriddenProperty;
        }

        return false;
    }

    private static bool IsImplicitExternalInterfacePropertyImplementation(IPropertySymbol property)
    {
        if (property.IsOverride)
        {
            return false;
        }

        foreach (var iface in property.ContainingType.AllInterfaces)
        {
            foreach (var member in iface.GetMembers())
            {
                if (member is not IPropertySymbol ifaceProp)
                {
                    continue;
                }

                if (ifaceProp.DeclaringSyntaxReferences.IsEmpty &&
                    SymbolEqualityComparer.Default.Equals(
                        property.ContainingType.FindImplementationForInterfaceMember(ifaceProp),
                        property))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
