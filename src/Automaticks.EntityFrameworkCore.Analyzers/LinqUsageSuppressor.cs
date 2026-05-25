using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace Automaticks.EntityFrameworkCore;

/// <summary>
///     Suppresses <c>ATXLQ002</c> (LinqUsage) in files that import <c>Microsoft.EntityFrameworkCore</c>
///     or any <c>Microsoft.EntityFrameworkCore.*</c> namespace.
///     EF Core's LINQ provider operates on expression trees rather than in-memory collections,
///     so LINQ queries in EF Core files are translated to SQL and do not carry the performance
///     concerns that ban LINQ in general production code.
///     Install both <c>Automaticks.Linq</c> and this package to enforce the
///     architecture rule while automatically allowing it where EF Core is used.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LinqUsageSuppressor : DiagnosticSuppressor
{
    private static readonly SuppressionDescriptor Rule = new(
        id: SuppressionIds.EFCore.LinqUsage,
        suppressedDiagnosticId: "ATXLQ002",
        justification: "LINQ is permitted in files that import Microsoft.EntityFrameworkCore.");

    /// <inheritdoc />
    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => [Rule];

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
            if (root is not CompilationUnitSyntax compilationUnit)
            {
                continue;
            }

            foreach (var usingDirective in compilationUnit.Usings)
            {
                var name = usingDirective.Name?.ToString() ?? string.Empty;
                if (name.Equals("Microsoft.EntityFrameworkCore", StringComparison.Ordinal) ||
                    name.StartsWith("Microsoft.EntityFrameworkCore.", StringComparison.Ordinal))
                {
                    context.ReportSuppression(Suppression.Create(Rule, diagnostic));
                    break;
                }
            }
        }
    }
}
