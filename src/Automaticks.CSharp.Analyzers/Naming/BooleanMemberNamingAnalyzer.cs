using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace Automaticks.CSharp.Naming;

/// <summary>
///     Flags <c>bool</c> and <c>bool?</c> fields and properties whose names do not begin with
///     an allowed prefix: <c>is</c>, <c>allow</c>, or <c>has</c> (case-insensitive).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BooleanMemberNamingAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a boolean field or property does not start with an allowed prefix.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;
    private static readonly ImmutableArray<string> AllowedPrefixes;

    static BooleanMemberNamingAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.BooleanMemberNaming,
            "Boolean fields and properties must use an allowed prefix",
            "'{0}' is a boolean {1} but its name does not start with an allowed prefix ('is', 'allow', or 'has', case-insensitive). Rename it to start with one of those prefixes, replacing any existing prefix rather than adding to it. This convention makes boolean intent immediately clear at every call site and is required for codebase consistency. A code fix is available (dotnet format analyzers --diagnostics ATXCS062).",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "Rename the field or property so its name begins with 'is', 'allow', or 'has' (any casing). Examples: 'enabled' \u2192 'isEnabled', 'canRetry' \u2192 'isRetry', 'AllowRetry' is already valid. Overrides and interface implementations where renaming would break an external contract are exempt.");
        Rule = rule;
        AllowedPrefixes = ["is", "allow", "has"];
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
        var fieldDecl = (context.Node as FieldDeclarationSyntax)!;
        foreach (var variable in fieldDecl.Declaration.Variables)
        {
            var name = variable.Identifier.Text;
            if (HasAllowedPrefix(name))
            {
                continue;
            }

            var symbol = (context.SemanticModel.GetDeclaredSymbol(variable) as IFieldSymbol)!;
            if (!HasBooleanType(symbol.Type))
            {
                continue;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, variable.Identifier.GetLocation(), name, "field"));
        }
    }

    private void AnalyzeProperty(SyntaxNodeAnalysisContext context)
    {
        var propDecl = (context.Node as PropertyDeclarationSyntax)!;
        var name = propDecl.Identifier.Text;

        if (HasAllowedPrefix(name))
        {
            return;
        }

        var symbol = (context.SemanticModel.GetDeclaredSymbol(propDecl) as IPropertySymbol)!;
        if (!HasBooleanType(symbol.Type))
        {
            return;
        }

        if (symbol.IsOverride && HasExternalPropertyOverride(symbol))
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

        if (HasImplicitExternalInterfacePropertyImplementation(symbol))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, propDecl.Identifier.GetLocation(), name, "property"));
    }

    private bool HasAllowedPrefix(string name)
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
                string.Compare(name, start, prefix, 0, prefix.Length, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }
        }

        return false;
    }

    private bool HasBooleanType(ITypeSymbol type)
    {
        if (type.SpecialType == SpecialType.System_Boolean)
        {
            return true;
        }

        if (type is INamedTypeSymbol { IsValueType: true, ConstructedFrom.SpecialType: SpecialType.System_Nullable_T } namedType)
        {
            return namedType.TypeArguments[0].SpecialType == SpecialType.System_Boolean;
        }

        return false;
    }

    private bool HasExternalPropertyOverride(IPropertySymbol property)
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

    private bool HasImplicitExternalInterfacePropertyImplementation(IPropertySymbol property)
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
