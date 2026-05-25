using Microsoft.CodeAnalysis.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Automaticks.CommunityToolkit.Mvvm.Analyzers.Tests.Stubs;

public sealed class TestAnalyzerConfigOptions : AnalyzerConfigOptions
{
    private readonly bool _isAnalyzerProject;
    private readonly bool _isTestProject;

    public TestAnalyzerConfigOptions(bool isTestProject, bool isAnalyzerProject = false)
    {
        _isTestProject = isTestProject;
        _isAnalyzerProject = isAnalyzerProject;
    }

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
