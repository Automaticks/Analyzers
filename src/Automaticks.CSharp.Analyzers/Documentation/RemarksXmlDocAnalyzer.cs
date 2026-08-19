using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace Automaticks.CSharp.Documentation;

/// <summary>
///     Flags <c>&lt;remarks&gt;</c> XML documentation elements in favor of <c>&lt;summary&gt;</c>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RemarksXmlDocAnalyzer : DiagnosticAnalyzer
{
    private const string RemarksTagName = "remarks";

    /// <summary>
    ///     The diagnostic rule reported when a <c>&lt;remarks&gt;</c> element is found in an XML doc comment.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static RemarksXmlDocAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.RemarksXmlDoc,
            "<remarks> is not allowed in XML documentation",
            "<remarks> is not allowed. Fold the content into the <summary> element instead. A code fix is available (dotnet format analyzers --diagnostics ATXCS038).",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "Remove the `<remarks>` XML documentation element. Merge any useful content from `<remarks>` into the `<summary>` element instead. The `<remarks>` element is not permitted in this codebase.");
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
        if (context.Node is not DocumentationCommentTriviaSyntax docComment)
        {
            return;
        }

        foreach (var node in docComment.Content)
        {
            if (node is XmlElementSyntax element &&
                element.StartTag.Name.LocalName.ValueText.Equals(RemarksTagName, StringComparison.Ordinal))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, element.StartTag.GetLocation()));
            }
            else if (node is XmlEmptyElementSyntax emptyElement &&
                     emptyElement.Name.LocalName.ValueText.Equals(RemarksTagName, StringComparison.Ordinal))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, emptyElement.GetLocation()));
            }
        }
    }
}
