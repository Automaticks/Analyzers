using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CommunityToolkit.Mvvm.Analyzers.Tests.Stubs;

/// <summary>Reports ATXMV001 on a lambda that is not wrapped in an ArgumentSyntax, to test the code fix's fallback node lookup.</summary>
public sealed class CommandLambdaAtLocalDeclarationAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule;

    static CommandLambdaAtLocalDeclarationAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.ModelViewViewModel.CommandLambda,
            "Test-only ATXMV001 on a lambda without an argument wrapper",
            "Test-only ATXMV001 on a lambda without an argument wrapper",
            "Testing",
            DiagnosticSeverity.Error,
            true);
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.ParenthesizedLambdaExpression, SyntaxKind.SimpleLambdaExpression);
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
        if (context.Node.Parent is ArgumentSyntax)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
    }
}
