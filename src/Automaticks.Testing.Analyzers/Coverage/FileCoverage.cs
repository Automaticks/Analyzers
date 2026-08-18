using System;
using System.Collections.Generic;

namespace Automaticks.Testing.Coverage;

/// <summary>
///     Accumulated coverage counters for a single source file taken from a coverage report.
///     Overloads that share a name are merged, so a name counts as covered when any overload ran.
/// </summary>
public sealed class FileCoverage
{
    private readonly HashSet<int> _countedLines;
    private readonly HashSet<int> _coveredLineNumbers;
    private readonly Dictionary<string, MethodCoverage> _methods;

    /// <summary>
    ///     Gets the number of executed lines.
    /// </summary>
    public int CoveredLines { get; private set; }

    /// <summary>
    ///     Gets the percentage of executed lines, or -1 when the file reports no lines.
    /// </summary>
    public int LinePercentage => TotalLines == 0 ? -1 : CoveredLines * 100 / TotalLines;

    /// <summary>
    ///     Gets the file path exactly as written in the coverage report.
    /// </summary>
    public string ReportedPath { get; }

    /// <summary>
    ///     Gets the number of reported lines.
    /// </summary>
    public int TotalLines { get; private set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="FileCoverage" /> class.
    /// </summary>
    /// <param name="reportedPath">The file path exactly as written in the coverage report.</param>
    public FileCoverage(string reportedPath)
    {
        ReportedPath = reportedPath;
        _methods = new Dictionary<string, MethodCoverage>(StringComparer.Ordinal);
        _countedLines = new HashSet<int>();
        _coveredLineNumbers = new HashSet<int>();
    }

    /// <summary>
    ///     Records one reported line against the file totals. A line counts once towards the
    ///     total, and counts as covered when any report shows it executed. Cobertura repeats a
    ///     line at class and method level, and a solution-wide run emits one report per test
    ///     project, so the same line arrives several times with different hit counts.
    /// </summary>
    /// <param name="lineNumber">The one-based source line number.</param>
    /// <param name="hits">The number of times the line was executed.</param>
    public void AddLine(int lineNumber, int hits)
    {
        if (_countedLines.Add(lineNumber))
        {
            TotalLines += 1;
        }

        if (hits > 0 && _coveredLineNumbers.Add(lineNumber))
        {
            CoveredLines += 1;
        }
    }

    /// <summary>
    ///     Finds the merged coverage recorded for a method name.
    /// </summary>
    /// <param name="name">The method name to locate.</param>
    /// <returns>The coverage entry, or <see langword="null" /> when the name is absent.</returns>
    public MethodCoverage? FindMethod(string name)
    {
        return _methods.TryGetValue(name, out var method) ? method : null;
    }

    /// <summary>
    ///     Gets the coverage entry for a method name, creating it when first seen.
    /// </summary>
    /// <param name="name">The method name to look up.</param>
    /// <returns>The coverage entry for the name.</returns>
    public MethodCoverage GetOrAddMethod(string name)
    {
        if (!_methods.TryGetValue(name, out var method))
        {
            method = new MethodCoverage(name);
            _methods.Add(name, method);
        }

        return method;
    }
}
