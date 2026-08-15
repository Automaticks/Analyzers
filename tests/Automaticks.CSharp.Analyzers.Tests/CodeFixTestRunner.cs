using Automaticks.CSharp.Analyzers.Tests.Stubs;
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

namespace Automaticks.CSharp.Analyzers.Tests;

/// <summary>Test helper for applying Roslyn code fixes to in-memory source code.</summary>
public static class CodeFixTestRunner
{
    private const string DocumentName = "Test.cs";
    private const int MaxFixIterations = 64;
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

    /// <summary>Repeatedly applies the fix until no fixable diagnostic remains.</summary>
    /// <param name="request">The analyzer, provider, and source to fix.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that resolves to the fully fixed source text.</returns>
    public static async Task<string> ApplyAllFixesAsync(CodeFixRequest request, CancellationToken cancellationToken)
    {
        var document = CreateDocument(request);
        var original = document;
        for (var iteration = 0; iteration < MaxFixIterations; iteration++)
        {
            var diagnostics = await GetFixableDiagnosticsAsync(request, document, cancellationToken);
            if (diagnostics.Count == 0)
            {
                await AssertNoNewCompilerErrorsAsync(original, document, cancellationToken);
                var settled = await document.GetTextAsync(cancellationToken);
                return settled.ToString();
            }

            document = await ApplyOneAsync(request, document, diagnostics[0], cancellationToken);
        }

        throw new InvalidOperationException("The code fix did not converge within the iteration limit.");
    }

    /// <summary>Runs the provider's FixAllProvider over the requested scope.</summary>
    /// <param name="request">The analyzer, provider, and source to fix.</param>
    /// <param name="scope">The Fix All scope to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that resolves to the fixed source text.</returns>
    public static async Task<string> ApplyFixAllAsync(
        CodeFixRequest request,
        FixAllScope scope,
        CancellationToken cancellationToken)
    {
        var document = CreateDocument(request);
        var diagnostics = await GetFixableDiagnosticsAsync(request, document, cancellationToken);
        if (diagnostics.Count == 0)
        {
            throw new InvalidOperationException("The analyzer reported no fixable diagnostic for this source.");
        }

        var fixAllProvider = request.Provider.GetFixAllProvider()
            ?? throw new InvalidOperationException("The provider exposes no FixAllProvider.");
        var seedActions = await GetActionsAsync(request, document, diagnostics[0], cancellationToken);
        if (seedActions.Count == 0)
        {
            throw new InvalidOperationException($"The provider offered no fix for '{diagnostics[0].Id}'.");
        }

        var equivalenceKey = SelectAction(seedActions, request.EquivalenceKey).EquivalenceKey;
        var diagnosticProvider = new TestFixAllDiagnosticProvider(diagnostics);
        var fixAllContext = new FixAllContext(
            document,
            request.Provider,
            scope,
            equivalenceKey,
            request.Provider.FixableDiagnosticIds,
            diagnosticProvider,
            cancellationToken);
        var action = await fixAllProvider.GetFixAsync(fixAllContext)
            ?? throw new InvalidOperationException($"Fix All produced no action for scope '{scope}'.");
        var operations = await action.GetOperationsAsync(cancellationToken);
        foreach (var operation in operations)
        {
            if (operation is ApplyChangesOperation applyChanges)
            {
                var changed = applyChanges.ChangedSolution.GetDocument(document.Id)
                    ?? throw new InvalidOperationException("Fix All removed the document under test.");
                var fixedText = await changed.GetTextAsync(cancellationToken);
                return fixedText.ToString();
            }
        }

        throw new InvalidOperationException("Fix All produced no ApplyChangesOperation.");
    }

    /// <summary>Applies the fix to the first reported diagnostic only.</summary>
    /// <param name="request">The analyzer, provider, and source to fix.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that resolves to the fixed source text.</returns>
    public static async Task<string> ApplyFixAsync(CodeFixRequest request, CancellationToken cancellationToken)
    {
        var document = CreateDocument(request);
        var diagnostics = await GetFixableDiagnosticsAsync(request, document, cancellationToken);
        if (diagnostics.Count == 0)
        {
            throw new InvalidOperationException("The analyzer reported no fixable diagnostic for this source.");
        }

        var fixedDocument = await ApplyOneAsync(request, document, diagnostics[0], cancellationToken);
        await AssertNoNewCompilerErrorsAsync(document, fixedDocument, cancellationToken);
        var text = await fixedDocument.GetTextAsync(cancellationToken);
        return text.ToString();
    }

    /// <summary>Counts the diagnostics the supplied provider declares itself able to fix.</summary>
    /// <param name="request">The analyzer, provider, and source to inspect.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that resolves to the number of fixable diagnostics.</returns>
    public static async Task<int> CountFixableAsync(CodeFixRequest request, CancellationToken cancellationToken)
    {
        var document = CreateDocument(request);
        var diagnostics = await GetFixableDiagnosticsAsync(request, document, cancellationToken);
        return diagnostics.Count;
    }

    /// <summary>Counts how many code actions the provider offers for the first diagnostic.</summary>
    /// <param name="request">The analyzer, provider, and source to inspect.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that resolves to the number of offered code actions.</returns>
    public static async Task<int> CountOfferedActionsAsync(CodeFixRequest request, CancellationToken cancellationToken)
    {
        var document = CreateDocument(request);
        var diagnostics = await GetFixableDiagnosticsAsync(request, document, cancellationToken);
        if (diagnostics.Count == 0)
        {
            return 0;
        }

        var actions = await GetActionsAsync(request, document, diagnostics[0], cancellationToken);
        return actions.Count;
    }

    /// <summary>Applies the fix and returns the resulting document name.</summary>
    /// <param name="request">The analyzer, provider, and source to fix.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that resolves to the document name after the fix.</returns>
    public static async Task<string> GetFixedDocumentNameAsync(
        CodeFixRequest request,
        CancellationToken cancellationToken)
    {
        var document = CreateDocument(request);
        var diagnostics = await GetFixableDiagnosticsAsync(request, document, cancellationToken);
        if (diagnostics.Count == 0)
        {
            throw new InvalidOperationException("The analyzer reported no fixable diagnostic for this source.");
        }

        var fixedDocument = await ApplyOneAsync(request, document, diagnostics[0], cancellationToken);
        return fixedDocument.Name;
    }

    /// <summary>Lists the Fix All scopes the provider's FixAllProvider advertises.</summary>
    /// <param name="request">The provider to inspect.</param>
    /// <returns>The supported Fix All scopes.</returns>
    public static List<FixAllScope> GetSupportedFixAllScopes(CodeFixRequest request)
    {
        var fixAllProvider = request.Provider.GetFixAllProvider()
            ?? throw new InvalidOperationException("The provider exposes no FixAllProvider.");
        var scopes = new List<FixAllScope>();
        foreach (var scope in fixAllProvider.GetSupportedFixAllScopes())
        {
            scopes.Add(scope);
        }

        return scopes;
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

    private static Document CreateDocument(CodeFixRequest request)
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
        var sourceText = SourceText.From(request.Source);
        var filePath = request.FilePath;
        if (string.IsNullOrEmpty(filePath))
        {
            return project.AddDocument(DocumentName, sourceText);
        }

        var documentName = Path.GetFileName(filePath);
        return project.AddDocument(documentName, sourceText, null, filePath);
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
        return PlatformReferences;
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
