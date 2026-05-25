using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace Automaticks.Diagnostics.CodeAnalysis;

/// <summary>
///     Flags suppression comments that hide diagnostics rather than fixing their root cause.
///     Two forms are forbidden:
///     <list type="bullet">
///         <item>
///             <description><c>#pragma warning disable</c> directives (ATXDC018).</description>
///         </item>
///         <item>
///             <description><c>// ReSharper disable</c> inline comments (ATXDC019).</description>
///         </item>
///     </list>
///     Both rules apply to all project types — production and test alike.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SuppressionCommentAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a <c>#pragma warning disable</c> directive is found.
    /// </summary>
    public static readonly DiagnosticDescriptor PragmaRule = new(
        DiagnosticIds.DiagnosticsCodeAnalysis.SuppressionPragma,
        "#pragma warning disable is not allowed",
        "#pragma warning disable suppresses a diagnostic instead of fixing it. Fix the root cause, or extend the .editorconfig allow-list if the rule is a genuine false positive.",
        "Diagnostics.CodeAnalysis",
        DiagnosticSeverity.Error,
        true,
        "Remove the `#pragma warning disable` directive and fix the root cause of the flagged diagnostic. If the diagnostic is a genuine false positive, add a structured exemption to the analyzer's `IsExemptContext` logic, or add a severity override in `.editorconfig`. Never suppress diagnostics with pragma comments.");

    /// <summary>
    ///     The diagnostic rule reported when a <c>// ReSharper disable</c> comment is found.
    /// </summary>
    public static readonly DiagnosticDescriptor ReSharperRule = new(
        DiagnosticIds.DiagnosticsCodeAnalysis.SuppressionReSharper,
        "// ReSharper disable is not allowed",
        "// ReSharper disable suppresses a diagnostic instead of fixing it. Fix the root cause, or add a severity override to .editorconfig if the rule is a genuine false positive.",
        "Diagnostics.CodeAnalysis",
        DiagnosticSeverity.Error,
        true,
        "Suppression via // ReSharper disable comments is forbidden. Address the underlying issue or, for rules that are genuine false positives, add a resharper_*_highlighting entry to .editorconfig.");

    private const string ReSharperDisablePrefix = "ReSharper disable";

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [PragmaRule, ReSharperRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
    }

    private static void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);

        foreach (var trivia in root.DescendantTrivia())
        {
            if (trivia.IsKind(SyntaxKind.PragmaWarningDirectiveTrivia))
            {
                var pragma = (PragmaWarningDirectiveTriviaSyntax)trivia.GetStructure()!;
                if (pragma.DisableOrRestoreKeyword.IsKind(SyntaxKind.DisableKeyword))
                {
                    context.ReportDiagnostic(Diagnostic.Create(PragmaRule, trivia.GetLocation()));
                }
            }
            else if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                var text = trivia.ToString().AsSpan();

                var trimmed = text.TrimStart('/').TrimStart(' ');
                if (trimmed.StartsWith(ReSharperDisablePrefix.AsSpan(), StringComparison.Ordinal))
                {
                    context.ReportDiagnostic(Diagnostic.Create(ReSharperRule, trivia.GetLocation()));
                }
            }
        }
    }
}
