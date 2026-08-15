using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Automaticks.CSharp.Formatting;

/// <summary>
///     Flags top-level regular <c>using</c> directives (non-static, non-alias, non-global) whose
///     namespace contributes no referenced symbols to the file.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UnusedUsingDirectiveAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a <c>using</c> directive is unused.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static UnusedUsingDirectiveAnalyzer()
    {
        Rule = new(
            DiagnosticIds.CSharp.UnusedUsingDirective,
            "Unused using directive",
            "Remove unused using directive '{0}'. A code fix is available (dotnet format analyzers --diagnostics ATXCS048).",
            "Style",
            DiagnosticSeverity.Error,
            true,
            "Remove the `using` directive. No types or extension methods from this namespace are referenced in the file. Removing it reduces noise in the import section and prevents future confusion.");
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

        var regularUsings = CollectRegularUsings(compilationUnit);

        if (regularUsings.Count == 0)
        {
            return;
        }

        var usedNamespaces = CollectUsedNamespaces(compilationUnit, context.SemanticModel);

        foreach (var usingDirective in regularUsings)
        {
            var namespaceName = usingDirective.Name?.ToString() ?? string.Empty;

            if (!usedNamespaces.Contains(namespaceName))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, usingDirective.GetLocation(), namespaceName));
            }
        }
    }

    private List<UsingDirectiveSyntax> CollectRegularUsings(CompilationUnitSyntax compilationUnit)
    {
        var result = new List<UsingDirectiveSyntax>();

        foreach (var usingDirective in compilationUnit.Usings)
        {
            if (!usingDirective.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword)
                && !usingDirective.StaticKeyword.IsKind(SyntaxKind.StaticKeyword)
                && usingDirective.Alias is null)
            {
                result.Add(usingDirective);
            }
        }

        return result;
    }

    private HashSet<string> CollectUsedNamespaces(
        CompilationUnitSyntax compilationUnit,
        SemanticModel semanticModel)
    {
        var usedNamespaces = new HashSet<string>(StringComparer.Ordinal);

        foreach (var node in compilationUnit.DescendantNodes())
        {
            if (node is not SimpleNameSyntax simpleName || HasUsingDirectiveAncestor(node))
            {
                continue;
            }

            var symbolInfo = semanticModel.GetSymbolInfo(simpleName);
            var symbol = symbolInfo.Symbol;

            if (symbol is null && symbolInfo.CandidateSymbols.Length > 0)
            {
                symbol = symbolInfo.CandidateSymbols[0];
            }

            if (symbol?.ContainingNamespace is { IsGlobalNamespace: false } containingNamespace)
            {
                usedNamespaces.Add(containingNamespace.ToDisplayString());
            }
        }

        return usedNamespaces;
    }

    private bool HasUsingDirectiveAncestor(SyntaxNode node)
    {
        var current = node.Parent;

        while (current is not null)
        {
            if (current is UsingDirectiveSyntax)
            {
                return true;
            }

            current = current.Parent;
        }

        return false;
    }
}
