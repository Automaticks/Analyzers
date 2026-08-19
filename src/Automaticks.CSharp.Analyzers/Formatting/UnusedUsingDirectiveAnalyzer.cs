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
        var compilationUnit = (context.Node as CompilationUnitSyntax)!;
        var regularUsings = CollectRegularUsings(compilationUnit);

        if (regularUsings.Count == 0)
        {
            return;
        }

        var names = new List<string>(regularUsings.Count);
        var pending = new HashSet<string>(StringComparer.Ordinal);
        foreach (var usingDirective in regularUsings)
        {
            var namespaceName = usingDirective.Name!.ToString();
            names.Add(namespaceName);

            if (!HasEnclosingNamespaceImport(compilationUnit, namespaceName))
            {
                pending.Add(namespaceName);
            }
        }

        var usedNamespaces = new HashSet<string>(StringComparer.Ordinal);
        if (pending.Count > 0)
        {
            usedNamespaces = CollectUsedNamespaces(compilationUnit, context.SemanticModel, pending);
        }

        for (var index = 0; index < regularUsings.Count; index++)
        {
            if (!usedNamespaces.Contains(names[index]))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, regularUsings[index].GetLocation(), names[index]));
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
        SemanticModel semanticModel,
        HashSet<string> pending)
    {
        var usedNamespaces = new HashSet<string>(StringComparer.Ordinal);
        var namespaceNames = new Dictionary<INamespaceSymbol, string>(SymbolEqualityComparer.Default);

        foreach (var node in compilationUnit.DescendantNodes(node => node is not UsingDirectiveSyntax))
        {
            if (node is not SimpleNameSyntax simpleName || HasInferredTypeKeyword(simpleName))
            {
                continue;
            }

            var name = GetNamespaceName(simpleName, semanticModel, namespaceNames);

            if (name is null || !pending.Remove(name))
            {
                continue;
            }

            usedNamespaces.Add(name);

            if (pending.Count == 0)
            {
                break;
            }
        }

        return usedNamespaces;
    }

    private string? GetNamespaceName(
        SimpleNameSyntax simpleName,
        SemanticModel semanticModel,
        Dictionary<INamespaceSymbol, string> namespaceNames)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(simpleName);
        var symbol = symbolInfo.Symbol;

        if (symbol is null && symbolInfo.CandidateSymbols.Length > 0)
        {
            symbol = symbolInfo.CandidateSymbols[0];
        }

        if (HasQualifiedReference(simpleName, symbol))
        {
            return null;
        }

        var containingNamespace = symbol?.ContainingNamespace;

        if (containingNamespace is null || containingNamespace.IsGlobalNamespace)
        {
            return null;
        }

        if (!namespaceNames.TryGetValue(containingNamespace, out var name))
        {
            name = containingNamespace.ToDisplayString();
            namespaceNames.Add(containingNamespace, name);
        }

        return name;
    }

    /// <summary>
    ///     A namespace that encloses every namespace declared in the file is already in scope, so importing it is always redundant.
    /// </summary>
    private bool HasEnclosingNamespaceImport(CompilationUnitSyntax compilationUnit, string namespaceName)
    {
        var declared = false;

        foreach (var member in compilationUnit.Members)
        {
            if (member is not BaseNamespaceDeclarationSyntax namespaceDeclaration)
            {
                return false;
            }

            var declaredName = namespaceDeclaration.Name.ToString();
            if (declaredName != namespaceName
                && !declaredName.StartsWith(namespaceName + ".", StringComparison.Ordinal))
            {
                return false;
            }

            declared = true;
        }

        return declared;
    }

    /// <summary>
    ///     'var' binds to the inferred type, which would otherwise credit that type's namespace even though the keyword needs no import.
    /// </summary>
    private bool HasInferredTypeKeyword(SimpleNameSyntax simpleName)
    {
        return simpleName is IdentifierNameSyntax { IsVar: true };
    }

    /// <summary>
    ///     A name that is already qualified resolves without the using directive, and a member name never needs one.
    /// </summary>
    private bool HasQualifiedReference(SimpleNameSyntax simpleName, ISymbol? symbol)
    {
        var parent = simpleName.Parent;

        if (parent is QualifiedNameSyntax qualifiedName && qualifiedName.Right == simpleName)
        {
            return true;
        }

        return parent is MemberAccessExpressionSyntax memberAccess
               && memberAccess.Name == simpleName
               && symbol is not IMethodSymbol { IsExtensionMethod: true };
    }
}
