using Automaticks.CommunityToolkit.Mvvm.Analyzers.Tests.Stubs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CommunityToolkit.Mvvm.Analyzers.Tests;

/// <summary>Test helper for applying Roslyn code fixes to in-memory source code.</summary>
public static class CodeFixTestRunner
{
    private const string DocumentName = "Test.cs";
    private const string ProjectName = "TestProject";
    private static readonly ImmutableArray<MetadataReference> PlatformReferences;

    static CodeFixTestRunner()
    {
        var trustedAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string
            ?? throw new InvalidOperationException("TRUSTED_PLATFORM_ASSEMBLIES not available");
        var paths = trustedAssemblies.Split(Path.PathSeparator);
        var builder = ImmutableArray.CreateBuilder<MetadataReference>(paths.Length);
        foreach (var path in paths)
        {
            builder.Add(MetadataReference.CreateFromFile(path));
        }

        PlatformReferences = builder.ToImmutable();
    }

    /// <summary>Applies the fix to the first reported diagnostic only.</summary>
    /// <param name="request">The analyzer, provider, and source to fix.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that resolves to the fixed source text.</returns>
    public static async Task<string> ApplyFixAsync(CodeFixRequest request, CancellationToken cancellationToken)
    {
        var document = CreateDocument(request.Source, request.OutputKind ?? OutputKind.DynamicallyLinkedLibrary);
        var diagnostics = await GetFixableDiagnosticsAsync(request, document, cancellationToken);
        if (diagnostics.Count == 0)
        {
            throw new InvalidOperationException("The analyzer reported no fixable diagnostic for this source.");
        }

        var actions = await GetActionsAsync(request, document, diagnostics[0], cancellationToken);
        if (actions.Count == 0)
        {
            throw new InvalidOperationException($"The provider offered no fix for '{diagnostics[0].Id}'.");
        }

        var operations = await actions[0].GetOperationsAsync(cancellationToken);
        foreach (var operation in operations)
        {
            if (operation is ApplyChangesOperation applyChanges)
            {
                var changed = applyChanges.ChangedSolution.GetDocument(document.Id)
                    ?? throw new InvalidOperationException("The fix removed the document under test.");
                await AssertNoNewCompilerErrorsAsync(document, changed, cancellationToken);
                var text = await changed.GetTextAsync(cancellationToken);
                return text.ToString();
            }
        }

        throw new InvalidOperationException("The code action produced no ApplyChangesOperation.");
    }

    /// <summary>Counts the diagnostics the supplied provider declares itself able to fix.</summary>
    /// <param name="request">The analyzer, provider, and source to inspect.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that resolves to the number of fixable diagnostics.</returns>
    public static async Task<int> CountFixableAsync(CodeFixRequest request, CancellationToken cancellationToken)
    {
        var document = CreateDocument(request.Source, request.OutputKind ?? OutputKind.DynamicallyLinkedLibrary);
        var diagnostics = await GetFixableDiagnosticsAsync(request, document, cancellationToken);
        return diagnostics.Count;
    }

    /// <summary>Counts how many code actions the provider offers for the first diagnostic.</summary>
    /// <param name="request">The analyzer, provider, and source to inspect.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that resolves to the number of offered code actions.</returns>
    public static async Task<int> CountOfferedActionsAsync(CodeFixRequest request, CancellationToken cancellationToken)
    {
        var document = CreateDocument(request.Source, request.OutputKind ?? OutputKind.DynamicallyLinkedLibrary);
        var diagnostics = await GetFixableDiagnosticsAsync(request, document, cancellationToken);
        if (diagnostics.Count == 0)
        {
            return 0;
        }

        var actions = await GetActionsAsync(request, document, diagnostics[0], cancellationToken);
        return actions.Count;
    }

    private static async Task AssertNoNewCompilerErrorsAsync(
        Document original,
        Document fixedDocument,
        CancellationToken cancellationToken)
    {
        var before = await CountCompilerErrorsAsync(original, cancellationToken);
        var after = await CountCompilerErrorsAsync(fixedDocument, cancellationToken);
        if (after <= before)
        {
            return;
        }

        var text = await fixedDocument.GetTextAsync(cancellationToken);
        throw new InvalidOperationException(
            $"The fix introduced {after - before} compiler error(s). Fixed source:{Environment.NewLine}{text}");
    }

    private static int CompareByLocation(Diagnostic left, Diagnostic right)
    {
        var leftPath = left.Location.SourceTree is null ? string.Empty : left.Location.SourceTree.FilePath;
        var rightPath = right.Location.SourceTree is null ? string.Empty : right.Location.SourceTree.FilePath;
        var pathComparison = string.CompareOrdinal(leftPath, rightPath);
        if (pathComparison != 0)
        {
            return pathComparison;
        }

        return left.Location.SourceSpan.Start.CompareTo(right.Location.SourceSpan.Start);
    }
    private static async Task<int> CountCompilerErrorsAsync(Document document, CancellationToken cancellationToken)
    {
        var compilation = await document.Project.GetCompilationAsync(cancellationToken)
            ?? throw new InvalidOperationException("The test project produced no compilation.");
        var count = 0;
        foreach (var diagnostic in compilation.GetDiagnostics(cancellationToken))
        {
            if (diagnostic.Severity == DiagnosticSeverity.Error)
            {
                count++;
            }
        }

        return count;
    }
    private static Document CreateDocument(string source, OutputKind outputKind)
    {
        var workspace = new AdhocWorkspace();
        var projectId = ProjectId.CreateNewId(ProjectName);
        var versionStamp = VersionStamp.Create();
        var compilationOptions = new CSharpCompilationOptions(outputKind);
        var projectInfo = ProjectInfo
            .Create(projectId, versionStamp, ProjectName, ProjectName, LanguageNames.CSharp)
            .WithMetadataReferences(GetPlatformReferences())
            .WithCompilationOptions(compilationOptions);
        var project = workspace.AddProject(projectInfo);
        var sourceText = SourceText.From(source);
        return project.AddDocument(DocumentName, sourceText);
    }

    private static async Task<List<CodeAction>> GetActionsAsync(
        CodeFixRequest request,
        Document document,
        Diagnostic diagnostic,
        CancellationToken cancellationToken)
    {
        var actions = new List<CodeAction>();
        var context = new CodeFixContext(document, diagnostic, (action, _) => actions.Add(action), cancellationToken);
        await request.Provider.RegisterCodeFixesAsync(context);
        return actions;
    }

    private static async Task<List<Diagnostic>> GetFixableDiagnosticsAsync(
        CodeFixRequest request,
        Document document,
        CancellationToken cancellationToken)
    {
        var compilation = await document.Project.GetCompilationAsync(cancellationToken)
            ?? throw new InvalidOperationException("The test project produced no compilation.");
        var configOptionsHolder = new TestAnalyzerConfigOptionsHolder(false, false);
        var analyzerOptions = new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty, configOptionsHolder);
        var analyzers = ImmutableArray.Create(request.Analyzer);
        var compilationWithAnalyzers = compilation.WithAnalyzers(analyzers, analyzerOptions);
        var reported = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync(cancellationToken);
        var fixableIds = request.Provider.FixableDiagnosticIds;
        var matches = new List<Diagnostic>();
        foreach (var diagnostic in reported)
        {
            if (fixableIds.Contains(diagnostic.Id))
            {
                matches.Add(diagnostic);
            }
        }

        matches.Sort(CompareByLocation);
        return matches;
    }

    private static ImmutableArray<MetadataReference> GetPlatformReferences()
    {
        return PlatformReferences;
    }
}
