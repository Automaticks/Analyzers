using Microsoft.CodeAnalysis.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Automaticks.Extensions.Options.Analyzers.Tests.Stubs;

/// <summary>Provides test-specific analyzer configuration values.</summary>
public sealed class TestAnalyzerConfigOptions : AnalyzerConfigOptions
{
    private readonly bool _isAnalyzerProject;
    private readonly bool _isTestProject;

    /// <summary>Initializes a new instance with only the IsTestProject flag.</summary>
    /// <param name="isTestProject">Whether the project under test is a test project.</param>
    public TestAnalyzerConfigOptions(bool isTestProject) : this(isTestProject, false)
    {
    }

    /// <summary>Initializes a new instance with both project type flags.</summary>
    /// <param name="isTestProject">Whether the project under test is a test project.</param>
    /// <param name="isAnalyzerProject">Whether the project under test is an analyzer project.</param>
    public TestAnalyzerConfigOptions(bool isTestProject, bool isAnalyzerProject)
    {
        _isTestProject = isTestProject;
        _isAnalyzerProject = isAnalyzerProject;
    }

    /// <inheritdoc />
    public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
    {
        if (key == "build_property.IsTestProject")
        {
            value = _isTestProject ? "true" : "false";
            return true;
        }

        if (key == "build_property.IsAnalyzerProject")
        {
            value = _isAnalyzerProject ? "true" : "false";
            return true;
        }

        value = null;
        return false;
    }
}
