using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.Linq;

/// <summary>
///     Flags calls to System.Linq.Enumerable and System.Linq.Queryable operators, and query expressions, by resolving the called symbol rather than readin...
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
        context.RegisterCompilationStartAction(RegisterPerCompilation);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [Rule];
        }
    }

    private void AnalyzeInvocation(SyntaxNodeAnalysisContext context, LinqSymbols symbols)
    {
        if (context.SemanticModel.GetSymbolInfo(context.Node).Symbol is not IMethodSymbol method)
        {
            return;
        }

        var containingType = method.ContainingType;
        if (!SymbolEqualityComparer.Default.Equals(containingType, symbols.Enumerable)
            && !SymbolEqualityComparer.Default.Equals(containingType, symbols.Queryable))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), method.Name));
    }

    private void AnalyzeQueryExpression(SyntaxNodeAnalysisContext context)
    {
        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), "query expression"));
    }

    private void RegisterPerCompilation(CompilationStartAnalysisContext compilationContext)
    {
        var symbols = new LinqSymbols(
            compilationContext.Compilation.GetTypeByMetadataName("System.Linq.Enumerable"),
            compilationContext.Compilation.GetTypeByMetadataName("System.Linq.Queryable"));
        compilationContext.RegisterSyntaxNodeAction(
            context => AnalyzeInvocation(context, symbols),
            SyntaxKind.InvocationExpression);
        compilationContext.RegisterSyntaxNodeAction(AnalyzeQueryExpression, SyntaxKind.QueryExpression);
    }

    private readonly struct LinqSymbols
    {
        public INamedTypeSymbol? Enumerable { get; }

        public INamedTypeSymbol? Queryable { get; }

        public LinqSymbols(INamedTypeSymbol? enumerable, INamedTypeSymbol? queryable)
        {
            Enumerable = enumerable;
            Queryable = queryable;
        }
    }
}
