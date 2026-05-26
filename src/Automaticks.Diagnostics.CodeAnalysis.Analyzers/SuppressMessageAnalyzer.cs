using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.Diagnostics.CodeAnalysis;

/// <summary>
///     Flags any use of the <c>[SuppressMessage]</c> attribute.
///     Diagnostic suppression via <c>[SuppressMessage]</c> hides rule violations instead of fixing their root cause.
///     Every analyzer rule must be addressed through proper code changes, analyzer updates, or legitimate
///     architectural exemptions built directly into the analyzer logic.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SuppressMessageAnalyzer : DiagnosticAnalyzer
{
    private const string SuppressMessageFullName = "SuppressMessageAttribute";
    private const string SuppressMessageShortName = "SuppressMessage";
    private static readonly DiagnosticDescriptor Rule;

    static SuppressMessageAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.DiagnosticsCodeAnalysis.SuppressMessage,
            "[SuppressMessage] is not allowed",
            "[SuppressMessage] suppresses a diagnostic instead of fixing it. Address the underlying issue or update the analyzer to handle this pattern as a legitimate exemption.",
            "Diagnostics.CodeAnalysis",
            DiagnosticSeverity.Error,
            true,
            "Remove the `[SuppressMessage]` attribute and fix the root cause of the flagged diagnostic. If the diagnostic is a genuine false positive, update the analyzer to recognize the pattern as valid by adding it to the exemption list in `IsExemptContext`. Never suppress diagnostics with `[SuppressMessage]`.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeAttribute, SyntaxKind.Attribute);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not AttributeSyntax attribute)
        {
            return;
        }

        var name = GetAttributeName(attribute);
        if (name is SuppressMessageShortName or SuppressMessageFullName)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, attribute.GetLocation()));
        }
    }

    private string GetAttributeName(AttributeSyntax attribute)
    {
        return attribute.Name switch
        {
            IdentifierNameSyntax identifier => identifier.Identifier.Text,
            QualifiedNameSyntax qualifiedName => qualifiedName.Right.Identifier.Text,
            _ => string.Empty
        };
    }
}
