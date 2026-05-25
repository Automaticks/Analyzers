using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp;

/// <summary>
///     Flags methods, operators, and property accessors whose maximum nesting depth exceeds
///     <see cref="MaxDepth" />. Nesting depth is incremented by each <c>if</c>/<c>else</c>,
///     <c>switch</c>, loop (<c>for</c>/<c>foreach</c>/<c>while</c>/<c>do</c>),
///     <c>try</c>/<c>catch</c>/<c>finally</c>, <c>using</c>, <c>lock</c>, local function,
///     lambda, and conditional expression (<c>?:</c>).
///     Local functions and lambdas contribute depth to their enclosing method rather than
///     being evaluated independently.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NestingDepthAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a member's nesting depth exceeds
    ///     <see cref="MaxDepth" />.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.CSharp.NestingDepth,
        "Method nesting depth exceeds the maximum",
        "Method '{0}' has a nesting depth of {1}, which exceeds the maximum of {2}",
        "Maintainability",
        DiagnosticSeverity.Error,
        true,
        "Reduce the nesting depth of the method. Use early-return guard clauses to flatten `if`-chains, extract inner `foreach`/`while` loops into helper methods, or break the method into smaller focused methods. The maximum nesting depth is configurable via `.editorconfig` with key `automaticks.nesting_depth`.");

    private const int MaxDepth = 5;

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

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

    private static void AnalyzeMember(SyntaxNodeAnalysisContext context)
    {
        if (!TryGetMemberInfo(context.Node, out var name, out var location, out var bodyNode))
        {
            return;
        }

        var walker = new NestingDepthWalker();
        walker.Visit(bodyNode);

        if (walker.DepthReached > MaxDepth)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, location, name, walker.DepthReached, MaxDepth));
        }
    }

    private static SyntaxNode? GetBodyNode(BlockSyntax? block, ArrowExpressionClauseSyntax? expressionBody)
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

    private static bool TryGetAccessorInfo(
        SyntaxNode node,
        out string name,
        out Location location,
        out SyntaxNode? bodyNode)
    {
        if (node is not AccessorDeclarationSyntax a || (a.Body is null && a.ExpressionBody is null))
        {
            name = null!;
            location = null!;
            bodyNode = null;
            return false;
        }

        name = a.Keyword.Text;
        location = a.Keyword.GetLocation();
        bodyNode = GetBodyNode(a.Body, a.ExpressionBody);
        return true;
    }

    private static bool TryGetConversionOperatorInfo(
        SyntaxNode node,
        out string name,
        out Location location,
        out SyntaxNode? bodyNode)
    {
        if (node is not ConversionOperatorDeclarationSyntax c || (c.Body is null && c.ExpressionBody is null))
        {
            name = null!;
            location = null!;
            bodyNode = null;
            return false;
        }

        name = $"{c.ImplicitOrExplicitKeyword.Text} operator {c.Type}";
        location = c.ImplicitOrExplicitKeyword.GetLocation();
        bodyNode = GetBodyNode(c.Body, c.ExpressionBody);
        return true;
    }

    private static bool TryGetMemberInfo(
        SyntaxNode node,
        out string name,
        out Location location,
        out SyntaxNode? bodyNode)
    {
        if (TryGetMethodInfo(node, out name, out location, out bodyNode))
        {
            return true;
        }

        if (TryGetOperatorInfo(node, out name, out location, out bodyNode))
        {
            return true;
        }

        if (TryGetConversionOperatorInfo(node, out name, out location, out bodyNode))
        {
            return true;
        }

        if (TryGetAccessorInfo(node, out name, out location, out bodyNode))
        {
            return true;
        }

        name = null!;
        location = null!;
        bodyNode = null;
        return false;
    }

    private static bool TryGetMethodInfo(
        SyntaxNode node,
        out string name,
        out Location location,
        out SyntaxNode? bodyNode)
    {
        if (node is not MethodDeclarationSyntax m || (m.Body is null && m.ExpressionBody is null))
        {
            name = null!;
            location = null!;
            bodyNode = null;
            return false;
        }

        name = m.Identifier.Text;
        location = m.Identifier.GetLocation();
        bodyNode = GetBodyNode(m.Body, m.ExpressionBody);
        return true;
    }

    private static bool TryGetOperatorInfo(
        SyntaxNode node,
        out string name,
        out Location location,
        out SyntaxNode? bodyNode)
    {
        if (node is not OperatorDeclarationSyntax o || (o.Body is null && o.ExpressionBody is null))
        {
            name = null!;
            location = null!;
            bodyNode = null;
            return false;
        }

        name = $"operator {o.OperatorToken.Text}";
        location = o.OperatorToken.GetLocation();
        bodyNode = GetBodyNode(o.Body, o.ExpressionBody);
        return true;
    }

    /// <summary>
    ///     Walks a member body and tracks maximum nesting depth using a depth counter.
    ///     Each nesting construct increments the counter on entry and decrements it on exit.
    ///     Local functions and lambdas are treated as nested constructs rather than independent members.
    ///     Traversal is O(n) with respect to syntax tree size.
    /// </summary>
    private sealed class NestingDepthWalker : CSharpSyntaxWalker
    {
        private int _currentDepth;

        /// <summary>Gets the maximum nesting depth observed during traversal.</summary>
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
