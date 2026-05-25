using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.Threading.Tasks;

/// <summary>
///     Flags invocations of task-returning methods whose result is discarded without being
///     awaited, returned, assigned to a variable, or passed as an argument. Discarding a
///     <see cref="System.Threading.Tasks.Task" /> or
///     <see cref="System.Threading.Tasks.ValueTask" /> silently swallows exceptions.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UnobservedTaskAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a task-returning invocation result is discarded.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.ThreadingTasks.UnobservedTask,
        "Unobserved Task invocation",
        "The result of invocation '{0}' is discarded. Await or observe the returned Task/ValueTask to prevent unobserved failures.",
        "Threading.Tasks",
        DiagnosticSeverity.Error,
        true,
        "Await, return, assign to a variable, or pass as an argument the `Task` or `ValueTask` returned by this method. Discarding an awaitable silently swallows all exceptions and prevents the caller from knowing when the work completes. Change `DoWorkAsync();` to `await DoWorkAsync();` or `return DoWorkAsync();`.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol method)
        {
            return;
        }

        if (!ReturnsTaskType(method, context.SemanticModel.Compilation))
        {
            return;
        }

        if (IsResultDiscarded(invocation))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation(), invocation.ToString()));
        }
    }

    private static bool IsResultDiscarded(InvocationExpressionSyntax invocation)
    {
        var parent = invocation.Parent;

        return parent is ExpressionStatementSyntax ||
               (parent is AssignmentExpressionSyntax assignment &&
                assignment.Right == invocation &&
                assignment is { Left: IdentifierNameSyntax { Identifier.Text: "_" }, Parent: ExpressionStatementSyntax });
    }

    private static bool ReturnsTaskType(IMethodSymbol method, Compilation compilation)
    {
        var returnType = method.ReturnType;

        var taskType = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
        var taskOfTType = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
        var valueTaskType = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask");
        var valueTaskOfTType = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1");

        if (SymbolEqualityComparer.Default.Equals(returnType, taskType))
        {
            return true;
        }

        if (SymbolEqualityComparer.Default.Equals(returnType, valueTaskType))
        {
            return true;
        }

        if (returnType is INamedTypeSymbol { IsGenericType: true } namedType)
        {
            var unboundType = namedType.ConstructUnboundGenericType();
            return SymbolEqualityComparer.Default.Equals(unboundType, taskOfTType?.ConstructUnboundGenericType()) ||
                   SymbolEqualityComparer.Default.Equals(unboundType, valueTaskOfTType?.ConstructUnboundGenericType());
        }

        return false;
    }
}
