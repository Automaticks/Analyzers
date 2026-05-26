using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace Automaticks.Extensions.Options;

/// <summary>
///     Flags any call to <c>BindConfiguration</c> where the receiver type is
///     <c>Microsoft.Extensions.Options.OptionsBuilder&lt;T&gt;</c>.
///     Use <c>Configure&lt;T&gt;(configuration.GetRequiredSection(...))</c> instead so that a missing
///     configuration section is detected at startup rather than silently using defaults.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BindConfigurationAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule;

    static BindConfigurationAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.Options.BindConfiguration,
            "BindConfiguration is forbidden",
            "Use 'Configure<T>(configuration.GetRequiredSection(...))' instead of 'AddOptions<T>().BindConfiguration(...)'. GetRequiredSection fails fast when the section is missing.",
            "Extensions.Options",
            DiagnosticSeverity.Error,
            true,
            "Replace `services.AddOptions<T>().BindConfiguration(\"Section\")` with `services.Configure<T>(configuration.GetRequiredSection(\"Section\"))`. `GetRequiredSection` throws `InvalidOperationException` at application startup when the configuration section is absent, preventing misconfigured deployments. `BindConfiguration` silently falls back to defaults.");
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
        if (context.Node is not InvocationExpressionSyntax invocation)
        {
            return;
        }

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        if (memberAccess.Name.Identifier.Text != "BindConfiguration")
        {
            return;
        }

        var receiverType = context.SemanticModel.GetTypeInfo(memberAccess.Expression).Type?.OriginalDefinition;
        if (receiverType is null)
        {
            return;
        }

        if (!HasOptionsBuilderType(receiverType))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation()));
    }

    private bool HasOptionsBuilderType(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol namedType)
        {
            return false;
        }

        if (!string.Equals(namedType.MetadataName, "OptionsBuilder`1", StringComparison.Ordinal))
        {
            return false;
        }

        var namespaceName = namedType.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        return string.Equals(namespaceName, "Microsoft.Extensions.Options", StringComparison.Ordinal);
    }
}
