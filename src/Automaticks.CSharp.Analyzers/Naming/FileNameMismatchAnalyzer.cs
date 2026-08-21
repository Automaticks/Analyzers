using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.IO;

namespace Automaticks.CSharp.Naming;

/// <summary>
///     Flags any top-level type whose name does not exactly match the base name of the file it is declared in (case-sensitive comparison; all extensions a...
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class FileNameMismatchAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a top-level type name does not match the file name.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static FileNameMismatchAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.FileNameMismatch,
            "Type name does not match file name",
            "Type name '{0}' does not match file name '{1}'. A code fix is available (dotnet format analyzers --diagnostics ATXCS031).",
            "Naming",
            DiagnosticSeverity.Error,
            true,
            "Rename the file so that its base name exactly matches the top-level type name (case-sensitive, extension stripped). Example: a file containing only `public class FooService` must be named `FooService.cs`. Alternatively, move the type to the correctly named file if it already exists.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(
            AnalyzeTypeDeclaration,
            SyntaxKind.ClassDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKind.RecordDeclaration,
            SyntaxKind.RecordStructDeclaration,
            SyntaxKind.EnumDeclaration,
            SyntaxKind.DelegateDeclaration);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [Rule];
        }
    }

    private void AnalyzeTypeDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (HasEnclosingTypeDeclaration(context.Node))
        {
            return;
        }

        var filePath = context.Node.SyntaxTree.FilePath;
        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }

        var identifier = GetIdentifier(context.Node);
        var typeName = identifier.Text;
        var fileName = StripAllExtensions(filePath);

        if (typeName == fileName)
        {
            return;
        }

        if (HasMatchingPartialDeclaration(context, typeName))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, identifier.GetLocation(), typeName, fileName));
    }

    private SyntaxToken GetIdentifier(SyntaxNode node)
    {
        if (node is BaseTypeDeclarationSyntax baseTypeDecl)
        {
            return baseTypeDecl.Identifier;
        }

        var delegateDecl = (node as DelegateDeclarationSyntax)!;
        return delegateDecl.Identifier;
    }

    private bool HasEnclosingTypeDeclaration(SyntaxNode node)
    {
        return node.Parent is TypeDeclarationSyntax or EnumDeclarationSyntax;
    }

    private bool HasMatchingPartialDeclaration(SyntaxNodeAnalysisContext context, string typeName)
    {
        if (context.Node is not TypeDeclarationSyntax typeDecl)
        {
            return false;
        }

        if (!typeDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            return false;
        }

        var symbol = context.SemanticModel.GetDeclaredSymbol(context.Node)!;

        foreach (var syntaxRef in symbol.DeclaringSyntaxReferences)
        {
            var refFilePath = syntaxRef.SyntaxTree.FilePath;
            if (string.IsNullOrEmpty(refFilePath))
            {
                continue;
            }

            if (typeName == StripAllExtensions(refFilePath))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Returns the file name with all extensions stripped.
    ///     For example, <c>DialogView.axaml.cs</c> → <c>DialogView</c>.
    /// </summary>
    private string StripAllExtensions(string filePath)
    {
        var name = Path.GetFileNameWithoutExtension(filePath);
        var next = Path.GetFileNameWithoutExtension(name);
        while (next != name)
        {
            name = next;
            next = Path.GetFileNameWithoutExtension(name);
        }

        return name;
    }
}
