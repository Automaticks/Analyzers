using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.Testing.Testability;

/// <summary>
///     Flags #if / #elif directives that branch on the DEBUG symbol.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DebugConditionalCompilationAnalyzer : DiagnosticAnalyzer
{
    private const string DebugSymbolName = "DEBUG";
    private static readonly DiagnosticDescriptor Rule;

    static DebugConditionalCompilationAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.Testing.DebugConditionalCompilation,
            "Conditional compilation on DEBUG hides the shipped code path from tests",
            "This '{0}' directive branches on the DEBUG symbol, so the code path actually shipped in release is never the one this test run exercises.",
            "Testing",
            DiagnosticSeverity.Warning,
            true,
            "Conditional compilation on DEBUG produces two different builds: the one under test and the one shipped. Whichever branch is inactive while testing never runs in CI, so a regression in it ships silently. Remove the conditional compilation, or move the behaviour behind a runtime flag both configurations exercise identically.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [Rule];
        }
    }

    private void AnalyzeDirectiveTrivia(SyntaxTreeAnalysisContext context, SyntaxTrivia trivia)
    {
        if (trivia.IsKind(SyntaxKind.IfDirectiveTrivia) && trivia.GetStructure() is IfDirectiveTriviaSyntax ifDirective)
        {
            ReportIfDebugSymbol(context, ifDirective, ifDirective.Condition, "#if");
            return;
        }

        if (trivia.IsKind(SyntaxKind.ElifDirectiveTrivia) && trivia.GetStructure() is ElifDirectiveTriviaSyntax elifDirective)
        {
            ReportIfDebugSymbol(context, elifDirective, elifDirective.Condition, "#elif");
        }
    }

    private void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);
        foreach (var trivia in root.DescendantTrivia())
        {
            AnalyzeDirectiveTrivia(context, trivia);
        }
    }

    private bool HasDebugSymbolReference(ExpressionSyntax condition)
    {
        foreach (var node in condition.DescendantNodesAndSelf())
        {
            if (node is IdentifierNameSyntax identifier && identifier.Identifier.ValueText == DebugSymbolName)
            {
                return true;
            }
        }

        return false;
    }

    private void ReportIfDebugSymbol(
        SyntaxTreeAnalysisContext context,
        DirectiveTriviaSyntax directive,
        ExpressionSyntax condition,
        string directiveText)
    {
        if (!HasDebugSymbolReference(condition))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, directive.GetLocation(), directiveText));
    }
}
