namespace Automaticks.Testing.Coverage;

/// <summary>
///     The covered and total branch counts recorded for a single line.
/// </summary>
public readonly struct BranchCounts
{
    /// <summary>
    ///     Gets the number of branches recorded as taken.
    /// </summary>
    public int Covered { get; }

    /// <summary>
    ///     Gets the number of branches present.
    /// </summary>
    public int Total { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BranchCounts" /> struct.
    /// </summary>
    /// <param name="covered">The number of branches recorded as taken.</param>
    /// <param name="total">The number of branches present.</param>
    public BranchCounts(int covered, int total)
    {
        Covered = covered;
        Total = total;
    }
}
