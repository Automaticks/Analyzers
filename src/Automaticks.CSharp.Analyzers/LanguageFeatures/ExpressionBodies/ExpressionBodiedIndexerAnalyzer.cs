using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.LanguageFeatures.ExpressionBodies;

/// <summary>
///     Flags indexers whose body is an expression body, forcing a block body instead.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ExpressionBodiedIndexerAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when an indexer uses an expression body.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static ExpressionBodiedIndexerAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.ExpressionBodiedIndexer,
            "Expression-bodied indexers are forbidden",
            "'{0}' must not use an expression body. Convert it to a block body. A code fix is available (dotnet format analyzers --diagnostics ATXCS078).",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "An expression body hides the body behind `=>`, so diffs grow noisier as logic changes and it becomes easy to miss that an indexer has grown non-trivial. Convert it to a block body instead.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(
            Analyze,
            SyntaxKind.IndexerDeclaration);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [Rule];
        }
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        var declaration = (context.Node as IndexerDeclarationSyntax)!;
        if (declaration.ExpressionBody is null)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, declaration.ExpressionBody.GetLocation(), "this[]"));
    }
}