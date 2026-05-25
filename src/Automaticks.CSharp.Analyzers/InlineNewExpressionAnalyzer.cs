using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace Automaticks.CSharp;

/// <summary>
///     Flags any <c>new</c> expression that is used inline — not directly assigned to a local variable.
///     The following positions are exempt from this rule:
///     <list type="bullet">
///         <item>The direct right-hand side of a local variable declaration: <c>var x = new Foo()</c>, including <c>for</c> and <c>using</c> declarations.</item>
///         <item>The direct expression of a <c>return</c> or <c>yield return</c> statement.</item>
///         <item>The direct body of an expression-bodied member (<c>=&gt;</c>).</item>
///         <item>Inside an attribute argument list.</item>
///         <item>The result expression of a switch expression arm: <c>x switch { T t =&gt; new Foo() }</c>.</item>
///         <item>The direct operand of a <c>throw</c> statement or <c>throw</c> expression: <c>throw new FooException()</c>.</item>
///     </list>
///     All other usages — constructor arguments, method arguments, collection initializer elements,
///     object initializer member values, field and property initializers, and conditional expressions —
///     must extract the instance to a named local variable first.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InlineNewExpressionAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when an inline <c>new</c> expression is detected.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.CSharp.InlineNewExpression,
        "Inline 'new' expression is forbidden",
        "Inline 'new' expression is forbidden; assign the instance to a local variable first",
        "CSharp",
        DiagnosticSeverity.Error,
        true,
        "Extract the `new` expression to a named local variable before passing it. Example: change `Method(new Foo())` to `var foo = new Foo(); Method(foo);`. Allowed positions where inline `new` is not flagged: `return new Foo()`, `var x = new Foo()`, expression-bodied members (`=> new Foo()`), switch expression arms (`T t => new Foo()`), attribute arguments, `yield return new Foo()`, `for`-loop initializers, `using` declarations, and `throw new FooException()`.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

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

    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        var node = (ExpressionSyntax)context.Node;

        if (IsExemptContext(node))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, GetNewKeyword(node).GetLocation()));
    }

    private static bool IsExemptContext(ExpressionSyntax node)
    {
        if (IsInsideAttributeArgument(node))
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
                return false;
        }
    }

    private static bool IsInsideAttributeArgument(SyntaxNode node)
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

    private static ExpressionSyntax GetEffectiveExpression(ExpressionSyntax node)
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

            return (ExpressionSyntax)current;
        }
    }

    private static SyntaxToken GetNewKeyword(ExpressionSyntax node)
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
}
