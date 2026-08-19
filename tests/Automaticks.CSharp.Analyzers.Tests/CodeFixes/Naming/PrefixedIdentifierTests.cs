using Automaticks.CSharp.CodeFixes.Naming;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes.Naming;

/// <summary>
///     Tests for PrefixedIdentifier.
/// </summary>
public class PrefixedIdentifierTests
{
    /// <summary>
    ///     Tests that Build_CamelCaseNameWithExistingPrefix_ReplacesPrefix.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Build_CamelCaseNameWithExistingPrefix_ReplacesPrefix(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = PrefixedIdentifier.Build("isReady", "can");

        await Assert.That(result).IsEqualTo("canReady");    }

    /// <summary>
    ///     Tests that Build_NameEqualToPrefix_KeepsWholeName.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Build_NameEqualToPrefix_KeepsWholeName(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = PrefixedIdentifier.Build("Is", "can");

        await Assert.That(result).IsEqualTo("CanIs");
    }

    /// <summary>
    ///     Tests that Build_NameIsOnlyUnderscores_ReturnsOriginal.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Build_NameIsOnlyUnderscores_ReturnsOriginal(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = PrefixedIdentifier.Build("__", "is");

        await Assert.That(result).IsEqualTo("__");
    }

    /// <summary>
    ///     Tests that Build_NameWithoutExistingPrefix_PrependsPrefix.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Build_NameWithoutExistingPrefix_PrependsPrefix(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = PrefixedIdentifier.Build("Validate", "can");

        await Assert.That(result).IsEqualTo("CanValidate");
    }

    /// <summary>
    ///     Tests that Build_PascalCaseNameWithExistingPrefix_ReplacesPrefix.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Build_PascalCaseNameWithExistingPrefix_ReplacesPrefix(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = PrefixedIdentifier.Build("IsKnown", "can");

        await Assert.That(result).IsEqualTo("CanKnown");
    }

    /// <summary>
    ///     Tests that Build_PrefixFollowedByLowercase_KeepsWholeName.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Build_PrefixFollowedByLowercase_KeepsWholeName(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = PrefixedIdentifier.Build("Island", "can");

        await Assert.That(result).IsEqualTo("CanIsland");
    }

    /// <summary>
    ///     Tests that Build_UnderscoreFieldWithExistingPrefix_KeepsUnderscore.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Build_UnderscoreFieldWithExistingPrefix_KeepsUnderscore(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = PrefixedIdentifier.Build("_hasBuiltBatches", "is");

        await Assert.That(result).IsEqualTo("_isBuiltBatches");
    }
}
