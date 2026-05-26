using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.Testing;

/// <summary>
///     Flags any invocation of <c>Task.Delay</c> without a <c>TimeProvider</c> argument.
///     Using <c>Task.Delay</c> as a synchronisation primitive produces flaky results
///     because wall-clock time is unreliable under CI load. Use awaitable synchronisation
///     primitives (e.g. <c>SemaphoreSlim.WaitAsync</c>, <c>TaskCompletionSource</c>) instead.
///     Calls that pass a <c>TimeProvider</c> are exempt because they can be
///     driven deterministically in tests.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TaskDelayInTestAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule;

    static TaskDelayInTestAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.Testing.TaskDelay,
            "Task.Delay without TimeProvider is not allowed",
            "Task.Delay without a TimeProvider is forbidden. Replace it with a proper awaitable synchronisation primitive (e.g. WaitForBehaviorStepAsync, SemaphoreSlim.WaitAsync, or TaskCompletionSource), or pass a TimeProvider to make timing controllable.",
            "Testing",
            DiagnosticSeverity.Error,
            true,
            "Replace `Task.Delay(...)` with a proper synchronization primitive that reacts to the actual event being waited for. Use `SemaphoreSlim.WaitAsync(...)`, `TaskCompletionSource`, or a test-framework helper such as `WaitForBehaviorStepAsync`. `Task.Delay` uses wall-clock time and produces flaky tests under CI load where timing is not guaranteed.");
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

        if (!HasTaskDelayMethod(method, context.SemanticModel.Compilation))
        {
            return;
        }

        if (HasTimeProviderParameter(method, context.SemanticModel.Compilation))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation()));
    }

    private bool HasTaskDelayMethod(IMethodSymbol method, Compilation compilation)
    {
        if (method.Name != "Delay")
        {
            return false;
        }

        var taskType = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
        if (taskType is null)
        {
            return false;
        }

        return SymbolEqualityComparer.Default.Equals(method.ContainingType, taskType);
    }

    private bool HasTimeProviderParameter(IMethodSymbol method, Compilation compilation)
    {
        var timeProviderType = compilation.GetTypeByMetadataName("System.TimeProvider");
        if (timeProviderType is null)
        {
            return false;
        }

        foreach (var parameter in method.Parameters)
        {
            if (SymbolEqualityComparer.Default.Equals(parameter.Type, timeProviderType))
            {
                return true;
            }
        }

        return false;
    }
}
