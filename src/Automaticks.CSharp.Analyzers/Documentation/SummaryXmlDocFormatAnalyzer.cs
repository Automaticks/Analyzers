using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace Automaticks.CSharp.Documentation;

/// <summary>
///     Flags <c>&lt;summary&gt;</c> XML documentation elements whose content is not on a
///     new line or is not indented with exactly 4 spaces.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SummaryXmlDocFormatAnalyzer : DiagnosticAnalyzer
{
    private const string RequiredIndentation = "     ";
    private const string SummaryTagName = "summary";

    /// <summary>
    ///     The diagnostic rule reported when a <c>&lt;summary&gt;</c> element has its content
    ///     on the same line as the opening tag, or is not indented with exactly 4 spaces.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static SummaryXmlDocFormatAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.SummaryXmlDocFormat,
            "<summary> content must start on a new line and be indented with 4 spaces",
            "<summary> content must begin on a new line and each content line must be indented with exactly 4 spaces after '/// '. A code fix is available (dotnet format analyzers --diagnostics ATXCS050).",
            "CSharp",
            DiagnosticSeverity.Warning,
            true,
            "The `<summary>` content must start on a new line after `/// <summary>` and every content line must be indented with exactly four spaces after `/// `. Correct format: `/// <summary>` / `///     One-sentence description ending with a period.` / `/// </summary>`. Adjust the indentation or line breaks of the `<summary>` block to match this format.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(
            AnalyzeDocComment,
            SyntaxKind.SingleLineDocumentationCommentTrivia,
            SyntaxKind.MultiLineDocumentationCommentTrivia);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeDocComment(SyntaxNodeAnalysisContext context)
    {
        var docComment = (context.Node as DocumentationCommentTriviaSyntax)!;
        foreach (var node in docComment.Content)
        {
            if (node is XmlElementSyntax element &&
                element.StartTag.Name.LocalName.ValueText.Equals(SummaryTagName, StringComparison.Ordinal) &&
                !HasValidFormat(element))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, element.StartTag.GetLocation()));
            }
        }
    }

    private bool HasIndentedLineStarts(SyntaxTokenList tokens)
    {
        for (var tokenIndex = 1; tokenIndex < tokens.Count; tokenIndex++)
        {
            var token = tokens[tokenIndex];
            if (!token.IsKind(SyntaxKind.XmlTextLiteralToken))
            {
                continue;
            }

            if (!tokens[tokenIndex - 1].IsKind(SyntaxKind.XmlTextLiteralNewLineToken))
            {
                continue;
            }

            var text = token.ValueText;
            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            if (!text.StartsWith(RequiredIndentation, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private bool HasValidFormat(XmlElementSyntax element)
    {
        var isFirstTextNode = true;

        foreach (var contentNode in element.Content)
        {
            if (contentNode is not XmlTextSyntax xmlText)
            {
                continue;
            }

            var tokens = xmlText.TextTokens;
            if (isFirstTextNode)
            {
                isFirstTextNode = false;
                if (!tokens[0].IsKind(SyntaxKind.XmlTextLiteralNewLineToken))
                {
                    return false;
                }
            }

            if (!HasIndentedLineStarts(tokens))
            {
                return false;
            }
        }

        return true;
    }
}
