using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace Automaticks.Testing;

/// <summary>
///     Warns when a test method name does not follow the three-part
///     <c>Method_Scenario_ExpectedResult</c> naming convention.
///     Only applies to methods decorated with <c>[Test]</c> or <c>[Arguments]</c> in test projects.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TestMethodNamingAnalyzer : DiagnosticAnalyzer
{
    private static readonly Regex NamingPattern =
        new(@"^[A-Za-z0-9]+_[A-Za-z0-9]+_[A-Za-z0-9]+$", RegexOptions.Compiled);

    /// <summary>
    ///     The diagnostic rule reported when a test method name does not match the
    ///     <c>Method_Scenario_ExpectedResult</c> pattern.
    /// </summary>
    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.Testing.TestMethodNaming,
        "Test method name must follow the three-part convention",
        "Test method '{0}' does not follow the naming convention '{Method}_{Scenario}_{ExpectedResult}'",
        "Testing",
        DiagnosticSeverity.Warning,
        true,
        "Rename the test method to use exactly three underscore-separated PascalCase segments: `{Method}_{Scenario}_{ExpectedResult}`. Example: `GetUser_UserNotFound_ThrowsNotFoundException`. Each segment is a PascalCase word or phrase; no additional underscores are allowed within a segment.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context)
    {
        var method = (IMethodSymbol)context.Symbol;
        if (!HasTestOrArgumentsAttribute(method))
        {
            return;
        }

        if (!NamingPattern.IsMatch(method.Name))
        {
            Location location;
            if (method.Locations.Length > 0)
            {
                location = method.Locations[0];
            }
            else
            {
                location = Location.None;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, location, method.Name));
        }
    }

    private static bool HasTestOrArgumentsAttribute(IMethodSymbol method)
    {
        foreach (var attr in method.GetAttributes())
        {
            if (attr.AttributeClass?.Name is "TestAttribute" or "ArgumentsAttribute")
            {
                return true;
            }
        }

        return false;
    }
}
