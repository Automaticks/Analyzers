using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.Testing.Testability;

/// <summary>
///     Flags a Debug.Assert whose condition mutates state.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AssertSideEffectAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule;

    static AssertSideEffectAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.Testing.AssertSideEffect,
            "Debug.Assert condition must not perform side effects",
            "The condition of this Debug.Assert performs a side effect ({0}). Debug.Assert is removed from release builds, so the side effect disappears and release behaviour differs from the behaviour you tested.",
            "Testing",
            DiagnosticSeverity.Error,
            true,
            "`Debug.Assert` is annotated with `[Conditional(\"DEBUG\")]`, so both the call and its whole argument list are erased by the compiler in release builds. Any assignment, increment, or `out`/`ref` argument inside the condition therefore never executes in release. Move the side effect to a preceding statement and assert on the resulting value.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (context.Node as InvocationExpressionSyntax)!;
        var condition = GetAssertedCondition(invocation);
        if (condition is null)
        {
            return;
        }

        if (!HasDebugAssertMethod(context, invocation))
        {
            return;
        }

        var sideEffect = FindSideEffect(condition);
        if (sideEffect is null)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, sideEffect.GetLocation(), DescribeSideEffect(sideEffect)));
    }

    private string? DescribeSideEffect(SyntaxNode node)
    {
        switch (node)
        {
            case AssignmentExpressionSyntax:
                return "an assignment";
            case PrefixUnaryExpressionSyntax prefix when HasMutatingOperator(prefix.OperatorToken):
                return "an increment or decrement";
            case PostfixUnaryExpressionSyntax postfix when HasMutatingOperator(postfix.OperatorToken):
                return "an increment or decrement";
            case ArgumentSyntax argument when HasOutOrRefKeyword(argument):
                return "an out or ref argument";
            default:
                return null;
        }
    }

    private SyntaxNode? FindSideEffect(ExpressionSyntax condition)
    {
        foreach (var node in condition.DescendantNodesAndSelf())
        {
            if (DescribeSideEffect(node) is not null)
            {
                return node;
            }
        }

        return null;
    }

    private ExpressionSyntax? GetAssertedCondition(InvocationExpressionSyntax invocation)
    {
        var arguments = invocation.ArgumentList.Arguments;
        return arguments.Count == 0 ? null : arguments[0].Expression;
    }

    private bool HasDebugAssertMethod(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation)
    {
        if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol method)
        {
            return false;
        }

        if (method.Name != "Assert")
        {
            return false;
        }

        var debugType = context.SemanticModel.Compilation.GetTypeByMetadataName("System.Diagnostics.Debug");
        return SymbolEqualityComparer.Default.Equals(method.ContainingType, debugType);
    }

    private bool HasMutatingOperator(SyntaxToken operatorToken)
    {
        return operatorToken.IsKind(SyntaxKind.PlusPlusToken) || operatorToken.IsKind(SyntaxKind.MinusMinusToken);
    }

    private bool HasOutOrRefKeyword(ArgumentSyntax argument)
    {
        return argument.RefKindKeyword.IsKind(SyntaxKind.OutKeyword)
               || argument.RefKindKeyword.IsKind(SyntaxKind.RefKeyword);
    }
}
