using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.LanguageFeatures;

/// <summary>
///     Flags all explicit cast expressions where the target type is a reference type (class or interface).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DirectCastAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a direct cast to a reference type is detected.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static DirectCastAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.DirectCast,
            "Direct cast to reference type is forbidden",
            "Direct cast to '{0}' is forbidden. Use pattern matching ('is {0} x') or an implicit conversion instead.",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "Replace the C-style direct cast `(Type)expression` with pattern matching. Use `expression is Type variable` to test and bind in one step, or `expression as Type` for nullable reference-type conversions. Direct casts throw `InvalidCastException` at runtime when the object is not the target type.");
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

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not CastExpressionSyntax castExpression)
        {
            return;
        }

        var typeInfo = context.SemanticModel.GetTypeInfo(castExpression.Type);
        var targetType = typeInfo.Type;

        if (targetType is null)
        {
            return;
        }

        if (targetType.IsValueType)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, castExpression.GetLocation(), targetType.Name));
    }

    private void RegisterPerCompilation(CompilationStartAnalysisContext compilationContext)
    {
        compilationContext.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.CastExpression);
    }
}
