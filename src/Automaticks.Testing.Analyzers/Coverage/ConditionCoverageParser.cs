namespace Automaticks.Testing.Coverage;

/// <summary>
///     Reads the Cobertura condition-coverage attribute.
/// </summary>
public static class ConditionCoverageParser
{
    /// <summary>
    ///     Reads the covered and total branch counts, returning zeroes when the text is unusable.
    /// </summary>
    /// <param name="conditionCoverage">The Cobertura condition-coverage attribute, when present.</param>
    /// <returns>The counts, or zeroes when nothing could be read.</returns>
    public static BranchCounts Parse(string? conditionCoverage)
    {
        if (string.IsNullOrEmpty(conditionCoverage))
        {
            return new BranchCounts(0, 0);
        }

        var open = conditionCoverage!.IndexOf('(');
        if (open < 0)
        {
            return new BranchCounts(0, 0);
        }

        var slash = conditionCoverage.IndexOf('/', open + 1);
        if (slash < 0)
        {
            return new BranchCounts(0, 0);
        }

        var close = conditionCoverage.IndexOf(')', slash + 1);
        if (close < 0)
        {
            return new BranchCounts(0, 0);
        }

        var coveredText = conditionCoverage.Substring(open + 1, slash - open - 1);
        var totalText = conditionCoverage.Substring(slash + 1, close - slash - 1);
        if (int.TryParse(coveredText, out var covered) && int.TryParse(totalText, out var total))
        {
            return new BranchCounts(covered, total);
        }

        return new BranchCounts(0, 0);
    }
}
