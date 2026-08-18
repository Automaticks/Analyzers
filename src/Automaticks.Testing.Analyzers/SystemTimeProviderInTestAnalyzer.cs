using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
        context.RegisterCompilationStartAction(RegisterPerCompilation);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context, INamedTypeSymbol? timeProviderType)
    {
        if (context.SemanticModel.GetSymbolInfo(context.Node).Symbol is not IPropertySymbol property)
        {
            return;
        }

        if (property.Name != "System")
        {
            return;
        }

        if (!SymbolEqualityComparer.Default.Equals(property.ContainingType, timeProviderType))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
    }

    private bool HasTestProjectFlag(AnalyzerOptions options)
    {
        options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue("build_property.IsTestProject", out var flag);
        return string.Equals(flag, "true", StringComparison.OrdinalIgnoreCase);
    }

    private void RegisterPerCompilation(CompilationStartAnalysisContext compilationContext)
    {
        if (!HasTestProjectFlag(compilationContext.Options))
        {
            return;
        }

        var timeProviderType = compilationContext.Compilation.GetTypeByMetadataName("System.TimeProvider");
        compilationContext.RegisterSyntaxNodeAction(
            context => AnalyzeMemberAccess(context, timeProviderType),
            SyntaxKind.SimpleMemberAccessExpression);
    }
}
