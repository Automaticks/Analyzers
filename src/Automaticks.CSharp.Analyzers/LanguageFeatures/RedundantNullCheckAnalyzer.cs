using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.LanguageFeatures;

/// <summary>
///     Flags redundant null-checks on parameters that are already declared as non-nullable reference types.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RedundantNullCheckAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a non-nullable parameter is guarded with a redundant <see cref="System.ArgumentNullException" /> check.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static RedundantNullCheckAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.RedundantNullCheck,
            "Redundant null check on non-nullable parameter",
            "Parameter '{0}' is declared as a non-nullable reference type but is guarded with a null check that throws ArgumentNullException. Remove the null check, or change the parameter type to '{0}?' if null is a valid input. A code fix is available (dotnet format analyzers --diagnostics ATXCS014).",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "Remove the `ArgumentNullException` null check on this parameter. The parameter is declared as a non-nullable reference type, so the type system already enforces non-null at the call site. If `null` is a valid input for this parameter, change the parameter type to `T?` and keep the guard.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeCoalesce, SyntaxKind.CoalesceExpression);
        context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeCoalesce(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not BinaryExpressionSyntax coalesceNode || coalesceNode.Right is not ThrowExpressionSyntax throwExpr)
        {
            return;
        }

        if (!HasArgumentNullExceptionType(throwExpr.Expression, context.SemanticModel))
        {
            return;
        }

        if (!HasNonNullableReferenceParameter(coalesceNode.Left, context.SemanticModel, out var paramName))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, coalesceNode.GetLocation(), paramName));
    }

    private void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not IfStatementSyntax ifStatement || ifStatement.Else is not null)
        {
            return;
        }

        var paramExpression = GetNullCheckedExpression(ifStatement.Condition);
        if (paramExpression is null)
        {
            return;
        }

        var throwExpression = GetSingleThrowExpression(ifStatement.Statement);
        if (throwExpression is null)
        {
            return;
        }

        if (!HasArgumentNullExceptionType(throwExpression, context.SemanticModel))
        {
            return;
        }

        if (!HasNonNullableReferenceParameter(paramExpression, context.SemanticModel, out var paramName))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, ifStatement.GetLocation(), paramName));
    }

    private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InvocationExpressionSyntax invocationNode || invocationNode.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        if (memberAccess.Name.Identifier.Text != "ThrowIfNull")
        {
            return;
        }

        if (context.SemanticModel.GetSymbolInfo(memberAccess.Expression).Symbol is not INamedTypeSymbol typeSymbol)
        {
            return;
        }

        if (typeSymbol.ToDisplayString() != "System.ArgumentNullException")
        {
            return;
        }

        if (invocationNode.ArgumentList.Arguments.Count == 0)
        {
            return;
        }

        var firstArg = invocationNode.ArgumentList.Arguments[0].Expression;
        if (!HasNonNullableReferenceParameter(firstArg, context.SemanticModel, out var paramName))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, invocationNode.GetLocation(), paramName));
    }

    private ExpressionSyntax? GetNullCheckedExpression(ExpressionSyntax condition)
    {
        if (condition is IsPatternExpressionSyntax { Pattern: ConstantPatternSyntax constantPattern } isPattern &&
            constantPattern.Expression.IsKind(SyntaxKind.NullLiteralExpression))
        {
            return isPattern.Expression;
        }

        if (condition is not BinaryExpressionSyntax binary || !binary.IsKind(SyntaxKind.EqualsExpression))
        {
            return null;
        }

        if (binary.Right.IsKind(SyntaxKind.NullLiteralExpression))
        {
            return binary.Left;
        }

        if (binary.Left.IsKind(SyntaxKind.NullLiteralExpression))
        {
            return binary.Right;
        }

        return null;
    }

    private ExpressionSyntax? GetSingleThrowExpression(StatementSyntax statement)
    {
        var body = statement;
        if (body is BlockSyntax block)
        {
            if (block.Statements.Count != 1)
            {
                return null;
            }

            body = block.Statements[0];
        }

        return body is ThrowStatementSyntax throwStatement ? throwStatement.Expression : null;
    }

    private bool HasArgumentNullExceptionType(ExpressionSyntax expression, SemanticModel semanticModel)
    {
        return semanticModel.GetTypeInfo(expression).Type?.ToDisplayString() == "System.ArgumentNullException";
    }

    private bool HasNonNullableReferenceParameter(
        ExpressionSyntax expression,
        SemanticModel semanticModel,
        out string parameterName)
    {
        parameterName = string.Empty;

        if (expression is not IdentifierNameSyntax identifier)
        {
            return false;
        }

        if (semanticModel.GetSymbolInfo(identifier).Symbol is not IParameterSymbol paramSymbol)
        {
            return false;
        }

        if (!paramSymbol.Type.IsReferenceType)
        {
            return false;
        }

        if (paramSymbol.NullableAnnotation != NullableAnnotation.NotAnnotated)
        {
            return false;
        }

        parameterName = paramSymbol.Name;
        return true;
    }
}
