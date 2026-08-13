using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace Automaticks.CSharp.Formatting;

/// <summary>
///     Requires all top-level regular <c>using</c> directives (non-static, non-alias, non-global)
///     to appear in pure case-insensitive alphabetical order.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UnsortedUsingDirectivesAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a <c>using</c> directive is out of alphabetical order.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static UnsortedUsingDirectivesAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.UnsortedUsingDirectives,
            "Using directives must be sorted alphabetically",
            "'{0}' is out of alphabetical order — it must appear before '{1}'. Sort all using directives case-insensitively A→Z.",
            "Style",
            DiagnosticSeverity.Error,
            true,
            "Reorder the `using` directives so that all regular (non-static, non-alias) directives in the file are sorted case-insensitively in ascending alphabetical order. Move the flagged directive to a position before '{1}'.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeCompilationUnit, SyntaxKind.CompilationUnit);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeCompilationUnit(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not CompilationUnitSyntax compilationUnit)
        {
            return;
        }

        string? previousName = null;

        foreach (var usingDirective in compilationUnit.Usings)
        {
            if (!HasRegularUsing(usingDirective))
            {
                continue;
            }

            var currentName = usingDirective.Name?.ToString() ?? string.Empty;

            if (previousName is not null &&
                string.Compare(currentName, previousName, StringComparison.OrdinalIgnoreCase) < 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, usingDirective.GetLocation(), currentName, previousName));
            }

            previousName = currentName;
        }
    }

    private bool HasRegularUsing(UsingDirectiveSyntax usingDirective)
    {
        return !usingDirective.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword)
            && !usingDirective.StaticKeyword.IsKind(SyntaxKind.StaticKeyword)
            && usingDirective.Alias is null;
    }
}
