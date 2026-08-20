using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.Naming;

/// <summary>
///     Flags declarations using the <c>internal</c> access modifier; use <c>public</c> instead.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InternalModifierAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a symbol is declared with <c>internal</c> accessibility.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static InternalModifierAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.InternalModifier,
            "The 'internal' access modifier is forbidden",
            "'{0}' uses the 'internal' access modifier, which is forbidden. Use 'public' instead. A code fix is available (dotnet format analyzers --diagnostics ATXCS013).",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "Change the `internal` access modifier to `public`. The `internal` modifier is never permitted in this codebase. Every member that needs to be accessible beyond the declaring type must be declared `public`.");
        Rule = rule;
    }

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

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        if (context.Symbol.DeclaredAccessibility != Accessibility.Internal)
        {
            return;
        }

        var location = context.Symbol.Locations[0];
        context.ReportDiagnostic(Diagnostic.Create(Rule, location, context.Symbol.Name));
    }
}
