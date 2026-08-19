using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.LanguageFeatures;

/// <summary>
///     Flags <c>using</c>-alias directives and <c>extern alias</c> directives.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AliasDirectiveAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a <c>using</c> or <c>extern</c> directive declares an alias.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static AliasDirectiveAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.AliasDirective,
            "Alias directive is forbidden",
            "The alias '{0}' is forbidden. Reference the real type or namespace name directly instead.",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "An alias (`using X = Y;`, `using X = Namespace;`, or `extern alias X;`) replaces a discoverable, searchable name with a private nickname understood only in this file. Remove the alias and reference the real type or namespace name at each call site; if that real name is unsuitable, rename the underlying type declaration instead of hiding it behind an alias.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(
            AnalyzeNode,
            SyntaxKind.UsingDirective,
            SyntaxKind.ExternAliasDirective);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeExternAliasDirective(SyntaxNodeAnalysisContext context, ExternAliasDirectiveSyntax externAliasDirective)
    {
        context.ReportDiagnostic(Diagnostic.Create(
            Rule,
            externAliasDirective.GetLocation(),
            externAliasDirective.Identifier.ValueText));
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        switch (context.Node)
        {
            case UsingDirectiveSyntax usingDirective:
                AnalyzeUsingDirective(context, usingDirective);
                break;
            case ExternAliasDirectiveSyntax externAliasDirective:
                AnalyzeExternAliasDirective(context, externAliasDirective);
                break;
        }
    }

    private void AnalyzeUsingDirective(SyntaxNodeAnalysisContext context, UsingDirectiveSyntax usingDirective)
    {
        if (usingDirective.Alias is null)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            Rule,
            usingDirective.GetLocation(),
            usingDirective.Alias.Name.Identifier.ValueText));
    }
}
