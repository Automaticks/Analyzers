using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace Automaticks.Testing;

/// <summary>
///     Warns when a test method name does not follow the three-part Method_Scenario_ExpectedResult naming convention.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TestMethodNamingAnalyzer : DiagnosticAnalyzer
{
    private static readonly Regex NamingPattern;
    private static readonly DiagnosticDescriptor Rule;

    static TestMethodNamingAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.Testing.TestMethodNaming,
            "Test method name must follow the three-part convention",
            "Test method '{0}' does not follow the naming convention '{Method}_{Scenario}_{ExpectedResult}'",
            "Testing",
            DiagnosticSeverity.Warning,
            true,
            "Rename the test method to use exactly three underscore-separated PascalCase segments: `{Method}_{Scenario}_{ExpectedResult}`. Example: `GetUser_UserNotFound_ThrowsNotFoundException`. Each segment is a PascalCase word or phrase; no additional underscores are allowed within a segment.");
        Rule = rule;
        var namingPattern = new Regex(@"^[A-Za-z0-9]+(?:_[A-Za-z0-9]+){2,}$", RegexOptions.Compiled);
        NamingPattern = namingPattern;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeMethod(SymbolAnalysisContext context)
    {
        var method = (context.Symbol as IMethodSymbol)!;
        if (!HasTestOrArgumentsAttribute(method))
        {
            return;
        }

        if (!NamingPattern.IsMatch(method.Name))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, method.Locations[0], method.Name));
        }
    }

    private bool HasTestOrArgumentsAttribute(IMethodSymbol method)
    {
        foreach (var attribute in method.GetAttributes())
        {
            if (attribute.AttributeClass!.Name is "TestAttribute" or "ArgumentsAttribute")
            {
                return true;
            }
        }

        return false;
    }
}
