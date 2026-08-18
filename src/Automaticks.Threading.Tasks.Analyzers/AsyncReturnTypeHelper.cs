using Microsoft.CodeAnalysis;

namespace Automaticks.Threading.Tasks;

/// <summary>
///     Resolves the async return types once per compilation and matches method return types against them.
/// </summary>
public sealed class AsyncReturnTypeHelper
{
    private readonly INamedTypeSymbol? _task;
    private readonly INamedTypeSymbol? _taskOfGeneric;
    private readonly INamedTypeSymbol? _valueTask;
    private readonly INamedTypeSymbol? _valueTaskOfGeneric;

    /// <summary>
    ///     Resolves the async return types for <paramref name="compilation" /> once.
    /// </summary>
    /// <param name="compilation">The compilation that provides framework type symbols.</param>
    public AsyncReturnTypeHelper(Compilation compilation)
    {
        _task = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
        _taskOfGeneric = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
        _valueTask = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask");
        _valueTaskOfGeneric = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1");
    }

    /// <summary>
    ///     Returns <see langword="true" /> when <paramref name="method" /> returns an async type.
    /// </summary>
    /// <param name="method">The method symbol to inspect.</param>
    /// <returns><see langword="true" /> when the method returns an async type; otherwise, <see langword="false" />.</returns>
    public bool HasAsyncReturnType(IMethodSymbol method)
    {
        var returnType = method.ReturnType;

        if (SymbolEqualityComparer.Default.Equals(returnType, _task))
        {
            return true;
        }

        if (SymbolEqualityComparer.Default.Equals(returnType, _valueTask))
        {
            return true;
        }

        if (returnType is not INamedTypeSymbol { IsGenericType: true } namedType)
        {
            return false;
        }

        var definition = namedType.OriginalDefinition;
        if (SymbolEqualityComparer.Default.Equals(definition, _taskOfGeneric))
        {
            return true;
        }

        if (SymbolEqualityComparer.Default.Equals(definition, _valueTaskOfGeneric))
        {
            return true;
        }

        return HasAsyncEnumerableType(namedType);
    }

    private bool HasAsyncEnumerableType(INamedTypeSymbol type)
    {
        return type.Name == "IAsyncEnumerable"
               && type.TypeArguments.Length == 1
               && type.ContainingNamespace.ToDisplayString() == "System.Collections.Generic";
    }
}
