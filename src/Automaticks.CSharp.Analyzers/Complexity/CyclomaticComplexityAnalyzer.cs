using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.Complexity;

/// <summary>
///     Flags methods, operators, and property accessors whose cyclomatic complexity exceeds the maximum of 10.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CyclomaticComplexityAnalyzer : DiagnosticAnalyzer
{
    private const string LimitKey = "automaticks.cyclomatic_complexity";
    private const int MaxComplexity = 15;

    /// <summary>
    ///     The diagnostic rule reported when a member's cyclomatic complexity exceeds
    ///     the configured maximum.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static CyclomaticComplexityAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.CyclomaticComplexity,
            "Method cyclomatic complexity exceeds the maximum",
            "Method '{0}' has a cyclomatic complexity of {1}, which exceeds the maximum of {2}",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "Reduce the number of independent code paths through the method. Extract branches into private helper methods, simplify boolean conditions, use early returns, or replace conditional trees with polymorphism. The threshold is configurable via `.editorconfig` with key `automaticks.cyclomatic_complexity`.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeOperator, SyntaxKind.OperatorDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeConversionOperator, SyntaxKind.ConversionOperatorDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeAccessor, SyntaxKind.GetAccessorDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeAccessor, SyntaxKind.SetAccessorDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeAccessor, SyntaxKind.InitAccessorDeclaration);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [Rule];
        }
    }

    private void AnalyzeAccessor(SyntaxNodeAnalysisContext context)
    {
        var accessor = (context.Node as AccessorDeclarationSyntax)!;
        var bodyNode = GetBodyNode(accessor.Body, accessor.ExpressionBody);
        if (bodyNode is null)
        {
            return;
        }

        var keyword = accessor.Keyword.Text;
        var memberName = accessor.Parent!.Parent is PropertyDeclarationSyntax property
            ? keyword + " accessor of '" + property.Identifier.Text + "'"
            : keyword + " accessor of indexer";

        AnalyzeMember(context, bodyNode, memberName, accessor.Keyword.GetLocation());
    }

    private void AnalyzeConversionOperator(SyntaxNodeAnalysisContext context)
    {
        var op = (context.Node as ConversionOperatorDeclarationSyntax)!;
        var bodyNode = GetBodyNode(op.Body, op.ExpressionBody);
        if (bodyNode is null)
        {
            return;
        }

        var memberName = "operator " + op.Type.ToString();
        AnalyzeMember(context, bodyNode, memberName, op.ImplicitOrExplicitKeyword.GetLocation());
    }

    private void AnalyzeMember(
        SyntaxNodeAnalysisContext context,
        SyntaxNode bodyNode,
        string memberName,
        Location location)
    {
        var walker = new CyclomaticComplexityWalker();
        walker.Visit(bodyNode);

        var complexity = 1 + walker.Count;
        var maxComplexity = ConfigurableLimit.Read(context, LimitKey, MaxComplexity);
        if (complexity > maxComplexity)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, location, memberName, complexity, maxComplexity));
        }
    }

    private void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        var method = (context.Node as MethodDeclarationSyntax)!;
        var bodyNode = GetBodyNode(method.Body, method.ExpressionBody);
        if (bodyNode is null)
        {
            return;
        }

        AnalyzeMember(context, bodyNode, method.Identifier.Text, method.Identifier.GetLocation());
    }

    private void AnalyzeOperator(SyntaxNodeAnalysisContext context)
    {
        var op = (context.Node as OperatorDeclarationSyntax)!;
        var bodyNode = GetBodyNode(op.Body, op.ExpressionBody);
        if (bodyNode is null)
        {
            return;
        }

        var memberName = "operator " + op.OperatorToken.Text;
        AnalyzeMember(context, bodyNode, memberName, op.OperatorToken.GetLocation());
    }

    private SyntaxNode? GetBodyNode(BlockSyntax? block, ArrowExpressionClauseSyntax? expressionBody)
    {
        if (block is not null)
        {
            return block;
        }

        if (expressionBody is not null)
        {
            return expressionBody;
        }

        return null;
    }

    /// <summary>
    ///     Walks a member body and counts cyclomatic complexity decision points.
    /// </summary>
    private sealed class CyclomaticComplexityWalker : CSharpSyntaxWalker
    {
        /// <summary>
        ///     Gets the total number of decision points found.
        /// </summary>
        public int Count { get; private set; }

        public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            if (node.IsKind(SyntaxKind.CoalesceAssignmentExpression))
            {
                Count++;
            }

            base.VisitAssignmentExpression(node);
        }

        public override void VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            if (node.IsKind(SyntaxKind.LogicalAndExpression)
                || node.IsKind(SyntaxKind.LogicalOrExpression)
                || node.IsKind(SyntaxKind.CoalesceExpression))
            {
                Count++;
            }

            base.VisitBinaryExpression(node);
        }

        public override void VisitCaseSwitchLabel(CaseSwitchLabelSyntax node)
        {
            Count++;
            base.VisitCaseSwitchLabel(node);
        }

        public override void VisitCatchClause(CatchClauseSyntax node)
        {
            Count++;
            base.VisitCatchClause(node);
        }

        public override void VisitConditionalExpression(ConditionalExpressionSyntax node)
        {
            Count++;
            base.VisitConditionalExpression(node);
        }

        public override void VisitDoStatement(DoStatementSyntax node)
        {
            Count++;
            base.VisitDoStatement(node);
        }

        public override void VisitForEachStatement(ForEachStatementSyntax node)
        {
            Count++;
            base.VisitForEachStatement(node);
        }

        public override void VisitForEachVariableStatement(ForEachVariableStatementSyntax node)
        {
            Count++;
            base.VisitForEachVariableStatement(node);
        }

        public override void VisitForStatement(ForStatementSyntax node)
        {
            Count++;
            base.VisitForStatement(node);
        }

        public override void VisitIfStatement(IfStatementSyntax node)
        {
            Count++;
            base.VisitIfStatement(node);
        }

        public override void VisitSwitchExpressionArm(SwitchExpressionArmSyntax node)
        {
            Count++;
            base.VisitSwitchExpressionArm(node);
        }

        public override void VisitWhenClause(WhenClauseSyntax node)
        {
            Count++;
            base.VisitWhenClause(node);
        }

        public override void VisitWhileStatement(WhileStatementSyntax node)
        {
            Count++;
            base.VisitWhileStatement(node);
        }
    }
}
