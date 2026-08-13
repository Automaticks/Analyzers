using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace Automaticks.CSharp.Formatting;

/// <summary>
///     Flags any <c>new</c> expression that is used inline — not directly assigned to a local variable
///     or used as the right-hand side of a top-level simple-assignment statement.
///     The following positions are exempt from this rule:
///     <list type="bullet">
///         <item>The direct right-hand side of a local variable declaration: <c>var x = new Foo()</c>, including <c>for</c> and <c>using</c> declarations.</item>
///         <item>The direct right-hand side of a top-level simple-assignment statement: <c>_field = new Foo();</c>, <c>this.Prop = new Foo();</c>, <c>dict[key] = new Foo();</c>. Only the simple <c>=</c> operator is exempt; compound assignments (<c>??=</c>, <c>+=</c>, etc.) remain flagged.</item>
///         <item>The direct expression of a <c>return</c> or <c>yield return</c> statement.</item>
///         <item>The direct body of an expression-bodied member (<c>=&gt;</c>).</item>
///         <item>Inside an attribute argument list.</item>
///         <item>The result expression of a switch expression arm: <c>x switch { T t =&gt; new Foo() }</c>.</item>
///         <item>The direct operand of a <c>throw</c> statement or <c>throw</c> expression: <c>throw new FooException()</c>.</item>
///     </list>
///     All other usages — constructor arguments, method arguments, collection initializer elements,
///     object initializer member values, field and property initializers, conditional expressions,
///     compound assignments, tuple/deconstruction assignments, and chained assignments —
///     must extract the instance to a named local variable first.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InlineNewExpressionAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when an inline <c>new</c> expression is detected.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static InlineNewExpressionAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.InlineNewExpression,
            "Inline 'new' expression is forbidden",
            "Inline 'new' expression is forbidden; assign the instance to a local variable first",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "Extract the `new` expression to a named local variable before passing it. Example: change `Method(new Foo())` to `var foo = new Foo(); Method(foo);`. Allowed positions where inline `new` is not flagged: `return new Foo()`, `var x = new Foo()`, top-level simple-assignment statements (`_field = new Foo();`, `Prop = new Foo();`, `dict[key] = new Foo();`), expression-bodied members (`=> new Foo()`), switch expression arms (`T t => new Foo()`), attribute arguments, `yield return new Foo()`, `for`-loop initializers, `using` declarations, and `throw new FooException()`. Compound assignments (`??=`, `+=`, etc.) and chained or deconstruction assignments are NOT exempt.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(
            Analyze,
            SyntaxKind.ObjectCreationExpression,
            SyntaxKind.ImplicitObjectCreationExpression,
            SyntaxKind.ArrayCreationExpression,
            SyntaxKind.ImplicitArrayCreationExpression,
            SyntaxKind.AnonymousObjectCreationExpression);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ExpressionSyntax node)
        {
            return;
        }

        if (HasExemptContext(node))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, GetNewKeyword(node).GetLocation()));
    }

    private ExpressionSyntax GetEffectiveExpression(ExpressionSyntax node)
    {
        SyntaxNode current = node;

        while (true)
        {
            var parent = current.Parent;

            if (parent is ParenthesizedExpressionSyntax)
            {
                current = parent;
                continue;
            }

            if (parent is PostfixUnaryExpressionSyntax postfix && postfix.IsKind(SyntaxKind.SuppressNullableWarningExpression))
            {
                current = parent;
                continue;
            }

            if (current is ExpressionSyntax effectiveExpression)
            {
                return effectiveExpression;
            }

            throw new InvalidOperationException($"Expected an expression but found '{current.GetType().Name}'.");
        }
    }

    private SyntaxToken GetNewKeyword(ExpressionSyntax node)
    {
        return node switch
        {
            ObjectCreationExpressionSyntax obj => obj.NewKeyword,
            ImplicitObjectCreationExpressionSyntax impl => impl.NewKeyword,
            ArrayCreationExpressionSyntax arr => arr.NewKeyword,
            ImplicitArrayCreationExpressionSyntax implArr => implArr.NewKeyword,
            AnonymousObjectCreationExpressionSyntax anon => anon.NewKeyword,
            _ => throw new InvalidOperationException($"Unexpected creation expression type: {node.GetType().Name}")
        };
    }

    private bool HasAttributeArgumentAncestor(SyntaxNode node)
    {
        var current = node.Parent;

        while (current is not null)
        {
            if (current is AttributeArgumentSyntax)
            {
                return true;
            }

            current = current.Parent;
        }

        return false;
    }

    private bool HasExemptContext(ExpressionSyntax node)
    {
        if (HasAttributeArgumentAncestor(node))
        {
            return true;
        }

        var effective = GetEffectiveExpression(node);
        var parent = effective.Parent;

        switch (parent)
        {
            case ReturnStatementSyntax:
            case YieldStatementSyntax:
            case ArrowExpressionClauseSyntax:
            case SwitchExpressionArmSyntax:
            case ThrowStatementSyntax:
            case ThrowExpressionSyntax:
                return true;

            case EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax varDecl } }:
                return varDecl.Parent is LocalDeclarationStatementSyntax
                    || varDecl.Parent is ForStatementSyntax
                    || varDecl.Parent is UsingStatementSyntax;

            default:
                return HasTopLevelSimpleAssignmentParent(effective);
        }
    }

    private bool HasTopLevelSimpleAssignmentParent(ExpressionSyntax effective)
    {
        return effective.Parent is AssignmentExpressionSyntax assignment
            && assignment.IsKind(SyntaxKind.SimpleAssignmentExpression)
            && assignment.Right == effective
            && assignment.Parent is ExpressionStatementSyntax;
    }
}
