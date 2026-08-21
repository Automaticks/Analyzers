using Automaticks.CSharp.Complexity;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Text;

namespace Automaticks.CSharp.Documentation;

/// <summary>
///     Flags an XML documentation element whose prose exceeds the configured length.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class XmlDocLengthAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The <c>.editorconfig</c> key holding the maximum prose length.
    /// </summary>
    public const string MaxLengthKey = "automaticks.xml_doc_max_length";
    private const int DefaultMaxLength = 150;
    private static readonly DiagnosticDescriptor Rule;

    static XmlDocLengthAnalyzer()
    {
        Rule = new(
            DiagnosticIds.CSharp.XmlDocLength,
            "XML documentation element is too long",
            "The '<{0}>' documentation contains {1} characters of prose, which exceeds the maximum of {2}. Shorten it to a single clear statement.",
            "Documentation",
            DiagnosticSeverity.Error,
            true,
            "Keep each documentation element short enough to read at a glance. Move background, rationale, and worked examples into the repository documentation rather than the XML comment. The limit counts prose across every line of the element and is configurable through `automaticks.xml_doc_max_length` in `.editorconfig`.");
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(
            AnalyzeDocumentationComment,
            SyntaxKind.SingleLineDocumentationCommentTrivia,
            SyntaxKind.MultiLineDocumentationCommentTrivia);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [Rule];
        }
    }

    private void AnalyzeDocumentationComment(SyntaxNodeAnalysisContext context)
    {
        var documentation = (context.Node as DocumentationCommentTriviaSyntax)!;
        var maxLength = ConfigurableLimit.Read(context, MaxLengthKey, DefaultMaxLength);

        foreach (var node in documentation.Content)
        {
            if (node is not XmlElementSyntax element)
            {
                continue;
            }

            var length = MeasureProse(element);
            if (length <= maxLength)
            {
                continue;
            }

            context.ReportDiagnostic(Diagnostic.Create(
                Rule,
                element.StartTag.Name.GetLocation(),
                element.StartTag.Name.LocalName.ValueText,
                length,
                maxLength));
        }
    }

    private int CountCollapsed(StringBuilder text)
    {
        var count = 0;
        var pendingSpace = false;

        for (var index = 0; index < text.Length; index++)
        {
            if (char.IsWhiteSpace(text[index]))
            {
                pendingSpace = count > 0;
                continue;
            }

            if (pendingSpace)
            {
                count++;
                pendingSpace = false;
            }

            count++;
        }

        return count;
    }

    /// <summary>
    ///     Counts the readable characters of an element, collapsing each run of whitespace to a
    ///     single space so line wrapping does not change the result.
    /// </summary>
    private int MeasureProse(XmlElementSyntax element)
    {
        var builder = new StringBuilder();

        foreach (var node in element.Content)
        {
            foreach (var token in node.DescendantTokens())
            {
                if (!token.IsKind(SyntaxKind.XmlTextLiteralToken)
                    && !token.IsKind(SyntaxKind.XmlTextLiteralNewLineToken))
                {
                    continue;
                }

                builder.Append(token.ValueText);
            }
        }

        return CountCollapsed(builder);
    }
}
