using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Automaticks.Testing.Analyzers.Tests.Stubs;

/// <summary>Provides test-specific analyzer configuration values.</summary>
public sealed class TestAnalyzerConfigOptions : AnalyzerConfigOptions
{
    private readonly IReadOnlyDictionary<string, string>? _configOptions;
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
        : this(isTestProject, isAnalyzerProject, null)
    {
    }

    /// <summary>Initializes a new instance with project type flags and custom config options.</summary>
    /// <param name="isTestProject">Whether the project under test is a test project.</param>
    /// <param name="isAnalyzerProject">Whether the project under test is an analyzer project.</param>
    /// <param name="configOptions">Additional .editorconfig-style key/value pairs.</param>
    public TestAnalyzerConfigOptions(
        bool isTestProject,
        bool isAnalyzerProject,
        IReadOnlyDictionary<string, string>? configOptions)
    {
        _configOptions = configOptions;
        _isAnalyzerProject = isAnalyzerProject;
        _isTestProject = isTestProject;
    }

    /// <inheritdoc />
    public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
    {
        if (_configOptions is not null && _configOptions.TryGetValue(key, out var configValue))
        {
            value = configValue;
            return true;
        }

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
