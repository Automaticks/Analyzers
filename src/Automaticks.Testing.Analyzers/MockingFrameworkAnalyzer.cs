using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace Automaticks.Testing;

/// <summary>
///     Flags using directives that import a mocking framework (Moq, NSubstitute, FakeItEasy, Telerik.JustMock, or Rhino.Mocks) in test projects.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MockingFrameworkAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule;
    private readonly string[] ForbiddenPrefixes;

    static MockingFrameworkAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.Testing.MockingFramework,
            "Mocking frameworks are not allowed",
            "Mocking framework '{0}' is not allowed. Use hand-written stubs in Stubs/ subdirectories instead.",
            "Testing",
            DiagnosticSeverity.Error,
            true,
            "Remove the mocking-framework usage. Instead, create a hand-written stub class (e.g., `StubFooService : IFooService`) in the `Stubs/` subdirectory of the test project and use that in your test. Banned frameworks: Moq, NSubstitute, FakeItEasy, Telerik.JustMock, Rhino.Mocks.");
        Rule = rule;
    }

    /// <summary>
    ///     Initializes the banned namespace prefixes inspected during analysis.
    /// </summary>
    public MockingFrameworkAnalyzer()
    {
        ForbiddenPrefixes =
        [
            "Moq",
            "NSubstitute",
            "FakeItEasy",
            "Telerik.JustMock",
            "Rhino.Mocks"
        ];
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeUsing, SyntaxKind.UsingDirective);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeUsing(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not UsingDirectiveSyntax usingDirective)
        {
            return;
        }

        var name = usingDirective.Name?.ToString() ?? string.Empty;
        foreach (var prefix in ForbiddenPrefixes)
        {
            if (name.Equals(prefix, StringComparison.Ordinal)
                || name.StartsWith(prefix + ".", StringComparison.Ordinal))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, usingDirective.GetLocation(), name));
                return;
            }
        }
    }
}
