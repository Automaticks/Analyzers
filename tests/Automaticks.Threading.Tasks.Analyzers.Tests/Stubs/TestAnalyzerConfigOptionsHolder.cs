using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Automaticks.Threading.Tasks.Analyzers.Tests.Stubs;

/// <summary>Provides analyzer configuration options for test compilations.</summary>
public sealed class TestAnalyzerConfigOptionsHolder : AnalyzerConfigOptionsProvider
{
    private readonly TestAnalyzerConfigOptions _globalOptions;

    /// <summary>Initializes a new instance with only the IsTestProject flag.</summary>
    /// <param name="isTestProject">Whether the project under test is a test project.</param>
    public TestAnalyzerConfigOptionsHolder(bool isTestProject) : this(isTestProject, false)
    {
    }

    /// <summary>Initializes a new instance with both project type flags.</summary>
    /// <param name="isTestProject">Whether the project under test is a test project.</param>
    /// <param name="isAnalyzerProject">Whether the project under test is an analyzer project.</param>
    public TestAnalyzerConfigOptionsHolder(bool isTestProject, bool isAnalyzerProject)
    {
        var configOptions = new TestAnalyzerConfigOptions(isTestProject, isAnalyzerProject);
        _globalOptions = configOptions;
    }

    /// <inheritdoc />
    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
    {
        return _globalOptions;
    }

    /// <inheritdoc />
    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
    {
        return _globalOptions;
    }

    /// <inheritdoc />
    public override AnalyzerConfigOptions GlobalOptions => _globalOptions;
}
