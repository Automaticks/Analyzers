using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.LanguageFeatures;

/// <summary>
///     Flags every use of the <c>global::</c> alias qualifier.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class GlobalAliasQualifierAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a name is qualified with the <c>global::</c> alias.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static GlobalAliasQualifierAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.GlobalAliasQualifier,
            "'global::' alias qualifier is forbidden",
            "'{0}' uses the 'global::' alias qualifier, which is forbidden.",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "`global::` is a disambiguation crutch that papers over a name colliding with something else in scope. Remove `global::` and make the surrounding names unambiguous on their own; if a real collision exists, rename the colliding type instead of qualifying around it.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.AliasQualifiedName);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [Rule];
        }
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var aliasQualifiedName = (context.Node as AliasQualifiedNameSyntax)!;
        if (!aliasQualifiedName.Alias.Identifier.IsKind(SyntaxKind.GlobalKeyword))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, aliasQualifiedName.GetLocation(), aliasQualifiedName.ToString()));
    }
}
