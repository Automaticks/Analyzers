using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.LanguageFeatures.ExpressionBodies;

/// <summary>
///     Flags methods whose body is an expression body, forcing a block body instead.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ExpressionBodiedMethodAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a method uses an expression body.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static ExpressionBodiedMethodAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.ExpressionBodiedMethod,
            "Expression-bodied methods are forbidden",
            "'{0}' must not use an expression body. Convert it to a block body. A code fix is available (dotnet format analyzers --diagnostics ATXCS075).",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "An expression body hides the return behind `=>`, so diffs grow noisier as logic changes and it becomes easy to miss that a method has grown non-trivial. Convert it to a block body with an explicit `return` (or a bare statement for `void`/`async Task`) instead.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.MethodDeclaration);
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
        var declaration = (context.Node as MethodDeclarationSyntax)!;
        if (declaration.ExpressionBody is null)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, declaration.ExpressionBody.GetLocation(), declaration.Identifier.Text));
    }
}