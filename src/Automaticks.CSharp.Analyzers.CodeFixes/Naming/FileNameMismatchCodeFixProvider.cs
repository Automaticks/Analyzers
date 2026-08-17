using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.CodeFixes.Naming;

/// <summary>
///     Renames the file so it matches the type reported by ATXCS031. The rule can also be
///     satisfied by moving the type to another file, which the fix leaves to the developer.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FileNameMismatchCodeFixProvider))]
[Shared]
public sealed class FileNameMismatchCodeFixProvider : CodeFixProvider
{
    private const string TitlePrefix = "Rename the file to ";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticIds.CSharp.FileNameMismatch];

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
        var filePath = context.Document.FilePath;
        if (root is null || filePath is null || filePath.Length == 0)
        {
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
            var declaration = token.Parent;
            var typeName = GetTypeName(declaration);
            if (typeName.Length == 0)
            {
                continue;
            }

            var newFileName = BuildFileName(filePath, typeName);
            var title = TitlePrefix + newFileName;
            var action = CodeAction.Create(
                title,
                cancellationToken => RenameFileAsync(context.Document, newFileName, cancellationToken),
                title);
            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private string BuildFileName(string filePath, string typeName)
    {
        var fileName = Path.GetFileName(filePath);
        var firstDot = fileName.IndexOf('.');
        var suffix = firstDot >= 0 ? fileName.Substring(firstDot) : string.Empty;
        return typeName + suffix;
    }

    private string GetTypeName(SyntaxNode? declaration)
    {
        if (declaration is BaseTypeDeclarationSyntax typeDeclaration)
        {
            return typeDeclaration.Identifier.ValueText;
        }

        if (declaration is DelegateDeclarationSyntax delegateDeclaration)
        {
            return delegateDeclaration.Identifier.ValueText;
        }

        return string.Empty;
    }

    private Task<Solution> RenameFileAsync(
        Document document,
        string newFileName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var solution = document.Project.Solution;
        var directory = Path.GetDirectoryName(document.FilePath);
        var updated = solution.WithDocumentName(document.Id, newFileName);
        if (!string.IsNullOrEmpty(directory))
        {
            var newPath = Path.Combine(directory, newFileName);
            updated = updated.WithDocumentFilePath(document.Id, newPath);
        }

        return Task.FromResult(updated);
    }
}
