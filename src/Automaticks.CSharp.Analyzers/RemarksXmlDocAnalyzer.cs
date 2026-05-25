using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp;

/// <summary>
///     Flags <c>&lt;remarks&gt;</c> XML documentation elements. Use a well-written
///     <c>&lt;summary&gt;</c> instead of appending supplementary prose in a remarks block.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RemarksXmlDocAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a <c>&lt;remarks&gt;</c> element is found in an XML doc comment.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.CSharp.RemarksXmlDoc,
        "<remarks> is not allowed in XML documentation",
        "<remarks> is not allowed. Fold the content into the <summary> element instead.",
        "CSharp",
        DiagnosticSeverity.Error,
        true,
        "Remove the `<remarks>` XML documentation element. Merge any useful content from `<remarks>` into the `<summary>` element instead. The `<remarks>` element is not permitted in this codebase.");

    private const string RemarksTagName = "remarks";

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
                element.StartTag.Name.LocalName.ValueText.Equals(RemarksTagName, System.StringComparison.Ordinal))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, element.StartTag.GetLocation()));
            }
            else if (node is XmlEmptyElementSyntax emptyElement &&
                     emptyElement.Name.LocalName.ValueText.Equals(RemarksTagName, System.StringComparison.Ordinal))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, emptyElement.GetLocation()));
            }
        }
    }
}
