using Microsoft.CodeAnalysis;

namespace Automaticks.Threading.Tasks;

/// <summary>
///     Shared helper for checking whether a method returns an async type
///     (Task, Task&lt;T&gt;, ValueTask, ValueTask&lt;T&gt;, or IAsyncEnumerable&lt;T&gt;).
/// </summary>
public static class AsyncReturnTypeHelper
{
    /// <summary>
    ///     Returns <see langword="true" /> when <paramref name="method" /> returns an async type.
    /// </summary>
    /// <param name="method">The method symbol to inspect.</param>
    /// <param name="compilation">The compilation that provides framework type symbols.</param>
    /// <returns><see langword="true" /> when the method returns an async type; otherwise, <see langword="false" />.</returns>
    public static bool HasAsyncReturnType(IMethodSymbol method, Compilation compilation)
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
        if (SymbolEqualityComparer.Default.Equals(unboundType, taskOfGenericType?.ConstructUnboundGenericType()))
        {
            return true;
        }

        if (SymbolEqualityComparer.Default.Equals(unboundType, valueTaskOfGenericType?.ConstructUnboundGenericType()))
        {
            return true;
        }

        return HasAsyncEnumerableType(namedType);
    }

    private static bool HasAsyncEnumerableType(INamedTypeSymbol type)
    {
        return type.Name == "IAsyncEnumerable"
               && type.TypeArguments.Length == 1
               && type.ContainingNamespace?.ToDisplayString() == "System.Collections.Generic";
    }
}
