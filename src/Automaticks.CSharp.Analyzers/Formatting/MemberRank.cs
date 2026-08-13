using System;

namespace Automaticks.CSharp.Formatting;

/// <summary>
///     A composite rank that captures the canonical position of a type member.
/// </summary>
public readonly struct MemberRank : IComparable<MemberRank>
{
    /// <summary>
    ///     The minimum possible rank, used as the initial comparison baseline.
    /// </summary>
    public static readonly MemberRank Minimum;
    private static readonly string[] GroupNames;
    private readonly int _accessRank;
    private readonly int _groupIndex;
    private readonly string _name;
    private readonly int _staticRank;
    private readonly int _subGroupIndex;

    /// <summary>
    ///     Human-readable name of this member's canonical group, used in diagnostic messages.
    /// </summary>
    public string GroupName
    {
        get
        {
            if (_groupIndex == 3)
            {
                return _subGroupIndex == 0 ? "read-only field" : "field";
            }

            if (_groupIndex != 7)
            {
                return GroupNames[_groupIndex];
            }

            return _subGroupIndex == 0 ? "explicit interface implementation" : "implicit implementation or override";
        }
    }

    static MemberRank()
    {
        var groupNames = new[]
        {
            "abstract member",
            "event",
            "constant",
            "field",
            "property",
            "indexer",
            "constructor",
            "concrete implementation",
            "method",
            "nested type"
        };
        GroupNames = groupNames;

        var minimum = new MemberRank(0, 0, 0, 0, string.Empty);
        Minimum = minimum;
    }

    /// <summary>
    ///     Initializes a new rank from its canonical ordering components.
    /// </summary>
    /// <param name="groupIndex">The canonical group index (abstract, event, field, etc.).</param>
    /// <param name="subGroupIndex">The sub-group index within the group.</param>
    /// <param name="accessRank">The access-level rank (public before protected before private).</param>
    /// <param name="staticRank">The static-vs-instance rank (static before instance).</param>
    /// <param name="name">The member name used for the final alphabetical tie-break.</param>
    public MemberRank(int groupIndex, int subGroupIndex, int accessRank, int staticRank, string name)
    {
        _groupIndex = groupIndex;
        _subGroupIndex = subGroupIndex;
        _accessRank = accessRank;
        _staticRank = staticRank;
        _name = name;
    }

    /// <inheritdoc />
    public int CompareTo(MemberRank other)
    {
        var groupComparison = _groupIndex.CompareTo(other._groupIndex);
        if (groupComparison != 0)
        {
            return groupComparison;
        }

        var subGroupComparison = _subGroupIndex.CompareTo(other._subGroupIndex);
        if (subGroupComparison != 0)
        {
            return subGroupComparison;
        }

        var accessComparison = _accessRank.CompareTo(other._accessRank);
        if (accessComparison != 0)
        {
            return accessComparison;
        }

        var staticComparison = _staticRank.CompareTo(other._staticRank);
        if (staticComparison != 0)
        {
            return staticComparison;
        }

        var nameComparison = string.Compare(
            NormalizeNameForComparison(_name),
            NormalizeNameForComparison(other._name),
            StringComparison.OrdinalIgnoreCase);
        return nameComparison;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is MemberRank other && CompareTo(other) == 0;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = 17;
        hash = (hash * 31) + _groupIndex;
        hash = (hash * 31) + _subGroupIndex;
        hash = (hash * 31) + _accessRank;
        hash = (hash * 31) + _staticRank;
        hash = (hash * 31) + StringComparer.OrdinalIgnoreCase.GetHashCode(_name);
        return hash;
    }

    /// <summary>
    ///     Returns <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> have different rank.
    /// </summary>
    public static bool operator !=(MemberRank left, MemberRank right)
    {
        return left.CompareTo(right) != 0;
    }

    /// <summary>
    ///     Returns <see langword="true" /> if <paramref name="left" /> ranks strictly before <paramref name="right" />.
    /// </summary>
    public static bool operator <(MemberRank left, MemberRank right)
    {
        return left.CompareTo(right) < 0;
    }

    /// <summary>
    ///     Returns <see langword="true" /> if <paramref name="left" /> ranks before or equal to <paramref name="right" />.
    /// </summary>
    public static bool operator <=(MemberRank left, MemberRank right)
    {
        return left.CompareTo(right) <= 0;
    }

    /// <summary>
    ///     Returns <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> have equal rank.
    /// </summary>
    public static bool operator ==(MemberRank left, MemberRank right)
    {
        return left.CompareTo(right) == 0;
    }

    /// <summary>
    ///     Returns <see langword="true" /> if <paramref name="left" /> ranks strictly after <paramref name="right" />.
    /// </summary>
    public static bool operator >(MemberRank left, MemberRank right)
    {
        return left.CompareTo(right) > 0;
    }

    /// <summary>
    ///     Returns <see langword="true" /> if <paramref name="left" /> ranks after or equal to <paramref name="right" />.
    /// </summary>
    public static bool operator >=(MemberRank left, MemberRank right)
    {
        return left.CompareTo(right) >= 0;
    }

    private static string NormalizeNameForComparison(string name) => name.Replace('_', '\x01');
}
