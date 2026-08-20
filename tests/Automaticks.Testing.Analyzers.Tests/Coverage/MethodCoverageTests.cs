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

        method.AddLine(1, "50% (1/2");

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

        method.AddLine(1, "100%");

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

        method.AddLine(1, "50% (12)");

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

        method.AddLine(1, "50% (a/b)");

        await Assert.That(method.TotalBranches).IsEqualTo(0);
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
