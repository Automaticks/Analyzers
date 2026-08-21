using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.LanguageFeatures;

/// <summary>
///     Flags any member whose body is an expression body, forcing a block body instead.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ExpressionBodiedMethodAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a member uses an expression body.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static ExpressionBodiedMethodAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.ExpressionBodiedMethod,
            "Expression-bodied members are forbidden",
            "'{0}' must not use an expression body. Convert it to a block body. A code fix is available (dotnet format analyzers --diagnostics ATXCS075).",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "An expression body hides the body behind `=>`, so diffs grow noisier as logic changes and it becomes easy to miss that a member has grown non-trivial. Convert the member to a block body with an explicit `return` (or a bare statement where nothing is returned) instead.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeArrowExpressionClause, SyntaxKind.ArrowExpressionClause);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeArrowExpressionClause(SyntaxNodeAnalysisContext context)
    {
        var arrow = (context.Node as ArrowExpressionClauseSyntax)!;
        context.ReportDiagnostic(Diagnostic.Create(Rule, arrow.GetLocation(), GetMemberName(arrow.Parent!)));
    }

    private string GetMemberName(SyntaxNode member)
    {
        if (member is MethodDeclarationSyntax method)
        {
            return method.Identifier.Text;
        }

        if (member is LocalFunctionStatementSyntax localFunction)
        {
            return localFunction.Identifier.Text;
        }

        if (member is PropertyDeclarationSyntax property)
        {
            return property.Identifier.Text;
        }

        if (member is IndexerDeclarationSyntax)
        {
            return "this[]";
        }

        if (member is OperatorDeclarationSyntax operatorDeclaration)
        {
            return "operator " + operatorDeclaration.OperatorToken.Text;
        }

        if (member is ConversionOperatorDeclarationSyntax conversionOperator)
        {
            return "operator " + conversionOperator.Type.ToString();
        }

        if (member is ConstructorDeclarationSyntax constructor)
        {
            return constructor.Identifier.Text;
        }

        if (member is DestructorDeclarationSyntax destructor)
        {
            return "~" + destructor.Identifier.Text;
        }

        var accessor = (member as AccessorDeclarationSyntax)!;
        return accessor.Keyword.Text;
    }
}
