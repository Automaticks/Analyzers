using System.Collections.Generic;

namespace Automaticks.Testing.Coverage;

/// <summary>
///     Merged coverage counters for a single method taken from one or more coverage reports.
/// </summary>
public sealed class MethodCoverage
{
    private readonly Dictionary<int, int> _branchTotalsByLine;
    private readonly Dictionary<int, int> _coveredBranchesByLine;
    private readonly Dictionary<int, int> _hitsByLine;

    /// <summary>
    ///     Gets the number of branches recorded as taken.
    /// </summary>
    public int CoveredBranches
    {
        get
        {
            return Sum(_coveredBranchesByLine);
        }
    }

    /// <summary>
    ///     Gets the number of executed lines.
    /// </summary>
    public int CoveredLines
    {
        get
        {
            var covered = 0;
            foreach (var entry in _hitsByLine)
            {
                if (entry.Value > 0)
                {
                    covered += 1;
                }
            }

            return covered;
        }
    }

    /// <summary>
    ///     Gets a value indicating whether any line of the method was executed.
    /// </summary>
    public bool IsCovered
    {
        get
        {
            return CoveredLines > 0;
        }
    }

    /// <summary>
    ///     Gets the method name as written in the coverage report.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Gets the number of branches present.
    /// </summary>
    public int TotalBranches
    {
        get
        {
            return Sum(_branchTotalsByLine);
        }
    }

    /// <summary>
    ///     Gets the number of reported lines.
    /// </summary>
    public int TotalLines
    {
        get
        {
            return _hitsByLine.Count;
        }
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MethodCoverage" /> class.
    /// </summary>
    /// <param name="name">The method name as written in the coverage report.</param>
    public MethodCoverage(string name)
    {
        Name = name;
        _branchTotalsByLine = new Dictionary<int, int>();
        _coveredBranchesByLine = new Dictionary<int, int>();
        _hitsByLine = new Dictionary<int, int>();
    }

    /// <summary>
    ///     Records one reported line, keeping the best result seen for that line.
    /// </summary>
    /// <param name="lineNumber">The one-based source line number.</param>
    /// <param name="hits">The number of times the line was executed.</param>
    /// <param name="conditionCoverage">The Cobertura condition-coverage attribute, when present.</param>
    public void AddLine(int lineNumber, int hits, string? conditionCoverage)
    {
        Record(_hitsByLine, lineNumber, hits);
        var counts = ConditionCoverageParser.Parse(conditionCoverage);
        if (counts.Total == 0)
        {
            return;
        }

        Record(_coveredBranchesByLine, lineNumber, counts.Covered);
        Record(_branchTotalsByLine, lineNumber, counts.Total);
    }

    private void Record(Dictionary<int, int> values, int lineNumber, int value)
    {
        if (!values.TryGetValue(lineNumber, out var existing) || value > existing)
        {
            values[lineNumber] = value;
        }
    }

    private int Sum(Dictionary<int, int> values)
    {
        var total = 0;
        foreach (var entry in values)
        {
            total += entry.Value;
        }

        return total;
    }
}
