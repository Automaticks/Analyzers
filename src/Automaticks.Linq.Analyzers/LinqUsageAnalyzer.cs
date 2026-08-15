using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace Automaticks.Linq;

/// <summary>
///     Flags <c>using System.Linq</c> (or any <c>System.Linq.*</c>) directive in production code.
///     LINQ is not permitted; use explicit <c>foreach</c> loops instead.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LinqUsageAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule;

    static LinqUsageAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.Linq.LinqUsage,
            "LINQ is not allowed",
            "LINQ is not allowed in this codebase. Use explicit loops instead.",
            "Linq",
            DiagnosticSeverity.Warning,
            true,
            "Replace the LINQ method chain or query expression with an explicit `foreach` loop. Remove the `using System.Linq;` directive if it becomes unused after the change. Note: LINQ inside Entity Framework Core `IQueryable` expressions is suppressed by a separate suppressor.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeUsing, SyntaxKind.UsingDirective);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeUsing(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not UsingDirectiveSyntax usingDirective)
        {
            return;
        }

        var name = usingDirective.Name?.ToString() ?? string.Empty;
        var isLinqUsage = name.Equals("System.Linq", StringComparison.Ordinal)
            || (name.StartsWith("System.Linq.", StringComparison.Ordinal)
                && !name.StartsWith("System.Linq.Expressions", StringComparison.Ordinal));

        if (isLinqUsage)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, usingDirective.GetLocation()));
        }
    }
}
