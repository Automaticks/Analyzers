namespace Automaticks.Testing.Coverage;

/// <summary>
///     Accumulated coverage counters for a single method taken from a coverage report.
/// </summary>
public sealed class MethodCoverage
{
    /// <summary>
    ///     Gets the number of branches recorded as taken.
    /// </summary>
    public int CoveredBranches { get; private set; }

    /// <summary>
    ///     Gets the number of executed lines.
    /// </summary>
    public int CoveredLines { get; private set; }

    /// <summary>
    ///     Gets a value indicating whether any line of the method was executed.
    /// </summary>
    public bool IsCovered => CoveredLines > 0;

    /// <summary>
    ///     Gets the method name as written in the coverage report.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Gets the number of branches present.
    /// </summary>
    public int TotalBranches { get; private set; }

    /// <summary>
    ///     Gets the number of reported lines.
    /// </summary>
    public int TotalLines { get; private set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MethodCoverage" /> class.
    /// </summary>
    /// <param name="name">The method name as written in the coverage report.</param>
    public MethodCoverage(string name)
    {
        Name = name;
    }

    /// <summary>
    ///     Records one reported line belonging to this method.
    /// </summary>
    /// <param name="hits">The number of times the line was executed.</param>
    /// <param name="conditionCoverage">The Cobertura condition-coverage attribute, when present.</param>
    public void AddLine(int hits, string? conditionCoverage)
    {
        TotalLines += 1;
        if (hits > 0)
        {
            CoveredLines += 1;
        }

        AddBranches(conditionCoverage);
    }

    private void AddBranches(string? conditionCoverage)
    {
        if (string.IsNullOrEmpty(conditionCoverage))
        {
            return;
        }

        var open = conditionCoverage!.IndexOf('(');
        if (open < 0)
        {
            return;
        }

        var slash = conditionCoverage.IndexOf('/', open + 1);
        if (slash < 0)
        {
            return;
        }

        var close = conditionCoverage.IndexOf(')', slash + 1);
        if (close < 0)
        {
            return;
        }

        var coveredText = conditionCoverage.Substring(open + 1, slash - open - 1);
        var totalText = conditionCoverage.Substring(slash + 1, close - slash - 1);
        if (int.TryParse(coveredText, out var covered) && int.TryParse(totalText, out var total))
        {
            CoveredBranches += covered;
            TotalBranches += total;
        }
    }
}
