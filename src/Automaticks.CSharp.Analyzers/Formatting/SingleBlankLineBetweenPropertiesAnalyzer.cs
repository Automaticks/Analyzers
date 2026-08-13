using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.Formatting;

/// <summary>
///     Enforces exactly one blank line between a property or indexer declaration and the member
///     immediately adjacent to it (before or after) inside any type declaration.
///     Zero blank lines (cramped) and two or more blank lines (over-spaced) are both violations.
///     A comment line between two members resets the blank-line counter — a blank line that precedes
///     a doc-comment preamble still counts as the required separator.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SingleBlankLineBetweenPropertiesAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     Fires when a property or indexer is immediately adjacent to another member with no blank line between them.
    /// </summary>
    public static readonly DiagnosticDescriptor TooFewBlankLinesRule;

    static SingleBlankLineBetweenPropertiesAnalyzer()
    {
        TooFewBlankLinesRule = new(
            DiagnosticIds.CSharp.SingleBlankLineBetweenProperties,
            "Missing blank line adjacent to a property or indexer declaration",
            "Add a blank line between this property or indexer and the adjacent member declaration",
            "Style",
            DiagnosticSeverity.Error,
            true,
            "Add exactly one blank line between the property or indexer declaration and the adjacent member declaration. Property and indexer declarations must be visually separated from neighboring members by one blank line on each side that borders a different member.");
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
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [TooFewBlankLinesRule];

    private void AnalyzeTypeDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not TypeDeclarationSyntax typeDecl)
        {
            return;
        }

        var members = typeDecl.Members;

        for (var index = 0; index < members.Count - 1; index++)
        {
            var first = members[index];
            var second = members[index + 1];

            if (!HasPropertyOrIndexer(first) && !HasPropertyOrIndexer(second))
            {
                continue;
            }

            var hasBlankLine = HasBlankLine(first, second);

            if (!hasBlankLine)
            {
                context.ReportDiagnostic(Diagnostic.Create(TooFewBlankLinesRule, second.GetFirstToken().GetLocation()));
            }
        }
    }

    private bool HasBlankLine(MemberDeclarationSyntax first, MemberDeclarationSyntax second)
    {
        var trailingTrivia = first.GetLastToken().TrailingTrivia;
        var leadingTrivia = second.GetFirstToken().LeadingTrivia;
        var sawNewline = false;

        foreach (var trivia in trailingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                if (sawNewline)
                {
                    return true;
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
                    return true;
                }

                sawNewline = true;
            }
            else if (!trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                sawNewline = false;
            }
        }

        return false;
    }

    private bool HasPropertyOrIndexer(MemberDeclarationSyntax member)
    {
        return member is PropertyDeclarationSyntax or IndexerDeclarationSyntax;
    }
}
