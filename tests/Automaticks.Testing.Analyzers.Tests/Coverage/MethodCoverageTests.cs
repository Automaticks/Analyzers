using Automaticks.Testing.Coverage;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Testing.Analyzers.Tests.Coverage;

/// <summary>
///     Tests for MethodCoverage.
/// </summary>
public class MethodCoverageTests
{
    /// <summary>
    ///     Tests that AddLine_ConditionCoverageMissingCloseParenthesis_DoesNotAccumulateBranches.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AddLine_ConditionCoverageMissingCloseParenthesis_DoesNotAccumulateBranches(CancellationToken cancellationToken)
    {
        var method = new MethodCoverage("Bar");

        method.AddLine(1, 1, "50% (1/2");

        await Assert.That(method.TotalBranches).IsEqualTo(0);
        await Assert.That(method.CoveredBranches).IsEqualTo(0);
    }

    /// <summary>
    ///     Tests that AddLine_ConditionCoverageMissingOpenParenthesis_DoesNotAccumulateBranches.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AddLine_ConditionCoverageMissingOpenParenthesis_DoesNotAccumulateBranches(CancellationToken cancellationToken)
    {
        var method = new MethodCoverage("Bar");

        method.AddLine(1, 1, "100%");

        await Assert.That(method.TotalBranches).IsEqualTo(0);
        await Assert.That(method.CoveredBranches).IsEqualTo(0);
    }

    /// <summary>
    ///     Tests that AddLine_ConditionCoverageMissingSlash_DoesNotAccumulateBranches.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AddLine_ConditionCoverageMissingSlash_DoesNotAccumulateBranches(CancellationToken cancellationToken)
    {
        var method = new MethodCoverage("Bar");

        method.AddLine(1, 1, "50% (12)");

        await Assert.That(method.TotalBranches).IsEqualTo(0);
        await Assert.That(method.CoveredBranches).IsEqualTo(0);
    }

    /// <summary>
    ///     Tests that AddLine_ConditionCoverageWithNonNumericCounts_DoesNotAccumulateBranches.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AddLine_ConditionCoverageWithNonNumericCounts_DoesNotAccumulateBranches(CancellationToken cancellationToken)
    {
        var method = new MethodCoverage("Bar");

        method.AddLine(1, 1, "50% (a/b)");

        await Assert.That(method.TotalBranches).IsEqualTo(0);
        await Assert.That(method.CoveredBranches).IsEqualTo(0);
    }

    /// <summary>
    ///     Tests that an executed line counts once towards the method totals.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AddLine_ExecutedLine_CountsAsCovered(CancellationToken cancellationToken)
    {
        var method = new MethodCoverage("Bar");

        method.AddLine(7, 3, null);

        await Assert.That(method.TotalLines).IsEqualTo(1);
        await Assert.That(method.CoveredLines).IsEqualTo(1);
        await Assert.That(method.IsCovered).IsTrue();
    }

    /// <summary>
    ///     Tests that a second report covering the same line replaces the earlier miss.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AddLine_SameLineCoveredByLaterReport_KeepsBestResult(CancellationToken cancellationToken)
    {
        var method = new MethodCoverage("Bar");

        method.AddLine(7, 0, "0% (0/2)");
        method.AddLine(7, 4, "100% (2/2)");

        await Assert.That(method.TotalLines).IsEqualTo(1);
        await Assert.That(method.CoveredLines).IsEqualTo(1);
        await Assert.That(method.TotalBranches).IsEqualTo(2);
        await Assert.That(method.CoveredBranches).IsEqualTo(2);
    }

    /// <summary>
    ///     Tests that a later report with a worse result does not lower the totals.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AddLine_SameLineMissedByLaterReport_KeepsBestResult(CancellationToken cancellationToken)
    {
        var method = new MethodCoverage("Bar");

        method.AddLine(7, 4, "100% (2/2)");
        method.AddLine(7, 0, "0% (0/2)");

        await Assert.That(method.TotalLines).IsEqualTo(1);
        await Assert.That(method.CoveredLines).IsEqualTo(1);
        await Assert.That(method.TotalBranches).IsEqualTo(2);
        await Assert.That(method.CoveredBranches).IsEqualTo(2);
    }

    /// <summary>
    ///     Tests that a line reported without hits leaves the method uncovered.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AddLine_UnexecutedLine_LeavesMethodUncovered(CancellationToken cancellationToken)
    {
        var method = new MethodCoverage("Bar");

        method.AddLine(7, 0, "0% (0/2)");

        await Assert.That(method.TotalLines).IsEqualTo(1);
        await Assert.That(method.CoveredLines).IsEqualTo(0);
        await Assert.That(method.IsCovered).IsFalse();
        await Assert.That(method.TotalBranches).IsEqualTo(2);
        await Assert.That(method.CoveredBranches).IsEqualTo(0);
    }

    /// <summary>
    ///     Tests that Name_AfterConstruction_ReturnsConstructorValue.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Name_AfterConstruction_ReturnsConstructorValue(CancellationToken cancellationToken)
    {
        var method = new MethodCoverage("Bar");

        await Assert.That(method.Name).IsEqualTo("Bar");
    }
}
