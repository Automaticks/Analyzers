using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.Complexity;

/// <summary>
///     Flags methods, operators, and property accessors whose maximum nesting depth exceeds <see cref="MaxDepth" />.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NestingDepthAnalyzer : DiagnosticAnalyzer
{
    private const string LimitKey = "automaticks.nesting_depth";
    private const int MaxDepth = 5;

    /// <summary>
    ///     The diagnostic rule reported when a member's nesting depth exceeds <see cref="MaxDepth" />.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static NestingDepthAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.NestingDepth,
            "Method nesting depth exceeds the maximum",
            "Method '{0}' has a nesting depth of {1}, which exceeds the maximum of {2}",
            "Maintainability",
            DiagnosticSeverity.Error,
            true,
            "Reduce the nesting depth of the method. Use early-return guard clauses to flatten `if`-chains, extract inner `foreach`/`while` loops into helper methods, or break the method into smaller focused methods. The maximum nesting depth is configurable via `.editorconfig` with key `automaticks.nesting_depth`.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(
            AnalyzeMember,
            SyntaxKind.MethodDeclaration,
            SyntaxKind.OperatorDeclaration,
            SyntaxKind.ConversionOperatorDeclaration,
            SyntaxKind.GetAccessorDeclaration,
            SyntaxKind.SetAccessorDeclaration,
            SyntaxKind.InitAccessorDeclaration);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeMember(SyntaxNodeAnalysisContext context)
    {
        var memberInfo = GetMemberInfo(context.Node);
        if (memberInfo is null)
        {
            return;
        }

        var walker = new NestingDepthWalker();
        walker.Visit(memberInfo.Value.BodyNode);

        var maxDepth = ConfigurableLimit.Read(context, LimitKey, MaxDepth);
        if (walker.DepthReached > maxDepth)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(Rule, memberInfo.Value.Location, memberInfo.Value.Name, walker.DepthReached, maxDepth));
        }
    }

    private MemberBodyInfo? GetAccessorInfo(SyntaxNode node)
    {
        if (node is not AccessorDeclarationSyntax accessorDeclaration)
        {
            return null;
        }

        var bodyNode = GetBodyNode(accessorDeclaration.Body, accessorDeclaration.ExpressionBody);
        if (bodyNode is null)
        {
            return null;
        }

        return new MemberBodyInfo(accessorDeclaration.Keyword.Text, accessorDeclaration.Keyword.GetLocation(), bodyNode);
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

    private MemberBodyInfo? GetConversionOperatorInfo(SyntaxNode node)
    {
        if (node is not ConversionOperatorDeclarationSyntax conversionOperatorDeclaration)
        {
            return null;
        }

        var bodyNode = GetBodyNode(conversionOperatorDeclaration.Body, conversionOperatorDeclaration.ExpressionBody);
        if (bodyNode is null)
        {
            return null;
        }

        var name = $"{conversionOperatorDeclaration.ImplicitOrExplicitKeyword.Text} operator {conversionOperatorDeclaration.Type}";
        return new MemberBodyInfo(name, conversionOperatorDeclaration.ImplicitOrExplicitKeyword.GetLocation(), bodyNode);
    }

    private MemberBodyInfo? GetMemberInfo(SyntaxNode node)
    {
        var methodInfo = GetMethodInfo(node);
        if (methodInfo is not null)
        {
            return methodInfo;
        }

        var operatorInfo = GetOperatorInfo(node);
        if (operatorInfo is not null)
        {
            return operatorInfo;
        }

        var conversionOperatorInfo = GetConversionOperatorInfo(node);
        if (conversionOperatorInfo is not null)
        {
            return conversionOperatorInfo;
        }

        return GetAccessorInfo(node);
    }

    private MemberBodyInfo? GetMethodInfo(SyntaxNode node)
    {
        if (node is not MethodDeclarationSyntax methodDeclaration)
        {
            return null;
        }

        var bodyNode = GetBodyNode(methodDeclaration.Body, methodDeclaration.ExpressionBody);
        if (bodyNode is null)
        {
            return null;
        }

        return new MemberBodyInfo(methodDeclaration.Identifier.Text, methodDeclaration.Identifier.GetLocation(), bodyNode);
    }

    private MemberBodyInfo? GetOperatorInfo(SyntaxNode node)
    {
        if (node is not OperatorDeclarationSyntax operatorDeclaration)
        {
            return null;
        }

        var bodyNode = GetBodyNode(operatorDeclaration.Body, operatorDeclaration.ExpressionBody);
        if (bodyNode is null)
        {
            return null;
        }

        return new MemberBodyInfo($"operator {operatorDeclaration.OperatorToken.Text}", operatorDeclaration.OperatorToken.GetLocation(), bodyNode);
    }

    /// <summary>
    ///     Groups the name, location, and body node of a member whose nesting depth is measured.
    /// </summary>
    private readonly struct MemberBodyInfo
    {
        /// <summary>
        ///     The member's body node to walk for nesting depth.
        /// </summary>
        public SyntaxNode BodyNode { get; }

        /// <summary>
        ///     The location used to anchor the diagnostic.
        /// </summary>
        public Location Location { get; }

        /// <summary>
        ///     The member's display name used in the diagnostic message.
        /// </summary>
        public string Name { get; }

        public MemberBodyInfo(string name, Location location, SyntaxNode bodyNode)
        {
            Name = name;
            Location = location;
            BodyNode = bodyNode;
        }
    }

    /// <summary>
    ///     Walks a member body and tracks the maximum nesting depth reached during traversal.
    /// </summary>
    private sealed class NestingDepthWalker : CSharpSyntaxWalker
    {
        private int _currentDepth;

        /// <summary>
        ///     Gets the maximum nesting depth observed during traversal.
        /// </summary>
        public int DepthReached { get; private set; }

        public override void VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
        {
            EnterBlock();
            Visit(node.Body);
            ExitBlock();
        }

        public override void VisitConditionalExpression(ConditionalExpressionSyntax node)
        {
            Visit(node.Condition);
            EnterBlock();
            Visit(node.WhenTrue);
            Visit(node.WhenFalse);
            ExitBlock();
        }

        public override void VisitDoStatement(DoStatementSyntax node)
        {
            EnterBlock();
            Visit(node.Statement);
            ExitBlock();
            Visit(node.Condition);
        }

        public override void VisitForEachStatement(ForEachStatementSyntax node)
        {
            Visit(node.Expression);
            EnterBlock();
            Visit(node.Statement);
            ExitBlock();
        }

        public override void VisitForEachVariableStatement(ForEachVariableStatementSyntax node)
        {
            Visit(node.Variable);
            Visit(node.Expression);
            EnterBlock();
            Visit(node.Statement);
            ExitBlock();
        }

        public override void VisitForStatement(ForStatementSyntax node)
        {
            VisitForStatementHeader(node);
            EnterBlock();
            Visit(node.Statement);
            ExitBlock();
        }

        public override void VisitIfStatement(IfStatementSyntax node)
        {
            Visit(node.Condition);
            EnterBlock();
            Visit(node.Statement);
            ExitBlock();
            VisitElseAfterIf(node.Else);
        }

        public override void VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
        {
            EnterBlock();

            if (node.Body is not null)
            {
                Visit(node.Body);
            }
            else if (node.ExpressionBody is not null)
            {
                Visit(node.ExpressionBody);
            }

            ExitBlock();
        }

        public override void VisitLockStatement(LockStatementSyntax node)
        {
            Visit(node.Expression);
            EnterBlock();
            Visit(node.Statement);
            ExitBlock();
        }

        public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
            EnterBlock();
            Visit(node.Body);
            ExitBlock();
        }

        public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
        {
            EnterBlock();
            Visit(node.Body);
            ExitBlock();
        }

        public override void VisitSwitchExpression(SwitchExpressionSyntax node)
        {
            Visit(node.GoverningExpression);
            EnterBlock();

            foreach (var arm in node.Arms)
            {
                Visit(arm);
            }

            ExitBlock();
        }

        public override void VisitSwitchStatement(SwitchStatementSyntax node)
        {
            Visit(node.Expression);
            EnterBlock();

            foreach (var section in node.Sections)
            {
                Visit(section);
            }

            ExitBlock();
        }

        public override void VisitTryStatement(TryStatementSyntax node)
        {
            EnterBlock();
            Visit(node.Block);
            ExitBlock();
            VisitCatchClauses(node);

            if (node.Finally is not null)
            {
                EnterBlock();
                Visit(node.Finally.Block);
                ExitBlock();
            }
        }

        public override void VisitUsingStatement(UsingStatementSyntax node)
        {
            if (node.Declaration is not null)
            {
                Visit(node.Declaration);
            }

            if (node.Expression is not null)
            {
                Visit(node.Expression);
            }

            EnterBlock();
            Visit(node.Statement);
            ExitBlock();
        }

        public override void VisitWhileStatement(WhileStatementSyntax node)
        {
            Visit(node.Condition);
            EnterBlock();
            Visit(node.Statement);
            ExitBlock();
        }

        private void EnterBlock()
        {
            _currentDepth++;

            if (_currentDepth > DepthReached)
            {
                DepthReached = _currentDepth;
            }
        }

        private void ExitBlock()
        {
            _currentDepth--;
        }

        private void VisitCatchClauses(TryStatementSyntax node)
        {
            foreach (var catchClause in node.Catches)
            {
                EnterBlock();
                Visit(catchClause.Block);
                ExitBlock();
            }
        }

        private void VisitElseAfterIf(ElseClauseSyntax? elseClause)
        {
            if (elseClause is null)
            {
                return;
            }

            if (elseClause.Statement is IfStatementSyntax elseIf)
            {
                VisitIfStatement(elseIf);
                return;
            }

            EnterBlock();
            Visit(elseClause.Statement);
            ExitBlock();
        }

        private void VisitForStatementHeader(ForStatementSyntax node)
        {
            if (node.Declaration is not null)
            {
                Visit(node.Declaration);
            }

            foreach (var init in node.Initializers)
            {
                Visit(init);
            }

            if (node.Condition is not null)
            {
                Visit(node.Condition);
            }

            foreach (var incr in node.Incrementors)
            {
                Visit(incr);
            }
        }
    }
}
