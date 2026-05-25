using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp;

/// <summary>
///     Flags redundant null-checks (null-coalescing throw, <c>if (x == null) throw</c>, or
///     <c>ArgumentNullException.ThrowIfNull</c>) on parameters that are already declared as
///     non-nullable reference types. The type system already expresses the no-null contract.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RedundantNullCheckAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a non-nullable parameter is guarded with a redundant
    ///     <see cref="System.ArgumentNullException" /> check.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.CSharp.RedundantNullCheck,
        "Redundant null check on non-nullable parameter",
        "Parameter '{0}' is declared as a non-nullable reference type but is guarded with a null check that throws ArgumentNullException. Remove the null check, or change the parameter type to '{0}?' if null is a valid input.",
        "CSharp",
        DiagnosticSeverity.Error,
        true,
        "Remove the `ArgumentNullException` null check on this parameter. The parameter is declared as a non-nullable reference type, so the type system already enforces non-null at the call site. If `null` is a valid input for this parameter, change the parameter type to `T?` and keep the guard.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeCoalesce, SyntaxKind.CoalesceExpression);
        context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeCoalesce(SyntaxNodeAnalysisContext context)
    {
        var coalesceNode = (BinaryExpressionSyntax)context.Node;

        if (coalesceNode.Right is not ThrowExpressionSyntax throwExpr)
        {
            return;
        }

        if (!IsArgumentNullException(throwExpr.Expression, context.SemanticModel))
        {
            return;
        }

        if (!TryGetNonNullableReferenceParameter(coalesceNode.Left, context.SemanticModel, out var paramName))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, coalesceNode.GetLocation(), paramName));
    }

    private static void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
    {
        var ifStatement = (IfStatementSyntax)context.Node;

        if (ifStatement.Else is not null)
        {
            return;
        }

        ExpressionSyntax? paramExpression = null;
        var condition = ifStatement.Condition;

        if (condition is IsPatternExpressionSyntax { Pattern: ConstantPatternSyntax constantPattern } isPattern &&
            constantPattern.Expression.IsKind(SyntaxKind.NullLiteralExpression))
        {
            paramExpression = isPattern.Expression;
        }
        else if (condition is BinaryExpressionSyntax binary &&
                 binary.IsKind(SyntaxKind.EqualsExpression))
        {
            if (binary.Right.IsKind(SyntaxKind.NullLiteralExpression))
            {
                paramExpression = binary.Left;
            }
            else if (binary.Left.IsKind(SyntaxKind.NullLiteralExpression))
            {
                paramExpression = binary.Right;
            }
        }

        if (paramExpression is null)
        {
            return;
        }

        var body = ifStatement.Statement;
        if (body is BlockSyntax block)
        {
            if (block.Statements.Count != 1)
            {
                return;
            }

            body = block.Statements[0];
        }

        if (body is not ThrowStatementSyntax throwStatement || throwStatement.Expression is null)
        {
            return;
        }

        if (!IsArgumentNullException(throwStatement.Expression, context.SemanticModel))
        {
            return;
        }

        if (!TryGetNonNullableReferenceParameter(paramExpression, context.SemanticModel, out var paramName))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, ifStatement.GetLocation(), paramName));
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocationNode = (InvocationExpressionSyntax)context.Node;

        if (invocationNode.Expression is not MemberAccessExpressionSyntax memberAccess)
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
        if (!TryGetNonNullableReferenceParameter(firstArg, context.SemanticModel, out var paramName))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, invocationNode.GetLocation(), paramName));
    }

    private static bool IsArgumentNullException(ExpressionSyntax expression, SemanticModel semanticModel)
    {
        return semanticModel.GetTypeInfo(expression).Type?.ToDisplayString() == "System.ArgumentNullException";
    }

    private static bool TryGetNonNullableReferenceParameter(
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
