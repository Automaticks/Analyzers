using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Automaticks.Threading.Tasks.Analyzers.Tests;

/// <summary>Configuration options for an analysis run.</summary>
public readonly struct AnalysisOptions
{
    /// <summary>Gets the additional metadata references to include in compilation.</summary>
    public IReadOnlyList<MetadataReference>? AdditionalReferences { get; init; }

    /// <summary>Gets the file path to use when parsing the source text.</summary>
    public string? FilePath { get; init; }

    /// <summary>Gets a value indicating whether the project under test is an analyzer project.</summary>
    public bool IsAnalyzerProject { get; init; }

    /// <summary>Gets a value indicating whether the project under test is a test project.</summary>
    public bool IsTestProject { get; init; }

    /// <summary>Gets the metadata references that replace the default platform references.</summary>
    public IReadOnlyList<MetadataReference>? PlatformReferences { get; init; }
}
