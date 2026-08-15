using Automaticks.CSharp.Formatting;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Formatting;

/// <summary>
///     Tests for MemberRank.
/// </summary>
public class MemberRankTests
{
    /// <summary>
    ///     Tests that CompareTo_DifferentAccessRank_OrdersByAccess.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CompareTo_DifferentAccessRank_OrdersByAccess(CancellationToken cancellationToken)
    {
        var publicRank = new MemberRank(8, 0, 0, 0, "Name");
        var privateRank = new MemberRank(8, 0, 1, 0, "Name");

        await Assert.That(publicRank < privateRank).IsTrue();
        await Assert.That(privateRank > publicRank).IsTrue();
    }

    /// <summary>
    ///     Tests that CompareTo_DifferentGroup_OrdersByGroup.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CompareTo_DifferentGroup_OrdersByGroup(CancellationToken cancellationToken)
    {
        var field = new MemberRank(3, 0, 0, 0, "Name");
        var method = new MemberRank(8, 0, 0, 0, "Name");

        await Assert.That(field < method).IsTrue();
    }

    /// <summary>
    ///     Tests that CompareTo_DifferentName_OrdersAlphabeticallyIgnoringCase.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CompareTo_DifferentName_OrdersAlphabeticallyIgnoringCase(CancellationToken cancellationToken)
    {
        var alpha = new MemberRank(8, 0, 0, 0, "alpha");
        var beta = new MemberRank(8, 0, 0, 0, "Beta");

        await Assert.That(alpha < beta).IsTrue();
    }

    /// <summary>
    ///     Tests that CompareTo_DifferentStaticRank_OrdersStaticFirst.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CompareTo_DifferentStaticRank_OrdersStaticFirst(CancellationToken cancellationToken)
    {
        var staticRank = new MemberRank(8, 0, 0, 0, "Name");
        var instanceRank = new MemberRank(8, 0, 0, 1, "Name");

        await Assert.That(staticRank <= instanceRank).IsTrue();
        await Assert.That(instanceRank >= staticRank).IsTrue();
    }

    /// <summary>
    ///     Tests that CompareTo_DifferentSubGroup_OrdersBySubGroup.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CompareTo_DifferentSubGroup_OrdersBySubGroup(CancellationToken cancellationToken)
    {
        var readOnlyField = new MemberRank(3, 0, 0, 0, "Name");
        var mutableField = new MemberRank(3, 1, 0, 0, "Name");

        await Assert.That(readOnlyField < mutableField).IsTrue();
    }

    /// <summary>
    ///     Tests that CompareTo_UnderscorePrefixedName_SortsBeforeLetters.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CompareTo_UnderscorePrefixedName_SortsBeforeLetters(CancellationToken cancellationToken)
    {
        var underscoreName = new MemberRank(3, 0, 0, 0, "_alpha");
        var letterName = new MemberRank(3, 0, 0, 0, "alpha");

        await Assert.That(underscoreName < letterName).IsTrue();
    }

    /// <summary>
    ///     Tests that Equals_DifferentRank_ReturnsFalse.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Equals_DifferentRank_ReturnsFalse(CancellationToken cancellationToken)
    {
        var field = new MemberRank(3, 0, 0, 0, "Name");
        var method = new MemberRank(8, 0, 0, 0, "Name");

        await Assert.That(field.Equals(method)).IsFalse();
        await Assert.That(field != method).IsTrue();
    }

    /// <summary>
    ///     Tests that Equals_NonMemberRankObject_ReturnsFalse.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Equals_NonMemberRankObject_ReturnsFalse(CancellationToken cancellationToken)
    {
        var rank = new MemberRank(8, 0, 0, 0, "Name");
        var other = "not a rank";

        await Assert.That(rank.Equals(other)).IsFalse();
    }

    /// <summary>
    ///     Tests that Equals_SameRank_ReturnsTrue.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Equals_SameRank_ReturnsTrue(CancellationToken cancellationToken)
    {
        var first = new MemberRank(8, 0, 0, 0, "Name");
        var second = new MemberRank(8, 0, 0, 0, "Name");

        await Assert.That(first.Equals(second)).IsTrue();
        await Assert.That(first == second).IsTrue();
    }

    /// <summary>
    ///     Tests that GetHashCode_DifferentRanks_ReturnsDifferentValue.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetHashCode_DifferentRanks_ReturnsDifferentValue(CancellationToken cancellationToken)
    {
        var first = new MemberRank(4, 1, 2, 0, "Name");
        var second = new MemberRank(9, 1, 2, 0, "Other");

        await Assert.That(first.GetHashCode()).IsNotEqualTo(second.GetHashCode());
    }

    /// <summary>
    ///     Tests that GetHashCode_EqualRanks_ReturnsSameValue.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetHashCode_EqualRanks_ReturnsSameValue(CancellationToken cancellationToken)
    {
        var first = new MemberRank(4, 1, 2, 0, "Name");
        var second = new MemberRank(4, 1, 2, 0, "Name");

        await Assert.That(first.GetHashCode()).IsEqualTo(second.GetHashCode());
    }

    /// <summary>
    ///     Tests that GroupName_FieldGroup_DistinguishesReadOnly.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GroupName_FieldGroup_DistinguishesReadOnly(CancellationToken cancellationToken)
    {
        var readOnlyField = new MemberRank(3, 0, 0, 0, "Name");
        var mutableField = new MemberRank(3, 1, 0, 0, "Name");

        await Assert.That(readOnlyField.GroupName).IsEqualTo("read-only field");
        await Assert.That(mutableField.GroupName).IsEqualTo("field");
    }

    /// <summary>
    ///     Tests that GroupName_ImplementationGroup_DistinguishesExplicit.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GroupName_ImplementationGroup_DistinguishesExplicit(CancellationToken cancellationToken)
    {
        var explicitImplementation = new MemberRank(7, 0, 0, 0, "Name");
        var implicitImplementation = new MemberRank(7, 1, 0, 0, "Name");

        await Assert.That(explicitImplementation.GroupName).IsEqualTo("explicit interface implementation");
        await Assert.That(implicitImplementation.GroupName).IsEqualTo("implicit implementation or override");
    }

    /// <summary>
    ///     Tests that GroupName_OtherGroup_ReturnsCanonicalName.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GroupName_OtherGroup_ReturnsCanonicalName(CancellationToken cancellationToken)
    {
        var property = new MemberRank(4, 0, 0, 0, "Name");
        var method = new MemberRank(8, 0, 0, 0, "Name");

        await Assert.That(property.GroupName).IsEqualTo("property");
        await Assert.That(method.GroupName).IsEqualTo("method");
    }

    /// <summary>
    ///     Tests that Minimum_ComparedToAnyRank_RanksFirst.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Minimum_ComparedToAnyRank_RanksFirst(CancellationToken cancellationToken)
    {
        var method = new MemberRank(8, 0, 0, 0, "Name");

        await Assert.That(MemberRank.Minimum <= method).IsTrue();
    }
}
