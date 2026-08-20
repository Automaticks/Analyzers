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
        context.RegisterCompilationStartAction(RegisterMethodAction);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeMethod(
        SymbolAnalysisContext context,
        AsyncReturnTypeHelper asyncReturnTypes,
        INamedTypeSymbol? cancellationTokenType,
        INamedTypeSymbol? hubType)
    {
        var method = (context.Symbol as IMethodSymbol)!;

        if (HasMethodExemption(method, hubType))
        {
            return;
        }

        if (!asyncReturnTypes.HasAsyncReturnType(method))
        {
            return;
        }

        if (HasTrailingCancellationToken(method, cancellationTokenType))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, method.Locations[0], method.Name));
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

    private bool HasMethodExemption(IMethodSymbol method, INamedTypeSymbol? hubType)
    {
        var hasExternalOverride = method.IsOverride && HasExternalOverride(method);
        var hasExemption = hasExternalOverride
               || HasExplicitExternalInterfaceImplementation(method)
               || HasImplicitExternalInterfaceImplementation(method)
               || HasSignalRealtimeHubMethod(method, hubType);
        return hasExemption;
    }

    private bool HasSignalRealtimeHubMethod(IMethodSymbol method, INamedTypeSymbol? hubType)
    {
        if (hubType is null)
        {
            return false;
        }

        var baseType = method.ContainingType.BaseType;
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

    private bool HasTrailingCancellationToken(IMethodSymbol method, INamedTypeSymbol? cancellationTokenType)
    {
        var parameters = method.Parameters;
        if (parameters.Length == 0)
        {
            return false;
        }

        var lastParameter = parameters[parameters.Length - 1];
        return SymbolEqualityComparer.Default.Equals(lastParameter.Type, cancellationTokenType);
    }

    private void RegisterMethodAction(CompilationStartAnalysisContext context)
    {
        var asyncReturnTypes = new AsyncReturnTypeHelper(context.Compilation);
        var cancellationTokenType = context.Compilation.GetTypeByMetadataName("System.Threading.CancellationToken");
        var hubType = context.Compilation.GetTypeByMetadataName("Microsoft.AspNetCore.SignalR.Hub");
        context.RegisterSymbolAction(
            symbolContext => AnalyzeMethod(symbolContext, asyncReturnTypes, cancellationTokenType, hubType),
            SymbolKind.Method);
    }
}
