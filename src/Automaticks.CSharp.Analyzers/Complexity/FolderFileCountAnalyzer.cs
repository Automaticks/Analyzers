using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;

namespace Automaticks.CSharp.Complexity;

/// <summary>
///     Enforces a maximum number of source files directly inside a single folder.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class FolderFileCountAnalyzer : DiagnosticAnalyzer
{
    private const int MaxFiles = 20;

    /// <summary>
    ///     Diagnostic rule emitted when a folder exceeds <see cref="MaxFiles" /> direct source files.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static FolderFileCountAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.FolderFileCount,
            "Folders must not exceed the maximum number of source files",
            "Folder '{0}' directly contains {1} source files, which exceeds the maximum of 20. Split it into smaller, more focused sub-folders.",
            "Maintainability",
            DiagnosticSeverity.Error,
            true,
            "The folder directly contains more than 20 source files. Split it into smaller, more focused sub-folders that each group a cohesive set of related types, moving the corresponding namespace accordingly. Files in subfolders do not count toward this limit.",
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

    private bool CanPrecede(string candidatePath, string currentPath)
    {
        return string.CompareOrdinal(candidatePath, currentPath) < 0;
    }

    private void CollectFile(
        SyntaxTreeAnalysisContext treeContext,
        ConcurrentDictionary<string, ConcurrentDictionary<string, Location>> filesByFolder)
    {
        var filePath = treeContext.Tree.FilePath;
        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }

        var directory = Path.GetDirectoryName(filePath);
        if (string.IsNullOrEmpty(directory))
        {
            return;
        }

        var span = new TextSpan(0, 0);
        var location = Location.Create(treeContext.Tree, span);
        var files = filesByFolder.GetOrAdd(directory, _ =>
        {
            var newFiles = new ConcurrentDictionary<string, Location>();
            return newFiles;
        });
        files.TryAdd(filePath, location);
    }

    private PrimaryFileLocation FindPrimaryFile(ConcurrentDictionary<string, Location> files)
    {
        string? primaryPath = null;
        Location? primaryLocation = null;
        var additionalLocations = new List<Location>();

        foreach (var fileEntry in files)
        {
            if (primaryPath is null || CanPrecede(fileEntry.Key, primaryPath))
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

        return new PrimaryFileLocation(primaryLocation!, additionalLocations);
    }

    private void RegisterCompilationActions(CompilationStartAnalysisContext compilationContext)
    {
        var filesByFolder = new ConcurrentDictionary<string, ConcurrentDictionary<string, Location>>();

        compilationContext.RegisterSyntaxTreeAction(treeContext => CollectFile(treeContext, filesByFolder));
        compilationContext.RegisterCompilationEndAction(endContext => ReportViolations(endContext, filesByFolder));
    }

    private void ReportViolations(
        CompilationAnalysisContext endContext,
        ConcurrentDictionary<string, ConcurrentDictionary<string, Location>> filesByFolder)
    {
        foreach (var folderEntry in filesByFolder)
        {
            if (folderEntry.Value.Count <= MaxFiles)
            {
                continue;
            }

            var primaryFile = FindPrimaryFile(folderEntry.Value);
            endContext.ReportDiagnostic(Diagnostic.Create(
                Rule,
                primaryFile.Location,
                primaryFile.AdditionalLocations,
                folderEntry.Key,
                folderEntry.Value.Count));
        }
    }

    /// <summary>
    ///     Holds the primary (earliest-path) file location and the remaining locations for a folder.
    /// </summary>
    private sealed class PrimaryFileLocation
    {
        /// <summary>
        ///     Gets the locations of the files other than the primary file.
        /// </summary>
        public List<Location> AdditionalLocations { get; }

        /// <summary>
        ///     Gets the location of the primary (earliest-path) file.
        /// </summary>
        public Location Location { get; }

        public PrimaryFileLocation(Location location, List<Location> additionalLocations)
        {
            Location = location;
            AdditionalLocations = additionalLocations;
        }
    }
}
