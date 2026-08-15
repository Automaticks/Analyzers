using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.Diagnostics.CodeAnalysis.Analyzers.Tests.Stubs;

/// <summary>Reports <c>ATXDC056</c> on a method identifier so a code fix can be tested against a diagnostic with no enclosing attribute.</summary>
public sealed class SuppressMessageAtMethodAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule;

    static SuppressMessageAtMethodAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.DiagnosticsCodeAnalysis.SuppressMessage,
            "Test-only ATXDC056 at a method identifier",
            "Test-only ATXDC056 at a method identifier",
            "Testing",
            DiagnosticSeverity.Warning,
            true);
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.MethodDeclaration);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MethodDeclarationSyntax method)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, method.Identifier.GetLocation()));
    }
}
