using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using static Automaticks.Threading.Tasks.AsyncReturnTypeHelper;

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
    /// <summary>
    ///     The diagnostic rule reported when an async-returning method is missing a
    ///     <see cref="System.Threading.CancellationToken" /> last parameter.
    /// </summary>
    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.ThreadingTasks.AsyncCancellationToken,
        "Async-returning methods must accept CancellationToken as the last parameter",
        "Method '{0}' returns an async type but does not have CancellationToken as its last parameter",
        "Threading.Tasks",
        DiagnosticSeverity.Error,
        true,
        "Add `CancellationToken cancellationToken` as the last parameter and propagate it to all inner async calls. Exempt: constructors, property accessors, `Main` entry points, event handlers, and `override`/`explicit interface` implementations whose base signature does not include a token.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context)
    {
        var method = (IMethodSymbol)context.Symbol;

        if (method.IsOverride && IsExternalOverride(method))
        {
            return;
        }

        if (method.ExplicitInterfaceImplementations.Length > 0)
        {
            foreach (var ifaceMethod in method.ExplicitInterfaceImplementations)
            {
                if (ifaceMethod.DeclaringSyntaxReferences.IsEmpty)
                {
                    return;
                }
            }
        }

        if (IsSignalRHubMethod(method, context.Compilation))
        {
            return;
        }

        if (IsImplicitExternalInterfaceImplementation(method))
        {
            return;
        }

        if (!ReturnsAsyncType(method, context.Compilation))
        {
            return;
        }

        var parameters = method.Parameters;
        if (parameters.Length > 0)
        {
            var cancellationTokenType = context.Compilation.GetTypeByMetadataName("System.Threading.CancellationToken");
            if (cancellationTokenType is not null)
            {
                var lastParam = parameters[parameters.Length - 1];
                if (SymbolEqualityComparer.Default.Equals(lastParam.Type, cancellationTokenType))
                {
                    return;
                }
            }
        }

        Location location;
        if (method.Locations.Length > 0)
        {
            location = method.Locations[0];
        }
        else
        {
            location = Location.None;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, location, method.Name));
    }

    private static bool IsExternalOverride(IMethodSymbol method)
    {
        var overridden = method.OverriddenMethod;
        while (overridden is not null)
        {
            if (overridden.DeclaringSyntaxReferences.IsEmpty)
            {
                return true;
            }

            overridden = overridden.OverriddenMethod;
        }

        return false;
    }

    private static bool IsImplicitExternalInterfaceImplementation(IMethodSymbol method)
    {
        if (method.IsOverride)
        {
            return false;
        }

        foreach (var iface in method.ContainingType.AllInterfaces)
        {
            foreach (var member in iface.GetMembers())
            {
                if (member is not IMethodSymbol ifaceMethod)
                {
                    continue;
                }

                if (ifaceMethod.DeclaringSyntaxReferences.IsEmpty &&
                    SymbolEqualityComparer.Default.Equals(
                        method.ContainingType.FindImplementationForInterfaceMember(ifaceMethod),
                        method))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsSignalRHubMethod(IMethodSymbol method, Compilation compilation)
    {
        var hubType = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.SignalR.Hub");
        if (hubType is null)
        {
            return false;
        }

        var containingType = method.ContainingType;
        var baseType = containingType?.BaseType;
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
}
