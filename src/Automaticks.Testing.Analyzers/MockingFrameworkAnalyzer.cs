using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace Automaticks.Testing;

/// <summary>
///     Flags <c>using</c> directives that import a mocking framework (Moq, NSubstitute,
///     FakeItEasy, Telerik.JustMock, or Rhino.Mocks) in test projects.
///     Hand-written stubs placed in <c>Stubs/</c> subdirectories must be used instead.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MockingFrameworkAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a mocking-framework namespace is imported.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.Testing.MockingFramework,
        "Mocking frameworks are not allowed",
        "Mocking framework '{0}' is not allowed. Use hand-written stubs in Stubs/ subdirectories instead.",
        "Testing",
        DiagnosticSeverity.Error,
        true,
        "Remove the mocking-framework usage. Instead, create a hand-written stub class (e.g., `StubFooService : IFooService`) in the `Stubs/` subdirectory of the test project and use that in your test. Banned frameworks: Moq, NSubstitute, FakeItEasy, Telerik.JustMock, Rhino.Mocks.");

    private static readonly string[] ForbiddenPrefixes =
    [
        "Moq",
        "NSubstitute",
        "FakeItEasy",
        "Telerik.JustMock",
        "Rhino.Mocks"
    ];

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeUsing, SyntaxKind.UsingDirective);
    }

    private static void AnalyzeUsing(SyntaxNodeAnalysisContext context)
    {
        var usingDirective = (UsingDirectiveSyntax)context.Node;
        var name = usingDirective.Name?.ToString() ?? string.Empty;

        foreach (var prefix in ForbiddenPrefixes)
        {
            if (name.Equals(prefix, StringComparison.Ordinal) ||
                name.StartsWith(prefix + ".", StringComparison.Ordinal))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, usingDirective.GetLocation(), name));
                return;
            }
        }
    }
}
