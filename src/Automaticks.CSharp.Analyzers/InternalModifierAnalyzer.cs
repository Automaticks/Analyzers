using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp;

/// <summary>
///     Flags any declaration that uses the <c>internal</c> access modifier.
///     All members that need to be visible beyond their declaring type must be <c>public</c>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InternalModifierAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a symbol is declared with <c>internal</c> accessibility.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.CSharp.InternalModifier,
        "The 'internal' access modifier is forbidden",
        "'{0}' uses the 'internal' access modifier, which is forbidden. Use 'public' instead.",
        "CSharp",
        DiagnosticSeverity.Error,
        true,
        "Change the `internal` access modifier to `public`. The `internal` modifier is never permitted in this codebase. Every member that needs to be accessible beyond the declaring type must be declared `public`.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(
            AnalyzeSymbol,
            SymbolKind.NamedType,
            SymbolKind.Method,
            SymbolKind.Property,
            SymbolKind.Field,
            SymbolKind.Event);
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        if (context.Symbol.DeclaredAccessibility != Accessibility.Internal)
        {
            return;
        }

        Location location;
        if (context.Symbol.Locations.Length > 0)
        {
            location = context.Symbol.Locations[0];
        }
        else
        {
            location = Location.None;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, location, context.Symbol.Name));
    }
}
