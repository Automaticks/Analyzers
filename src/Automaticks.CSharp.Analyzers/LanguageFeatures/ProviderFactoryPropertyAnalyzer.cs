using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace Automaticks.CSharp.LanguageFeatures;

/// <summary>
///     Flags public properties declared on types whose name ends with Provider, Factory, Builder, or Client.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ProviderFactoryPropertyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule;

    /// <summary>
    ///     The diagnostic rule reported when a provider/factory/builder/client/session type exposes a property.
    /// </summary>
    private readonly string[] ForbiddenSuffixes;

    static ProviderFactoryPropertyAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.ProviderFactoryProperty,
            "Provider/Factory/Builder/Client/Session types must not expose properties",
            "Type '{0}' exposes property '{1}'. Provider, Factory, Builder, Client, and Session types must use methods, not properties.",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "Convert the property to a method. Types whose name ends with Provider, Factory, Builder, Client, or Session are service types and must only expose methods, not properties. Example: rename `public Foo CurrentFoo { get; }` to `public Foo GetCurrentFoo()` or `public Foo CreateFoo()`. Exempt: properties that override an external base member or implement an external interface, since their shape is fixed by an assembly outside this compilation.");
        Rule = rule;
    }

    /// <summary>
    ///     Initializes the service-type suffixes inspected during analysis.
    /// </summary>
    public ProviderFactoryPropertyAnalyzer()
    {
        ForbiddenSuffixes = ["Provider", "Factory", "Builder", "Client", "Session"];
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeType(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol type)
        {
            return;
        }

        var hasMatchingSuffix = false;
        foreach (var suffix in ForbiddenSuffixes)
        {
            if (type.Name.EndsWith(suffix, StringComparison.Ordinal))
            {
                hasMatchingSuffix = true;
                break;
            }
        }

        if (!hasMatchingSuffix)
        {
            return;
        }

        foreach (var member in type.GetMembers())
        {
            if (member is not IPropertySymbol property)
            {
                continue;
            }

            if (HasExternalOrigin(property))
            {
                continue;
            }

            Location location;
            if (property.Locations.Length > 0)
            {
                location = property.Locations[0];
            }
            else
            {
                location = Location.None;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, location, type.Name, property.Name));
        }
    }

    private bool HasExternalExplicitPropertyImplementation(IPropertySymbol property)
    {
        foreach (var interfaceProperty in property.ExplicitInterfaceImplementations)
        {
            if (interfaceProperty.DeclaringSyntaxReferences.IsEmpty)
            {
                return true;
            }
        }

        return false;
    }

    private bool HasExternalImplicitPropertyImplementation(IPropertySymbol property)
    {
        foreach (var interfaceType in property.ContainingType.AllInterfaces)
        {
            foreach (var member in interfaceType.GetMembers())
            {
                if (member is not IPropertySymbol interfaceProperty)
                {
                    continue;
                }

                if (interfaceProperty.DeclaringSyntaxReferences.IsEmpty &&
                    SymbolEqualityComparer.Default.Equals(
                        property.ContainingType.FindImplementationForInterfaceMember(interfaceProperty),
                        property))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool HasExternalOrigin(IPropertySymbol property)
    {
        if (HasExternalPropertyOverride(property))
        {
            return true;
        }

        if (HasExternalExplicitPropertyImplementation(property))
        {
            return true;
        }

        if (property.IsOverride)
        {
            return false;
        }

        return HasExternalImplicitPropertyImplementation(property);
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
}
