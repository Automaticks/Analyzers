using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.IO;

namespace Automaticks.CSharp;

/// <summary>
///     Flags any top-level type whose name does not exactly match the base name of the file it is
///     declared in (case-sensitive comparison; all extensions are stripped before comparing, so
///     <c>DialogView.axaml.cs</c> compares as <c>DialogView</c>).
///     Nested types, generated code, and partial types where at least one declaration resides in
///     a correctly-named file are all exempt.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class FileNameMismatchAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a top-level type name does not match the file name.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.CSharp.FileNameMismatch,
        "Type name does not match file name",
        "Type name '{0}' does not match file name '{1}'",
        "Naming",
        DiagnosticSeverity.Error,
        true,
        "Rename the file so that its base name exactly matches the top-level type name (case-sensitive, extension stripped). Example: a file containing only `public class FooService` must be named `FooService.cs`. Alternatively, move the type to the correctly named file if it already exists.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

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

    private static void AnalyzeTypeDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (IsNestedType(context.Node))
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

        if (IsPartialMatchingAnyDeclaration(context, typeName))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, identifier.GetLocation(), typeName, fileName));
    }

    private static SyntaxToken GetIdentifier(SyntaxNode node)
    {
        if (node is BaseTypeDeclarationSyntax baseTypeDecl)
        {
            return baseTypeDecl.Identifier;
        }

        if (node is DelegateDeclarationSyntax delegateDecl)
        {
            return delegateDecl.Identifier;
        }

        return default;
    }

    private static bool IsNestedType(SyntaxNode node)
    {
        return node.Parent is TypeDeclarationSyntax or EnumDeclarationSyntax;
    }

    private static bool IsPartialMatchingAnyDeclaration(SyntaxNodeAnalysisContext context, string typeName)
    {
        if (context.Node is not TypeDeclarationSyntax typeDecl)
        {
            return false;
        }

        if (!typeDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            return false;
        }

        var symbol = context.SemanticModel.GetDeclaredSymbol(context.Node);
        if (symbol is null)
        {
            return false;
        }

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
    private static string StripAllExtensions(string filePath)
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
