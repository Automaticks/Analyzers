using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.Threading.Tasks;

/// <summary>
///     Flags methods that return <see cref="System.Threading.Tasks.Task" />,
///     <see cref="System.Threading.Tasks.Task{TResult}" />,
///     <see cref="System.Threading.Tasks.ValueTask" />,
///     <see cref="System.Threading.Tasks.ValueTask{TResult}" />, or
///     <c>IAsyncEnumerable&lt;T&gt;</c> but do not accept a
///     <see cref="System.Threading.CancellationToken" /> as their last parameter.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AsyncCancellationTokenAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule;

    static AsyncCancellationTokenAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.ThreadingTasks.AsyncCancellationToken,
            "Async-returning methods must accept CancellationToken as the last parameter",
            "Method '{0}' returns an async type but does not have CancellationToken as its last parameter",
            "Threading.Tasks",
            DiagnosticSeverity.Error,
            true,
            "Add `CancellationToken cancellationToken` as the last parameter and propagate it to all inner async calls. Exempt: constructors, property accessors, `Main` entry points, event handlers, and `override`/`explicit interface` implementations whose base signature does not include a token.");
        Rule = rule;
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
        if (context.Symbol is not IMethodSymbol method)
        {
            return;
        }

        if (HasMethodExemption(method, context.Compilation))
        {
            return;
        }

        if (!AsyncReturnTypeHelper.HasAsyncReturnType(method, context.Compilation))
        {
            return;
        }

        if (HasTrailingCancellationToken(method, context.Compilation))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, GetMethodLocation(method), method.Name));
    }

    private Location GetMethodLocation(IMethodSymbol method)
    {
        return method.Locations.Length > 0
            ? method.Locations[0]
            : Location.None;
    }

    private bool HasExplicitExternalInterfaceImplementation(IMethodSymbol method)
    {
        if (method.ExplicitInterfaceImplementations.Length == 0)
        {
            return false;
        }

        foreach (var interfaceMethod in method.ExplicitInterfaceImplementations)
        {
            if (interfaceMethod.DeclaringSyntaxReferences.IsEmpty)
            {
                return true;
            }
        }

        return false;
    }

    private bool HasExternalOverride(IMethodSymbol method)
    {
        var overriddenMethod = method.OverriddenMethod;
        while (overriddenMethod is not null)
        {
            if (overriddenMethod.DeclaringSyntaxReferences.IsEmpty)
            {
                return true;
            }

            overriddenMethod = overriddenMethod.OverriddenMethod;
        }

        return false;
    }

    private bool HasImplicitExternalInterfaceImplementation(IMethodSymbol method)
    {
        if (method.IsOverride)
        {
            return false;
        }

        foreach (var interfaceType in method.ContainingType.AllInterfaces)
        {
            foreach (var member in interfaceType.GetMembers())
            {
                if (member is not IMethodSymbol interfaceMethod)
                {
                    continue;
                }

                if (!interfaceMethod.DeclaringSyntaxReferences.IsEmpty)
                {
                    continue;
                }

                var implementation = method.ContainingType.FindImplementationForInterfaceMember(interfaceMethod);
                if (SymbolEqualityComparer.Default.Equals(implementation, method))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool HasMethodExemption(IMethodSymbol method, Compilation compilation)
    {
        return method.IsOverride && HasExternalOverride(method)
               || HasExplicitExternalInterfaceImplementation(method)
               || HasImplicitExternalInterfaceImplementation(method)
               || HasSignalRealtimeHubMethod(method, compilation);
    }

    private bool HasSignalRealtimeHubMethod(IMethodSymbol method, Compilation compilation)
    {
        var hubType = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.SignalR.Hub");
        if (hubType is null)
        {
            return false;
        }

        var baseType = method.ContainingType?.BaseType;
        while (baseType is not null)
        {
            if (SymbolEqualityComparer.Default.Equals(baseType.OriginalDefinition, hubType))
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    private bool HasTrailingCancellationToken(IMethodSymbol method, Compilation compilation)
    {
        var parameters = method.Parameters;
        if (parameters.Length == 0)
        {
            return false;
        }

        var cancellationTokenType = compilation.GetTypeByMetadataName("System.Threading.CancellationToken");
        if (cancellationTokenType is null)
        {
            return false;
        }

        var lastParameter = parameters[parameters.Length - 1];
        return SymbolEqualityComparer.Default.Equals(lastParameter.Type, cancellationTokenType);
    }
}
