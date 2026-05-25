using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Automaticks.CSharp;

/// <summary>
///     Flags any top-level <c>using</c> directive that is identical to an earlier directive in the
///     same file. Covers regular, <c>static</c>, and alias usings. <c>global using</c> directives
///     are exempt because they reside in dedicated infrastructure files.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DuplicateUsingDirectiveAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The diagnostic rule reported when a duplicate <c>using</c> directive is detected.</summary>
    public static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.CSharp.DuplicateUsingDirective,
        "Duplicate using directive",
        "Remove the duplicate using directive '{0}'",
        "Style",
        DiagnosticSeverity.Error,
        true,
        "Remove the duplicate `using` directive. Keep exactly one copy of each namespace import at the top of the file.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeCompilationUnit, SyntaxKind.CompilationUnit);
    }

    private static void AnalyzeCompilationUnit(SyntaxNodeAnalysisContext context)
    {
        var compilationUnit = (CompilationUnitSyntax)context.Node;
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var usingDirective in compilationUnit.Usings)
        {
            if (usingDirective.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword))
            {
                continue;
            }

            var key = BuildKey(usingDirective);

            if (!seen.Add(key))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, usingDirective.GetLocation(), key));
            }
        }
    }

    private static string BuildKey(UsingDirectiveSyntax usingDirective)
    {
        var name = usingDirective.Name?.ToString() ?? string.Empty;

        if (usingDirective.StaticKeyword.IsKind(SyntaxKind.StaticKeyword))
        {
            return "static:" + name;
        }

        if (usingDirective.Alias is { } alias)
        {
            return alias.Name.Identifier.Text + "=" + name;
        }

        return name;
    }
}
