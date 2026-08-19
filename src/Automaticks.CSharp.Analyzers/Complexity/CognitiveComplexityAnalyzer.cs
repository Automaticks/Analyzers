using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.Complexity;

/// <summary>
///     Flags methods whose cognitive complexity exceeds .
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CognitiveComplexityAnalyzer : DiagnosticAnalyzer
{
    private const string LimitKey = "automaticks.cognitive_complexity";
    private const int MaxComplexity = 15;

    /// <summary>
    ///     The diagnostic rule reported when a method's cognitive complexity exceeds
    ///     <see cref="MaxComplexity" />.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static CognitiveComplexityAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.CognitiveComplexity,
            "Method cognitive complexity exceeds the maximum",
            "Method '{0}' has a cognitive complexity of {1}, which exceeds the maximum of {2}",
            "Maintainability",
            DiagnosticSeverity.Error,
            true,
            "Reduce the method's cognitive complexity by: extracting nested conditions or loops into well-named private methods, replacing complex boolean expressions with named predicate helpers, or using early returns / guard clauses to flatten nesting. The threshold is configurable via `.editorconfig` with key `automaticks.cognitive_complexity`.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MethodDeclarationSyntax method)
        {
            return;
        }

        SyntaxNode? bodyNode;
        if (method.Body is not null)
        {
            bodyNode = method.Body;
        }
        else if (method.ExpressionBody is not null)
        {
            bodyNode = method.ExpressionBody;
        }
        else
        {
            return;
        }

        var walker = new CognitiveComplexityWalker();
        walker.Visit(bodyNode);

        var maxComplexity = ConfigurableLimit.Read(context, LimitKey, MaxComplexity);
        if (walker.Score > maxComplexity)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(Rule, method.Identifier.GetLocation(), method.Identifier.Text, walker.Score, maxComplexity));
        }
    }

    /// <summary>
    ///     Walks a method body and computes cognitive complexity using Sonar-style rules.
    /// </summary>
    private sealed class CognitiveComplexityWalker : CSharpSyntaxWalker
    {
        private int _nestingLevel;

        /// <summary>
        ///     Gets the total cognitive complexity score accumulated during traversal.
        /// </summary>
        public int Score { get; private set; }

        public override void VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
        {
            _nestingLevel++;
            Visit(node.Body);
            _nestingLevel--;
        }

        public override void VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            if ((node.IsKind(SyntaxKind.LogicalAndExpression) || node.IsKind(SyntaxKind.LogicalOrExpression))
                && (node.Parent is not BinaryExpressionSyntax parentBinary || !parentBinary.IsKind(node.Kind())))
            {
                Score++;
            }

            base.VisitBinaryExpression(node);
        }

        public override void VisitCatchClause(CatchClauseSyntax node)
        {
            Score++;
            _nestingLevel++;
            Visit(node.Block);
            _nestingLevel--;

            if (node.Filter is not null)
            {
                Visit(node.Filter);
            }
        }

        public override void VisitConditionalExpression(ConditionalExpressionSyntax node)
        {
            Score += 1 + _nestingLevel;
            Visit(node.Condition);
            _nestingLevel++;
            Visit(node.WhenTrue);
            Visit(node.WhenFalse);
            _nestingLevel--;
        }

        public override void VisitDoStatement(DoStatementSyntax node)
        {
            Score += 1 + _nestingLevel;
            _nestingLevel++;
            Visit(node.Statement);
            _nestingLevel--;
            Visit(node.Condition);
        }

        public override void VisitForEachStatement(ForEachStatementSyntax node)
        {
            Score += 1 + _nestingLevel;
            Visit(node.Expression);
            _nestingLevel++;
            Visit(node.Statement);
            _nestingLevel--;
        }

        public override void VisitForEachVariableStatement(ForEachVariableStatementSyntax node)
        {
            Score += 1 + _nestingLevel;
            Visit(node.Variable);
            Visit(node.Expression);
            _nestingLevel++;
            Visit(node.Statement);
            _nestingLevel--;
        }

        public override void VisitForStatement(ForStatementSyntax node)
        {
            Score += 1 + _nestingLevel;
            VisitForStatementHeader(node);
            _nestingLevel++;
            Visit(node.Statement);
            _nestingLevel--;
        }

        public override void VisitIfStatement(IfStatementSyntax node)
        {
            var isElseIf = node.Parent is ElseClauseSyntax;
            Score += isElseIf ? 1 : 1 + _nestingLevel;

            Visit(node.Condition);
            _nestingLevel++;
            Visit(node.Statement);
            _nestingLevel--;

            if (node.Else is not null)
            {
                HandleElseClause(node.Else);
            }
        }

        public override void VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
        {
            _nestingLevel++;

            if (node.Body is not null)
            {
                Visit(node.Body);
            }
            else if (node.ExpressionBody is not null)
            {
                Visit(node.ExpressionBody);
            }

            _nestingLevel--;
        }

        public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
            _nestingLevel++;
            Visit(node.Body);
            _nestingLevel--;
        }

        public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
        {
            _nestingLevel++;
            Visit(node.Body);
            _nestingLevel--;
        }

        public override void VisitSwitchExpression(SwitchExpressionSyntax node)
        {
            Score += 1 + _nestingLevel;
            Visit(node.GoverningExpression);
            _nestingLevel++;

            foreach (var arm in node.Arms)
            {
                Visit(arm);
            }

            _nestingLevel--;
        }

        public override void VisitSwitchStatement(SwitchStatementSyntax node)
        {
            Score += 1 + _nestingLevel;
            Visit(node.Expression);
            _nestingLevel++;

            foreach (var section in node.Sections)
            {
                Visit(section);
            }

            _nestingLevel--;
        }

        public override void VisitWhileStatement(WhileStatementSyntax node)
        {
            Score += 1 + _nestingLevel;
            Visit(node.Condition);
            _nestingLevel++;
            Visit(node.Statement);
            _nestingLevel--;
        }

        private void HandleElseClause(ElseClauseSyntax elseClause)
        {
            if (elseClause.Statement is IfStatementSyntax elseIf)
            {
                VisitIfStatement(elseIf);
                return;
            }

            Score++;
            _nestingLevel++;
            Visit(elseClause.Statement);
            _nestingLevel--;
        }

        private void VisitForStatementHeader(ForStatementSyntax node)
        {
            if (node.Declaration is not null)
            {
                Visit(node.Declaration);
            }

            foreach (var initializer in node.Initializers)
            {
                Visit(initializer);
            }

            if (node.Condition is not null)
            {
                Visit(node.Condition);
            }

            foreach (var incrementor in node.Incrementors)
            {
                Visit(incrementor);
            }
        }
    }
}
