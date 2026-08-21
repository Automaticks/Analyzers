using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CommunityToolkit.Mvvm.Analyzers.Tests.Stubs;

/// <summary>Reports ATXMV001 on a class identifier, to test the code fix's "no lambda found at all" guard.</summary>
public sealed class CommandLambdaAtClassDeclarationAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule;

    static CommandLambdaAtClassDeclarationAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.ModelViewViewModel.CommandLambda,
            "Test-only ATXMV001 on a class identifier",
            "Test-only ATXMV001 on a class identifier",
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
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.ClassDeclaration);
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
        var classDeclaration = (context.Node as ClassDeclarationSyntax)!;
        context.ReportDiagnostic(Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation()));
    }
}
