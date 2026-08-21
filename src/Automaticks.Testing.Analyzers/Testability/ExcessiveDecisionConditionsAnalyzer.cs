using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.Testing.Testability;

/// <summary>
///     Flags a boolean decision that combines more leaf conditions than the configured maximum.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ExcessiveDecisionConditionsAnalyzer : DiagnosticAnalyzer
{
    private const int DefaultMaximumConditions = 3;
    private const string MaximumConditionsKey = "automaticks.max_decision_conditions";
    private static readonly DiagnosticDescriptor Rule;

    static ExcessiveDecisionConditionsAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.Testing.ExcessiveDecisionConditions,
            "Boolean decision needs too many test cases",
            "This decision combines {0} leaf conditions, so full modified condition/decision coverage needs at least {1} test cases. Extract named predicate helpers to make each condition independently testable.",
            "Testing",
            DiagnosticSeverity.Warning,
            true,
            "Every extra leaf condition joined by && or || roughly doubles the number of test cases needed to prove each condition independently changes the outcome. Set the threshold with `automaticks.max_decision_conditions` in `.editorconfig`, or split the decision into named predicate helpers so each one can be tested in isolation.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(
            AnalyzeDecision,
            SyntaxKind.IfStatement,
            SyntaxKind.WhileStatement,
            SyntaxKind.DoStatement,
            SyntaxKind.ConditionalExpression,
            SyntaxKind.ReturnStatement);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeDecision(SyntaxNodeAnalysisContext context)
    {
        var condition = GetDecisionCondition(context);
        if (condition is null)
        {
            return;
        }

        var leafCount = CountLeaves(condition);
        var maximum = ReadMaximumConditions(context);
        if (leafCount <= maximum)
        {
            return;
        }

        var minimumTestCases = leafCount + 1;
        context.ReportDiagnostic(Diagnostic.Create(Rule, condition.GetLocation(), leafCount, minimumTestCases));
    }

    private int CountLeaves(ExpressionSyntax expression)
    {
        var unwrapped = Unwrap(expression);
        if (unwrapped is BinaryExpressionSyntax binary && IsLogicalConnective(binary))
        {
            return CountLeaves(binary.Left) + CountLeaves(binary.Right);
        }

        return 1;
    }

    private ExpressionSyntax? GetBooleanReturnExpression(SyntaxNodeAnalysisContext context, ReturnStatementSyntax returnStatement)
    {
        var expression = returnStatement.Expression;
        if (expression is null)
        {
            return null;
        }

        var type = context.SemanticModel.GetTypeInfo(expression, context.CancellationToken).Type;
        return type?.SpecialType == SpecialType.System_Boolean ? expression : null;
    }

    private ExpressionSyntax? GetDecisionCondition(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is IfStatementSyntax ifStatement)
        {
            return ifStatement.Condition;
        }

        if (context.Node is WhileStatementSyntax whileStatement)
        {
            return whileStatement.Condition;
        }

        if (context.Node is DoStatementSyntax doStatement)
        {
            return doStatement.Condition;
        }

        if (context.Node is ConditionalExpressionSyntax conditional)
        {
            return conditional.Condition;
        }

        var returnStatement = (context.Node as ReturnStatementSyntax)!;
        return GetBooleanReturnExpression(context, returnStatement);
    }

    private bool IsLogicalConnective(BinaryExpressionSyntax binary)
    {
        return binary.IsKind(SyntaxKind.LogicalAndExpression) || binary.IsKind(SyntaxKind.LogicalOrExpression);
    }

    private int ReadMaximumConditions(SyntaxNodeAnalysisContext context)
    {
        var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
        if (options.TryGetValue(MaximumConditionsKey, out var raw) && int.TryParse(raw, out var parsed) && parsed > 0)
        {
            return parsed;
        }

        return DefaultMaximumConditions;
    }

    private ExpressionSyntax Unwrap(ExpressionSyntax expression)
    {
        var current = expression;
        while (current is ParenthesizedExpressionSyntax parenthesized)
        {
            current = parenthesized.Expression;
        }

        return current;
    }
}
