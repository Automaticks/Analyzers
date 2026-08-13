using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.Naming;

/// <summary>
///     Flags static methods declared inside non-static classes. Static methods that do not
///     depend on instance state must be moved to a dedicated static helper or extension class.
///     Extension methods (first parameter has <c>this</c> modifier) are exempt.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class StaticMethodInNonStaticClassAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a static method is found inside a non-static class.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static StaticMethodInNonStaticClassAnalyzer()
    {
        Rule = new(
            DiagnosticIds.CSharp.StaticMethodInNonStaticClass,
            "Static methods must only exist in static classes",
            "Method '{0}' is static but is declared in non-static class '{1}'. Move it to a dedicated static class.",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "Move the static method to a dedicated `public static class`. Static methods in non-static classes cannot participate in dependency injection, are hard to mock, and often indicate a design smell. Create or reuse a static helper class and relocate the method there.");
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MethodDeclarationSyntax method)
        {
            return;
        }

        if (!method.Modifiers.Any(SyntaxKind.StaticKeyword))
        {
            return;
        }

        if (method.Parent is not ClassDeclarationSyntax containingClass)
        {
            return;
        }

        if (containingClass.Modifiers.Any(SyntaxKind.StaticKeyword))
        {
            return;
        }

        if (HasThisParameterModifier(method))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            Rule,
            method.Identifier.GetLocation(),
            method.Identifier.Text,
            containingClass.Identifier.Text));
    }

    private bool HasThisParameterModifier(MethodDeclarationSyntax method)
    {
        var parameters = method.ParameterList.Parameters;
        return parameters.Count != 0 && parameters[0].Modifiers.Any(SyntaxKind.ThisKeyword);
    }
}
