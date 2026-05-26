using Automaticks.Linq.Analyzers.Tests.Stubs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Linq.Analyzers.Tests;

/// <summary>Test helper for running Roslyn analyzers on in-memory source code.</summary>
public static class AnalyzerTestRunner
{
    /// <summary>Analyzes a single source string with default options.</summary>
    /// <param name="analyzer">The diagnostic analyzer to run.</param>
    /// <param name="source">The C# source code to analyze.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that resolves to the collection of reported diagnostics.</returns>
    public static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(
        DiagnosticAnalyzer analyzer,
        string source,
        CancellationToken cancellationToken)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        return await AnalyzeTreesAsync(analyzer, [syntaxTree], default, cancellationToken);
    }

    /// <summary>Analyzes a single source string with the specified options.</summary>
    /// <param name="analyzer">The diagnostic analyzer to run.</param>
    /// <param name="source">The C# source code to analyze.</param>
    /// <param name="options">The analysis configuration options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that resolves to the collection of reported diagnostics.</returns>
    public static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(
        DiagnosticAnalyzer analyzer,
        string source,
        AnalysisOptions options,
        CancellationToken cancellationToken)
    {
        var syntaxTree = string.IsNullOrEmpty(options.FilePath)
            ? CSharpSyntaxTree.ParseText(source)
            : CSharpSyntaxTree.ParseText(source, path: options.FilePath);
        return await AnalyzeTreesAsync(analyzer, [syntaxTree], options, cancellationToken);
    }

    /// <summary>Analyzes multiple source files with default options.</summary>
    /// <param name="analyzer">The diagnostic analyzer to run.</param>
    /// <param name="sourceFiles">The source files to analyze.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that resolves to the collection of reported diagnostics.</returns>
    public static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(
        DiagnosticAnalyzer analyzer,
        IReadOnlyList<SourceFile> sourceFiles,
        CancellationToken cancellationToken)
    {
        var syntaxTreesBuilder = ImmutableArray.CreateBuilder<SyntaxTree>(sourceFiles.Count);
        foreach (var sourceFile in sourceFiles)
        {
            syntaxTreesBuilder.Add(CSharpSyntaxTree.ParseText(sourceFile.Source, path: sourceFile.FilePath));
        }

        return await AnalyzeTreesAsync(analyzer, syntaxTreesBuilder.ToImmutable(), default, cancellationToken);
    }

    /// <summary>Compiles the given source to a <see cref="MetadataReference"/>.</summary>
    /// <param name="source">The C# source code to compile.</param>
    /// <returns>A metadata reference to the compiled assembly.</returns>
    public static MetadataReference CompileToReference(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source);
        var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
        var compilation = CSharpCompilation.Create(
            "ReferencedAssembly",
            [tree],
            GetPlatformReferences(),
            compilationOptions);
        using var memoryStream = new MemoryStream();
        compilation.Emit(memoryStream);
        return MetadataReference.CreateFromImage(memoryStream.ToArray());
    }

    private static async Task<ImmutableArray<Diagnostic>> AnalyzeTreesAsync(
        DiagnosticAnalyzer analyzer,
        IEnumerable<SyntaxTree> syntaxTrees,
        AnalysisOptions options,
        CancellationToken cancellationToken)
    {
        var references = options.AdditionalReferences != null
            ? GetPlatformReferences().AddRange(options.AdditionalReferences)
            : GetPlatformReferences();
        var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            syntaxTrees,
            references,
            compilationOptions);
        var configOptionsHolder = new TestAnalyzerConfigOptionsHolder(options.IsTestProject, options.IsAnalyzerProject);
        var analyzerOptions = new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty, configOptionsHolder);
        var compilationWithAnalyzers = compilation.WithAnalyzers(
            ImmutableArray.Create(analyzer),
            analyzerOptions);
        return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync(cancellationToken);
    }

    private static ImmutableArray<MetadataReference> GetPlatformReferences()
    {
        var trustedAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string
            ?? throw new InvalidOperationException("TRUSTED_PLATFORM_ASSEMBLIES not available");
        var paths = trustedAssemblies.Split(Path.PathSeparator);
        var builder = ImmutableArray.CreateBuilder<MetadataReference>(paths.Length);
        foreach (var path in paths)
        {
            builder.Add(MetadataReference.CreateFromFile(path));
        }
        return builder.ToImmutable();
    }
}
