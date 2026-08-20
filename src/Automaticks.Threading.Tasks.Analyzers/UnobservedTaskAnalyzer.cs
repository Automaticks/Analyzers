using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.Threading.Tasks;

/// <summary>
///     Flags invocations of task-returning methods whose result is discarded without being awaited, returned, assigned to a variable, or passed as an argu...
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UnobservedTaskAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule;

    static UnobservedTaskAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.ThreadingTasks.UnobservedTask,
            "Unobserved Task invocation",
            "The result of invocation '{0}' is discarded. Await or observe the returned Task/ValueTask to prevent unobserved failures. A code fix is available (dotnet format analyzers --diagnostics ATXTA010).",
            "Threading.Tasks",
            DiagnosticSeverity.Error,
            true,
            "Await, return, assign to a variable, or pass as an argument the `Task` or `ValueTask` returned by this method. Discarding an awaitable silently swallows all exceptions and prevents the caller from knowing when the work completes. Change `DoWorkAsync();` to `await DoWorkAsync();` or `return DoWorkAsync();`.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
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

        if (!HasTaskReturnType(method, context.SemanticModel.Compilation))
        {
            return;
        }

        if (HasDiscardedResult(invocation))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation(), invocation.ToString()));
        }
    }

    private bool HasDiscardedResult(InvocationExpressionSyntax invocation)
    {
        var parent = invocation.Parent;
        var isDiscardAssignment = parent is AssignmentExpressionSyntax assignment
            && assignment.Right == invocation
            && assignment is { Left: IdentifierNameSyntax { Identifier.Text: "_" }, Parent: ExpressionStatementSyntax };
        return parent is ExpressionStatementSyntax || isDiscardAssignment;
    }

    private bool HasTaskReturnType(IMethodSymbol method, Compilation compilation)
    {
        var returnType = method.ReturnType;

        var taskType = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
        var taskOfGenericType = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
        var valueTaskType = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask");
        var valueTaskOfGenericType = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1");

        if (SymbolEqualityComparer.Default.Equals(returnType, taskType))
        {
            return true;
        }

        if (SymbolEqualityComparer.Default.Equals(returnType, valueTaskType))
        {
            return true;
        }

        if (returnType is not INamedTypeSymbol { IsGenericType: true } namedType)
        {
            return false;
        }

        var unboundType = namedType.ConstructUnboundGenericType();
        return SymbolEqualityComparer.Default.Equals(unboundType, taskOfGenericType?.ConstructUnboundGenericType())
               || SymbolEqualityComparer.Default.Equals(unboundType, valueTaskOfGenericType?.ConstructUnboundGenericType());
    }
}
