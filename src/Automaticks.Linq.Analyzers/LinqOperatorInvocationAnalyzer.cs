using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.Linq;

/// <summary>
///     Flags calls to <c>System.Linq.Enumerable</c> and <c>System.Linq.Queryable</c> operators, and
///     query expressions, by resolving the called symbol rather than reading using directives.
///     <c>ATXLQ002</c> only inspects using directives, so it goes silent when
///     <c>ImplicitUsings</c> brings <c>System.Linq</c> into scope through a generated global using.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LinqOperatorInvocationAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule;

    static LinqOperatorInvocationAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.Linq.LinqOperatorInvocation,
            "LINQ operator calls are not allowed",
            "'{0}' is a LINQ operator. Use an explicit loop instead.",
            "Linq",
            DiagnosticSeverity.Warning,
            true,
            "Replace the LINQ operator with an explicit `foreach` loop. This rule resolves the called method symbol, so it still fires when `System.Linq` is in scope through an implicit global using and no `using System.Linq;` directive is visible in the file.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        context.RegisterSyntaxNodeAction(AnalyzeQueryExpression, SyntaxKind.QueryExpression);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InvocationExpressionSyntax invocation)
        {
            return;
        }

        if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol method)
        {
            return;
        }

        if (!HasLinqDeclaringType(method, context.SemanticModel.Compilation))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation(), method.Name));
    }

    private void AnalyzeQueryExpression(SyntaxNodeAnalysisContext context)
    {
        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), "query expression"));
    }

    private bool HasLinqDeclaringType(IMethodSymbol method, Compilation compilation)
    {
        var containingType = method.ContainingType;
        if (containingType is null)
        {
            return false;
        }

        var enumerableType = compilation.GetTypeByMetadataName("System.Linq.Enumerable");
        if (enumerableType is not null && SymbolEqualityComparer.Default.Equals(containingType, enumerableType))
        {
            return true;
        }

        var queryableType = compilation.GetTypeByMetadataName("System.Linq.Queryable");
        return queryableType is not null && SymbolEqualityComparer.Default.Equals(containingType, queryableType);
    }
}
