using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.Formatting;

/// <summary>
///     Flags any new expression that is used inline — not directly assigned to a local variable or used as the right-hand side of a top-level simple-assig...
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
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [Rule];
        }
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        var node = (context.Node as ExpressionSyntax)!;
        if (HasExemptContext(node))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, GetNewKeyword(node).GetLocation()));
    }

    private ExpressionSyntax GetEffectiveExpression(ExpressionSyntax node)
    {
        var current = node;

        while (true)
        {
            var parent = current.Parent;

            if (parent is ParenthesizedExpressionSyntax parenthesized)
            {
                current = parenthesized;
                continue;
            }

            if (parent is PostfixUnaryExpressionSyntax postfix && postfix.IsKind(SyntaxKind.SuppressNullableWarningExpression))
            {
                current = postfix;
                continue;
            }

            return current;
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
            _ => (node as AnonymousObjectCreationExpressionSyntax)!.NewKeyword
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

            case EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax declarator }:
            {
                var varDecl = (declarator.Parent as VariableDeclarationSyntax)!;
                return varDecl.Parent is LocalDeclarationStatementSyntax
                    || varDecl.Parent is ForStatementSyntax
                    || varDecl.Parent is UsingStatementSyntax;
            }

            default:
                return HasTopLevelSimpleAssignmentParent(effective);
        }
    }

    private bool HasTopLevelSimpleAssignmentParent(ExpressionSyntax effective)
    {
        var isSimpleAssignmentStatement = effective.Parent is AssignmentExpressionSyntax assignment
            && assignment.IsKind(SyntaxKind.SimpleAssignmentExpression)
            && assignment.Right == effective
            && assignment.Parent is ExpressionStatementSyntax;
        return isSimpleAssignmentStatement;
    }
}
