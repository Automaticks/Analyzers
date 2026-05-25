using Automaticks.Reflection.Analyzers.Tests.Stubs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.Reflection.Analyzers.Tests;

public static class AnalyzerTestRunner
{
    public static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(
        DiagnosticAnalyzer analyzer,
        string source,
        bool isTestProject = false,
        bool isAnalyzerProject = false)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        return await AnalyzeTreesAsync(
            analyzer,
            [syntaxTree],
            isTestProject,
            isAnalyzerProject);
    }

    public static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(
        DiagnosticAnalyzer analyzer,
        string source,
        IReadOnlyList<MetadataReference> additionalReferences,
        bool isTestProject = false,
        bool isAnalyzerProject = false)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        return await AnalyzeTreesAsync(
            analyzer,
            [syntaxTree],
            isTestProject,
            isAnalyzerProject,
            additionalReferences);
    }

    public static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(
        DiagnosticAnalyzer analyzer,
        string source,
        string filePath,
        bool isTestProject = false,
        bool isAnalyzerProject = false)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, path: filePath);
        return await AnalyzeTreesAsync(
            analyzer,
            [syntaxTree],
            isTestProject,
            isAnalyzerProject);
    }

    public static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(
        DiagnosticAnalyzer analyzer,
        IReadOnlyList<(string Source, string FilePath)> sourceFiles,
        bool isTestProject = false,
        bool isAnalyzerProject = false)
    {
        var syntaxTrees = sourceFiles
                          .Select(f => CSharpSyntaxTree.ParseText(f.Source, path: f.FilePath))
                          .ToImmutableArray();
        return await AnalyzeTreesAsync(
            analyzer,
            syntaxTrees,
            isTestProject,
            isAnalyzerProject);
    }

    public static MetadataReference CompileToReference(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source);
        var compilation = CSharpCompilation.Create(
            "ReferencedAssembly",
            [tree],
            GetPlatformReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var ms = new MemoryStream();
        compilation.Emit(ms);
        return MetadataReference.CreateFromImage(ms.ToArray());
    }

    private static async Task<ImmutableArray<Diagnostic>> AnalyzeTreesAsync(
        DiagnosticAnalyzer analyzer,
        IEnumerable<SyntaxTree> syntaxTrees,
        bool isTestProject,
        bool isAnalyzerProject,
        IReadOnlyList<MetadataReference>? additionalReferences = null)
    {
        var references = additionalReferences != null
            ? GetPlatformReferences().AddRange(additionalReferences)
            : GetPlatformReferences();
        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            syntaxTrees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var optionsProvider = new TestAnalyzerConfigOptionsProvider(isTestProject, isAnalyzerProject);
        var analyzerOptions = new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty, optionsProvider);

        var compilationWithAnalyzers = compilation.WithAnalyzers(
            ImmutableArray.Create(analyzer),
            analyzerOptions);

        return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
    }

    private static ImmutableArray<MetadataReference> GetPlatformReferences()
    {
        return ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
               .Split(Path.PathSeparator)
               .Select(path => MetadataReference.CreateFromFile(path))
               .Cast<MetadataReference>()
               .ToImmutableArray();
    }
}
