using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Automaticks.Diagnostics.CodeAnalysis.Analyzers.Tests.Stubs;

public sealed class TestAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
{
    private readonly TestAnalyzerConfigOptions _globalOptions;

    public override AnalyzerConfigOptions GlobalOptions => _globalOptions;

    public TestAnalyzerConfigOptionsProvider(bool isTestProject, bool isAnalyzerProject = false)
    {
        _globalOptions = new TestAnalyzerConfigOptions(isTestProject, isAnalyzerProject);
    }

    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
    {
        return _globalOptions;
    }

    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
    {
        return _globalOptions;
    }
}
