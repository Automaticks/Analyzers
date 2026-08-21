using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace Automaticks.EntityFrameworkCore;

/// <summary>
///     Suppresses ATXLQ002 (LinqUsage) in files that import Microsoft.EntityFrameworkCore or any Microsoft.EntityFrameworkCore.* namespace.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LinqUsageSuppressor : DiagnosticSuppressor
{
    private static readonly SuppressionDescriptor Rule;

    static LinqUsageSuppressor()
    {
        var rule = new SuppressionDescriptor(
            id: SuppressionIds.EFCore.LinqUsage,
            suppressedDiagnosticId: "ATXLQ002",
            justification: "LINQ is permitted in files that import Microsoft.EntityFrameworkCore.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
        foreach (var diagnostic in context.ReportedDiagnostics)
        {
            var tree = diagnostic.Location.SourceTree;
            if (tree is null)
            {
                continue;
            }

            var root = tree.GetRoot(context.CancellationToken);
            var compilationUnit = (root as CompilationUnitSyntax)!;
            foreach (var usingDirective in compilationUnit.Usings)
            {
                var name = usingDirective.Name?.ToString() ?? string.Empty;
                if (name.Equals("Microsoft.EntityFrameworkCore", StringComparison.Ordinal)
                    || name.StartsWith("Microsoft.EntityFrameworkCore.", StringComparison.Ordinal))
                {
                    context.ReportSuppression(Suppression.Create(Rule, diagnostic));
                    break;
                }
            }
        }
    }

    /// <inheritdoc />
    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => [Rule];
}
