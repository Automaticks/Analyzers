namespace Automaticks.Testing.Analyzers.Tests;

/// <summary>Represents a source file with its content and path.</summary>
public record struct SourceFile
{
    /// <summary>Gets the file path of the source file.</summary>
    public string FilePath { get; init; }

    /// <summary>Gets the source code content.</summary>
    public string Source { get; init; }
}
