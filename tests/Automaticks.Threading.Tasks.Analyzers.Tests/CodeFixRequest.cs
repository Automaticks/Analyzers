using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Automaticks.Threading.Tasks.Analyzers.Tests;

/// <summary>Describes a single code fix invocation for <see cref="CodeFixTestRunner" />.</summary>
public readonly struct CodeFixRequest
{
    /// <summary>Gets the analyzer that produces the diagnostic being fixed.</summary>
    public DiagnosticAnalyzer Analyzer { get; init; }

    /// <summary>Gets the equivalence key selecting one of several offered fixes.</summary>
    public string? EquivalenceKey { get; init; }

    /// <summary>Gets the provider that supplies the fix.</summary>
    public CodeFixProvider Provider { get; init; }

    /// <summary>Gets the C# source code to fix.</summary>
    public string Source { get; init; }
}
