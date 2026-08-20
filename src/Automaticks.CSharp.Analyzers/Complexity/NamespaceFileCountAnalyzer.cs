using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Automaticks.CSharp.Complexity;

/// <summary>
///     Enforces a maximum number of distinct source files that declare a given namespace.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NamespaceFileCountAnalyzer : DiagnosticAnalyzer
{
    private const int MaxFiles = 20;

    /// <summary>
    ///     Diagnostic rule emitted when a namespace exceeds <see cref="MaxFiles" /> source files.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static NamespaceFileCountAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.NamespaceFileCount,
            "Namespaces must not exceed the maximum number of source files",
            "Namespace '{0}' is declared by {1} source files, which exceeds the maximum of 20. Split it into smaller, more focused sub-namespaces.",
            "Maintainability",
            DiagnosticSeverity.Error,
            true,
            "The namespace is declared directly by more than 20 source files. Split it into smaller, more focused sub-namespaces that each group a cohesive set of related types, moving the corresponding files accordingly. Files in child namespaces do not count toward this limit.",
            customTags: WellKnownDiagnosticTags.CompilationEnd);
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(RegisterCompilationActions);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void CollectNamespacePart(
        SyntaxNodeAnalysisContext nodeContext,
        ConcurrentDictionary<string, ConcurrentDictionary<string, Location>> filesByNamespace)
    {
        var namespaceDecl = (nodeContext.Node as BaseNamespaceDeclarationSyntax)!;

        if (HasNestedNamespace(namespaceDecl))
        {
            return;
        }

        var filePath = namespaceDecl.SyntaxTree.FilePath;
        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }

        var symbol = nodeContext.SemanticModel.GetDeclaredSymbol(namespaceDecl)!;

        var namespaceName = symbol.ToDisplayString();
        var location = namespaceDecl.Name.GetLocation();
        var files = filesByNamespace.GetOrAdd(namespaceName, CreateFileLocationMap);
        files.TryAdd(filePath, location);
    }

    private ConcurrentDictionary<string, Location> CreateFileLocationMap(string namespaceName)
    {
        return new ConcurrentDictionary<string, Location>();
    }

    private Location FindPrimaryLocation(ConcurrentDictionary<string, Location> files, List<Location> additionalLocations)
    {
        string? primaryPath = null;
        Location? primaryLocation = null;

        foreach (var fileEntry in files)
        {
            if (primaryPath is null || HasEarlierPath(fileEntry.Key, primaryPath))
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

        return primaryLocation!;
    }

    private bool HasEarlierPath(string candidatePath, string currentPath)
    {
        return string.CompareOrdinal(candidatePath, currentPath) < 0;
    }

    private bool HasNestedNamespace(BaseNamespaceDeclarationSyntax namespaceDecl)
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

    private void RegisterCompilationActions(CompilationStartAnalysisContext compilationContext)
    {
        var filesByNamespace = new ConcurrentDictionary<string, ConcurrentDictionary<string, Location>>();

        compilationContext.RegisterSyntaxNodeAction(
            nodeContext => CollectNamespacePart(nodeContext, filesByNamespace),
            SyntaxKind.NamespaceDeclaration,
            SyntaxKind.FileScopedNamespaceDeclaration);

        compilationContext.RegisterCompilationEndAction(endContext => ReportViolations(endContext, filesByNamespace));
    }

    private void ReportViolations(
        CompilationAnalysisContext endContext,
        ConcurrentDictionary<string, ConcurrentDictionary<string, Location>> filesByNamespace)
    {
        foreach (var namespaceEntry in filesByNamespace)
        {
            if (namespaceEntry.Value.Count <= MaxFiles)
            {
                continue;
            }

            var additionalLocations = new List<Location>();
            var primaryLocation = FindPrimaryLocation(namespaceEntry.Value, additionalLocations);
            endContext.ReportDiagnostic(Diagnostic.Create(
                Rule,
                primaryLocation,
                additionalLocations,
                namespaceEntry.Key,
                namespaceEntry.Value.Count));
        }
    }
}
