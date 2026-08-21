using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.Documentation;

/// <summary>
///     Flags non-first members with XML doc comments that are not preceded by exactly one blank line.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MissingBlankLineBeforeXmlDocAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a non-first member with XML doc comments is not preceded
    ///     by a blank line.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static MissingBlankLineBeforeXmlDocAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.MissingBlankLineBeforeXmlDoc,
            "Missing blank line before XML doc comment",
            "Add a blank line before the XML doc comment. A code fix is available (dotnet format analyzers --diagnostics ATXCS054).",
            "Style",
            DiagnosticSeverity.Error,
            true,
            "Add one blank line immediately before the `///` XML doc comment block. Every non-first member that carries XML documentation must be preceded by exactly one blank line to visually separate it from the preceding member.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(
            AnalyzeMembers,
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKind.RecordDeclaration,
            SyntaxKind.RecordStructDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.NamespaceDeclaration,
            SyntaxKind.FileScopedNamespaceDeclaration,
            SyntaxKind.CompilationUnit);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeMemberList(
        SyntaxNodeAnalysisContext context,
        SyntaxList<MemberDeclarationSyntax> members)
    {
        for (var index = 1; index < members.Count; index++)
        {
            var location = FindMissingBlankLineLocation(members[index - 1], members[index]);
            if (location is not null)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, location));
            }
        }
    }

    private void AnalyzeMembers(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is TypeDeclarationSyntax typeDeclaration)
        {
            AnalyzeMemberList(context, typeDeclaration.Members);
        }
        else if (context.Node is NamespaceDeclarationSyntax namespaceDeclaration)
        {
            AnalyzeMemberList(context, namespaceDeclaration.Members);
        }
        else if (context.Node is FileScopedNamespaceDeclarationSyntax fileScopedNamespace)
        {
            AnalyzeMemberList(context, fileScopedNamespace.Members);
        }
        else if (context.Node is CompilationUnitSyntax compilationUnit)
        {
            AnalyzeMemberList(context, compilationUnit.Members);
        }
    }

    private Location? FindMissingBlankLineLocation(
        MemberDeclarationSyntax previous,
        MemberDeclarationSyntax current)
    {
        var sawNewline = false;

        foreach (var trivia in previous.GetLastToken().TrailingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                sawNewline = true;
            }
            else if (!trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                sawNewline = false;
            }
        }

        foreach (var trivia in current.GetFirstToken().LeadingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                if (sawNewline)
                {
                    return null;
                }

                sawNewline = true;
            }
            else if (trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                     trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
            {
                return trivia.GetLocation();
            }
            else if (!trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                sawNewline = false;
            }
        }

        return null;
    }
}
