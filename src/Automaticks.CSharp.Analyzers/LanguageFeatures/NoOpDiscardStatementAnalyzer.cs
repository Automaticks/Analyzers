using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.LanguageFeatures;

/// <summary>
///     Flags a discard assignment whose right-hand side only reads <c>this</c>, a parameter, or a
///     local. The statement computes nothing and exists to silence a diagnostic: <c>_ = this;</c>
///     stops "make this member static" firing, and <c>_ = parameter;</c> hides an unused parameter
///     or an empty catch block.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NoOpDiscardStatementAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule;

    static NoOpDiscardStatementAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.NoOpDiscardStatement,
            "No-op discard statement is forbidden",
            "This discard statement does nothing and only suppresses a diagnostic. Remove it and address the underlying warning.",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "Delete the statement and fix what it was hiding. `_ = this;` masks a member that should move to a static class; `_ = parameter;` masks a parameter the body ignores, which for a CancellationToken means cancellation is silently dropped; `_ = exception;` masks an empty catch block that swallows the error.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeExpressionStatement, SyntaxKind.ExpressionStatement);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeExpressionStatement(SyntaxNodeAnalysisContext context)
    {
        var statement = (context.Node as ExpressionStatementSyntax)!;
        if (statement.Expression is not AssignmentExpressionSyntax assignment)
        {
            return;
        }

        if (!assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
        {
            return;
        }

        if (!HasDiscardTarget(assignment.Left, context.SemanticModel))
        {
            return;
        }

        if (!HasSideEffectFreeSource(assignment.Right, context.SemanticModel))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, statement.GetLocation()));
    }

    private bool HasDiscardTarget(ExpressionSyntax target, SemanticModel semanticModel)
    {
        return semanticModel.GetSymbolInfo(target).Symbol is IDiscardSymbol;
    }

    private bool HasSideEffectFreeSource(ExpressionSyntax source, SemanticModel semanticModel)
    {
        if (source is ThisExpressionSyntax)
        {
            return true;
        }

        if (source is not IdentifierNameSyntax)
        {
            return false;
        }

        return semanticModel.GetSymbolInfo(source).Symbol is IParameterSymbol or ILocalSymbol;
    }
}
