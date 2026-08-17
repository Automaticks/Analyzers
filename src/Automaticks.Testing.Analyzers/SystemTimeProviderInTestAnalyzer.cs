using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace Automaticks.Testing;

/// <summary>
///     Flags <c>TimeProvider.System</c> in test code. <c>ATXTST004</c> exempts any <c>Task.Delay</c>
///     that takes a <c>TimeProvider</c>, on the assumption the provider is controllable, so passing
///     the real system clock satisfies that rule while keeping the wall-clock flakiness it exists to
///     remove.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SystemTimeProviderInTestAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule;

    static SystemTimeProviderInTestAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.Testing.SystemTimeProviderInTest,
            "TimeProvider.System is not allowed in tests",
            "TimeProvider.System is the real clock. Use a controllable fake TimeProvider so the test does not depend on wall-clock time.",
            "Testing",
            DiagnosticSeverity.Warning,
            true,
            "Replace `TimeProvider.System` with a fake provider the test drives explicitly. Passing the real clock satisfies the TimeProvider exemption in ATXTST004 while leaving the test dependent on wall-clock time, which is what makes it flaky under CI load.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        if (!HasTestProjectFlag(context.Options))
        {
            return;
        }

        if (!HasSystemTimeProviderTarget(memberAccess, context.SemanticModel))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, memberAccess.GetLocation()));
    }

    private bool HasSystemTimeProviderTarget(MemberAccessExpressionSyntax memberAccess, SemanticModel semanticModel)
    {
        if (semanticModel.GetSymbolInfo(memberAccess).Symbol is not IPropertySymbol property)
        {
            return false;
        }

        if (property.Name != "System")
        {
            return false;
        }

        var timeProviderType = semanticModel.Compilation.GetTypeByMetadataName("System.TimeProvider");
        return timeProviderType is not null
            && SymbolEqualityComparer.Default.Equals(property.ContainingType, timeProviderType);
    }

    private bool HasTestProjectFlag(AnalyzerOptions options)
    {
        if (!options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue("build_property.IsTestProject", out var flag))
        {
            return false;
        }

        return string.Equals(flag, "true", StringComparison.OrdinalIgnoreCase);
    }
}
