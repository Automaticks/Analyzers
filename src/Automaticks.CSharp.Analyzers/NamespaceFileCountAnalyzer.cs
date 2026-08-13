using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Automaticks.CSharp;

/// <summary>
///     Enforces a maximum number of distinct source files that declare a given namespace. A
///     namespace whose file count exceeds <see cref="MaxFiles" /> is flagged.
///     <para>
///         Matching is exact: a file contributes only to the innermost (leaf) namespace it
///         declares, so files in a child namespace (e.g. <c>MyApp.Services.Impl</c>) do not
///         count toward a parent namespace (e.g. <c>MyApp.Services</c>). Files with no declared
///         namespace and generated files (those containing an <c>&lt;auto-generated&gt;</c>
///         header) are skipped automatically.
///     </para>
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NamespaceFileCountAnalyzer : DiagnosticAnalyzer
{
    /// <summary>Diagnostic rule emitted when a namespace exceeds <see cref="MaxFiles" /> source files.</summary>
    public static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.CSharp.NamespaceFileCount,
        "Namespaces must not exceed the maximum number of source files",
        "Namespace '{0}' is declared by {1} source files, which exceeds the maximum of 20. Split it into smaller, more focused sub-namespaces.",
        "Maintainability",
        DiagnosticSeverity.Error,
        true,
        "The namespace is declared directly by more than 20 source files. Split it into smaller, more focused sub-namespaces that each group a cohesive set of related types, moving the corresponding files accordingly. Files in child namespaces do not count toward this limit.",
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    private const int MaxFiles = 20;

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(RegisterCompilationActions);
    }

    private static void CollectNamespacePart(
        SyntaxNodeAnalysisContext nodeContext,
        ConcurrentDictionary<string, ConcurrentDictionary<string, Location>> filesByNamespace)
    {
        var namespaceDecl = (BaseNamespaceDeclarationSyntax)nodeContext.Node;
        if (HasNestedNamespace(namespaceDecl))
        {
            return;
        }

        var filePath = namespaceDecl.SyntaxTree.FilePath;
        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }

        if (nodeContext.SemanticModel.GetDeclaredSymbol(namespaceDecl) is not { } symbol)
        {
            return;
        }

        var namespaceName = symbol.ToDisplayString();
        var location = namespaceDecl.Name.GetLocation();
        var files = filesByNamespace.GetOrAdd(namespaceName, _ => new ConcurrentDictionary<string, Location>());
        files.TryAdd(filePath, location);
    }

    private static bool HasNestedNamespace(BaseNamespaceDeclarationSyntax namespaceDecl)
    {
        foreach (var member in namespaceDecl.Members)
        {
            if (member is BaseNamespaceDeclarationSyntax)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsEarlierPath(string candidatePath, string currentPath)
    {
        return string.CompareOrdinal(candidatePath, currentPath) < 0;
    }

    private static void RegisterCompilationActions(CompilationStartAnalysisContext compilationContext)
    {
        var filesByNamespace = new ConcurrentDictionary<string, ConcurrentDictionary<string, Location>>();

        compilationContext.RegisterSyntaxNodeAction(
            nodeContext => CollectNamespacePart(nodeContext, filesByNamespace),
            SyntaxKind.NamespaceDeclaration,
            SyntaxKind.FileScopedNamespaceDeclaration);

        compilationContext.RegisterCompilationEndAction(endContext => ReportViolations(endContext, filesByNamespace));
    }

    private static void ReportViolations(
        CompilationAnalysisContext endContext,
        ConcurrentDictionary<string, ConcurrentDictionary<string, Location>> filesByNamespace)
    {
        foreach (var namespaceEntry in filesByNamespace)
        {
            if (namespaceEntry.Value.Count <= MaxFiles)
            {
                continue;
            }

            string? primaryPath = null;
            Location? primaryLocation = null;
            var additionalLocations = new List<Location>();

            foreach (var fileEntry in namespaceEntry.Value)
            {
                if (primaryPath is null || IsEarlierPath(fileEntry.Key, primaryPath))
                {
                    if (primaryLocation is not null)
                    {
                        additionalLocations.Add(primaryLocation);
                    }

                    primaryPath = fileEntry.Key;
                    primaryLocation = fileEntry.Value;
                }
                else
                {
                    additionalLocations.Add(fileEntry.Value);
                }
            }

            if (primaryLocation is null)
            {
                continue;
            }

            endContext.ReportDiagnostic(Diagnostic.Create(
                Rule,
                primaryLocation,
                additionalLocations,
                namespaceEntry.Key,
                namespaceEntry.Value.Count));
        }
    }
}
