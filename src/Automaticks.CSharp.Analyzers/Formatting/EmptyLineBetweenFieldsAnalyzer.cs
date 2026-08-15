using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.Formatting;

/// <summary>
///     Flags blank lines between two directly adjacent field or constant declarations inside any type declaration.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EmptyLineBetweenFieldsAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a blank line is found between two adjacent field or constant declarations.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static EmptyLineBetweenFieldsAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.EmptyLineBetweenFields,
            "Empty lines between adjacent field or constant declarations are forbidden",
            "Remove the blank line between adjacent field or constant declarations. A code fix is available (dotnet format analyzers --diagnostics ATXCS039).",
            "Style",
            DiagnosticSeverity.Error,
            true,
            "Remove the blank line between adjacent field or constant declarations. All consecutive fields and constants should be grouped without blank lines between them. Use a blank line only to separate fields from members of a different kind (e.g., between the last field and the first property).");
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
            SyntaxKind.StructDeclaration,
            SyntaxKind.RecordDeclaration,
            SyntaxKind.RecordStructDeclaration,
            SyntaxKind.InterfaceDeclaration);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeTypeDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not TypeDeclarationSyntax typeDecl)
        {
            return;
        }

        var members = typeDecl.Members;

        for (var index = 0; index < members.Count - 1; index++)
        {
            if (members[index] is not FieldDeclarationSyntax firstField)
            {
                continue;
            }

            if (members[index + 1] is not FieldDeclarationSyntax secondField)
            {
                continue;
            }

            var blankLineLocation = FindBlankLineLocation(firstField, secondField);
            if (blankLineLocation is not null)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, blankLineLocation));
            }
        }
    }

    private Location? FindBlankLineLocation(FieldDeclarationSyntax first, FieldDeclarationSyntax second)
    {
        var trailingTrivia = first.GetLastToken().TrailingTrivia;
        var leadingTrivia = second.GetFirstToken().LeadingTrivia;

        if (HasXmlDocComment(leadingTrivia))
        {
            return null;
        }

        var sawNewline = false;

        foreach (var trivia in trailingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                if (sawNewline)
                {
                    return trivia.GetLocation();
                }

                sawNewline = true;
            }
            else if (!trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                sawNewline = false;
            }
        }

        foreach (var trivia in leadingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                if (sawNewline)
                {
                    return trivia.GetLocation();
                }

                sawNewline = true;
            }
            else if (!trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                sawNewline = false;
            }
        }

        return null;
    }

    private bool HasXmlDocComment(SyntaxTriviaList leadingTrivia)
    {
        foreach (var trivia in leadingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
            {
                return true;
            }
        }

        return false;
    }
}
