using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace Automaticks.CSharp;

/// <summary>
///     Flags public properties declared on types whose name ends with <c>Provider</c>,
///     <c>Factory</c>, <c>Builder</c>, or <c>Client</c>. These service types must expose
///     their API through methods only.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ProviderFactoryPropertyAnalyzer : DiagnosticAnalyzer
{
    private static readonly string[] ForbiddenSuffixes = ["Provider", "Factory", "Builder", "Client", "Session"];

    /// <summary>
    ///     The diagnostic rule reported when a provider/factory/builder/client/session type exposes a property.
    /// </summary>
    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.CSharp.ProviderFactoryProperty,
        "Provider/Factory/Builder/Client/Session types must not expose properties",
        "Type '{0}' exposes property '{1}'. Provider, Factory, Builder, Client, and Session types must use methods, not properties.",
        "CSharp",
        DiagnosticSeverity.Error,
        true,
        "Convert the property to a method. Types whose name ends with Provider, Factory, Builder, Client, or Session are service types and must only expose methods, not properties. Example: rename `public Foo CurrentFoo { get; }` to `public Foo GetCurrentFoo()` or `public Foo CreateFoo()`.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
    }

    private static void AnalyzeType(SymbolAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.Symbol;

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
}
