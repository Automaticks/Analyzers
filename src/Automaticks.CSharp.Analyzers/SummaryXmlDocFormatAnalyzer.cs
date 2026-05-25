using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace Automaticks.CSharp;

/// <summary>
///     Flags <c>&lt;summary&gt;</c> XML documentation elements whose content is not on a
///     new line or is not indented with exactly 4 spaces.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SummaryXmlDocFormatAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a <c>&lt;summary&gt;</c> element has its content
    ///     on the same line as the opening tag, or is not indented with exactly 4 spaces.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.CSharp.SummaryXmlDocFormat,
        "<summary> content must start on a new line and be indented with 4 spaces",
        "<summary> content must begin on a new line and each content line must be indented with exactly 4 spaces after '/// '",
        "CSharp",
        DiagnosticSeverity.Warning,
        true,
        "The `<summary>` content must start on a new line after `/// <summary>` and every content line must be indented with exactly four spaces after `/// `. Correct format: `/// <summary>` / `///     One-sentence description ending with a period.` / `/// </summary>`. Adjust the indentation or line breaks of the `<summary>` block to match this format.");

    private const string SummaryTagName = "summary";
    private const string RequiredIndentation = "     ";

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

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

    private static void AnalyzeDocComment(SyntaxNodeAnalysisContext context)
    {
        var docComment = (DocumentationCommentTriviaSyntax)context.Node;

        foreach (var node in docComment.Content)
        {
            if (node is XmlElementSyntax element &&
                element.StartTag.Name.LocalName.ValueText.Equals(SummaryTagName, StringComparison.Ordinal) &&
                !IsFormatValid(element))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, element.StartTag.GetLocation()));
            }
        }
    }

    private static bool IsFormatValid(XmlElementSyntax element)
    {
        var isFirstTextNode = true;

        foreach (var contentNode in element.Content)
        {
            if (contentNode is not XmlTextSyntax xmlText)
            {
                continue;
            }

            var tokens = xmlText.TextTokens;
            if (tokens.Count == 0)
            {
                continue;
            }

            if (isFirstTextNode)
            {
                isFirstTextNode = false;
                if (!tokens[0].IsKind(SyntaxKind.XmlTextLiteralNewLineToken))
                {
                    return false;
                }
            }

            if (!AreLineStartsIndented(tokens))
            {
                return false;
            }
        }

        return true;
    }

    private static bool AreLineStartsIndented(SyntaxTokenList tokens)
    {
        for (var i = 1; i < tokens.Count; i++)
        {
            var token = tokens[i];
            if (!token.IsKind(SyntaxKind.XmlTextLiteralToken))
            {
                continue;
            }

            if (!tokens[i - 1].IsKind(SyntaxKind.XmlTextLiteralNewLineToken))
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
}
