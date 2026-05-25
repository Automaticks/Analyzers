using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp;

/// <summary>
///     Flags any parameter declared with the <c>params</c> modifier.
///     <c>params</c> implicitly allocates a new array on every call site; callers must instead
///     pass a typed collection directly.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ParamsParameterAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a <c>params</c> parameter is found.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.CSharp.ParamsParameter,
        "The params keyword is forbidden",
        "Remove the params modifier and replace it with a typed collection parameter",
        "CSharp",
        DiagnosticSeverity.Error,
        true,
        "Remove the `params` modifier and change the parameter type to an explicit collection such as `IReadOnlyList<T>`, `IReadOnlyCollection<T>`, or `T[]`. Update all call sites to pass a collection or array explicitly. `params` allocates a new array on every call site, introducing hidden heap pressure.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeParameter, SyntaxKind.Parameter);
    }

    private static void AnalyzeParameter(SyntaxNodeAnalysisContext context)
    {
        var parameter = (ParameterSyntax)context.Node;

        if (!parameter.Modifiers.Any(SyntaxKind.ParamsKeyword))
        {
            return;
        }

        foreach (var modifier in parameter.Modifiers)
        {
            if (!modifier.IsKind(SyntaxKind.ParamsKeyword))
            {
                continue;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, modifier.GetLocation()));
            return;
        }
    }
}
