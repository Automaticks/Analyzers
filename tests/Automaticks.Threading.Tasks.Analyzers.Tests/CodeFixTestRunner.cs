using Automaticks.Threading.Tasks.Analyzers.Tests.Stubs;
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

namespace Automaticks.Threading.Tasks.Analyzers.Tests;

/// <summary>Test helper for applying Roslyn code fixes to in-memory source code.</summary>
public static class CodeFixTestRunner
{
    private const string DocumentName = "Test.cs";
    private const int MaxFixIterations = 64;
    private const string ProjectName = "TestProject";

    /// <summary>Repeatedly applies the fix until no fixable diagnostic remains.</summary>
    /// <param name="request">The analyzer, provider, and source to fix.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that resolves to the fully fixed source text.</returns>
    public static async Task<string> ApplyAllFixesAsync(CodeFixRequest request, CancellationToken cancellationToken)
    {
        var document = CreateDocument(request.Source);
        for (var iteration = 0; iteration < MaxFixIterations; iteration++)
        {
            var diagnostics = await GetFixableDiagnosticsAsync(request, document, cancellationToken);
            if (diagnostics.Count == 0)
            {
                var settled = await document.GetTextAsync(cancellationToken);
                return settled.ToString();
            }

            document = await ApplyOneAsync(request, document, diagnostics[0], cancellationToken);
        }

        throw new InvalidOperationException("The code fix did not converge within the iteration limit.");
    }

    /// <summary>Applies the fix to the first reported diagnostic only.</summary>
    /// <param name="request">The analyzer, provider, and source to fix.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that resolves to the fixed source text.</returns>
    public static async Task<string> ApplyFixAsync(CodeFixRequest request, CancellationToken cancellationToken)
    {
        var document = CreateDocument(request.Source);
        var diagnostics = await GetFixableDiagnosticsAsync(request, document, cancellationToken);
        if (diagnostics.Count == 0)
        {
            throw new InvalidOperationException("The analyzer reported no fixable diagnostic for this source.");
        }

        var fixedDocument = await ApplyOneAsync(request, document, diagnostics[0], cancellationToken);
        var text = await fixedDocument.GetTextAsync(cancellationToken);
        return text.ToString();
    }

    /// <summary>Counts the diagnostics the supplied provider declares itself able to fix.</summary>
    /// <param name="request">The analyzer, provider, and source to inspect.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that resolves to the number of fixable diagnostics.</returns>
    public static async Task<int> CountFixableAsync(CodeFixRequest request, CancellationToken cancellationToken)
    {
        var document = CreateDocument(request.Source);
        var diagnostics = await GetFixableDiagnosticsAsync(request, document, cancellationToken);
        return diagnostics.Count;
    }

    /// <summary>Counts how many code actions the provider offers for the first diagnostic.</summary>
    /// <param name="request">The analyzer, provider, and source to inspect.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that resolves to the number of offered code actions.</returns>
    public static async Task<int> CountOfferedActionsAsync(CodeFixRequest request, CancellationToken cancellationToken)
    {
        var document = CreateDocument(request.Source);
        var diagnostics = await GetFixableDiagnosticsAsync(request, document, cancellationToken);
        if (diagnostics.Count == 0)
        {
            return 0;
        }

        var actions = await GetActionsAsync(request, document, diagnostics[0], cancellationToken);
        return actions.Count;
    }

    private static async Task<Document> ApplyOneAsync(
        CodeFixRequest request,
        Document document,
        Diagnostic diagnostic,
        CancellationToken cancellationToken)
    {
        var actions = await GetActionsAsync(request, document, diagnostic, cancellationToken);
        if (actions.Count == 0)
        {
            throw new InvalidOperationException($"The provider offered no fix for '{diagnostic.Id}'.");
        }

        var selected = SelectAction(actions, request.EquivalenceKey);
        var operations = await selected.GetOperationsAsync(cancellationToken);
        foreach (var operation in operations)
        {
            if (operation is ApplyChangesOperation applyChanges)
            {
                var changed = applyChanges.ChangedSolution.GetDocument(document.Id);
                return changed ?? throw new InvalidOperationException("The fix removed the document under test.");
            }
        }

        throw new InvalidOperationException("The code action produced no ApplyChangesOperation.");
    }

    private static Document CreateDocument(string source)
    {
        var workspace = new AdhocWorkspace();
        var projectId = ProjectId.CreateNewId(ProjectName);
        var versionStamp = VersionStamp.Create();
        var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
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

        return matches;
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

    private static CodeAction SelectAction(List<CodeAction> actions, string? equivalenceKey)
    {
        if (equivalenceKey is null)
        {
            return actions[0];
        }

        foreach (var action in actions)
        {
            if (string.Equals(action.EquivalenceKey, equivalenceKey, StringComparison.Ordinal))
            {
                return action;
            }
        }

        throw new InvalidOperationException($"No code action matched equivalence key '{equivalenceKey}'.");
    }
}
