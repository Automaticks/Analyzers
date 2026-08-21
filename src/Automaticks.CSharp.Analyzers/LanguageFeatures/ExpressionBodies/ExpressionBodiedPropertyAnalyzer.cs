using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.LanguageFeatures.ExpressionBodies;

/// <summary>
///     Flags properties whose body is an expression body, forcing a block body instead.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ExpressionBodiedPropertyAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a property uses an expression body.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static ExpressionBodiedPropertyAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.ExpressionBodiedProperty,
            "Expression-bodied properties are forbidden",
            "'{0}' must not use an expression body. Convert it to a block body. A code fix is available (dotnet format analyzers --diagnostics ATXCS077).",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "An expression body hides the body behind `=>`, so diffs grow noisier as logic changes and it becomes easy to miss that a property has grown non-trivial. Convert it to a block body instead.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(
            Analyze,
            SyntaxKind.PropertyDeclaration);
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
        var declaration = (context.Node as PropertyDeclarationSyntax)!;
        if (declaration.ExpressionBody is null)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, declaration.ExpressionBody.GetLocation(), declaration.Identifier.Text));
    }
}