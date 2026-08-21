using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.Diagnostics.CodeAnalysis.Analyzers.Tests.Stubs;

/// <summary>Reports ATXDC018 on a class identifier (not trivia), to test the code fix's non-trivia-location guard.</summary>
public sealed class SuppressionPragmaAtNonTriviaLocationAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule;

    static SuppressionPragmaAtNonTriviaLocationAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.DiagnosticsCodeAnalysis.SuppressionPragma,
            "Test-only ATXDC018 on a class identifier",
            "Test-only ATXDC018 on a class identifier",
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
