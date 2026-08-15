using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Threading;

namespace Automaticks.Testing.Analyzers.Tests.Stubs;

/// <summary>Provides an in-memory additional file for analyzers that read AdditionalFiles.</summary>
public sealed class TestAdditionalText : AdditionalText
{
    private readonly SourceText _text;

    /// <summary>Initializes a new instance with the specified path and content.</summary>
    /// <param name="path">The reported file path.</param>
    /// <param name="content">The file content.</param>
    public TestAdditionalText(string path, string content)
    {
        Path = path;
        _text = SourceText.From(content);
    }

    /// <inheritdoc />
    public override SourceText GetText(CancellationToken cancellationToken = default)
    {
        return _text;
    }

    /// <inheritdoc />
    public override string Path { get; }
}
