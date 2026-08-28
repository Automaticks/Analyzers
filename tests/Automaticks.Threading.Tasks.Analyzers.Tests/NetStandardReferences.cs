using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;

namespace Automaticks.Threading.Tasks.Analyzers.Tests;

/// <summary>Supplies the netstandard2.0 reference assembly, which declares Task but not ValueTask.</summary>
public static class NetStandardReferences
{
    private static readonly IReadOnlyList<MetadataReference> Facade;

    static NetStandardReferences()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "ReferenceAssemblies", "netstandard.dll");
        Facade = [MetadataReference.CreateFromFile(path)];
    }

    /// <summary>Gets netstandard2.0 as the sole platform reference of a compilation.</summary>
    /// <returns>The netstandard2.0 reference assembly.</returns>
    public static IReadOnlyList<MetadataReference> GetFacade()
    {
        return Facade;
    }
}
