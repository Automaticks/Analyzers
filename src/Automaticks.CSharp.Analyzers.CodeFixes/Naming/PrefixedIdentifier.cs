using System;

namespace Automaticks.CSharp.CodeFixes.Naming;

/// <summary>
///     Builds a prefixed identifier that keeps the original casing convention.
/// </summary>
public static class PrefixedIdentifier
{
    private static readonly string[] BooleanPrefixes;

    static PrefixedIdentifier()
    {
        BooleanPrefixes = ["allow", "can", "has", "is"];
    }

    /// <summary>
    ///     Prepends to , preserving any leading underscores and matching the original pascal or camel casing.
    /// </summary>
    /// <param name="name">The identifier to prefix.</param>
    /// <param name="prefix">The lowercase prefix to prepend.</param>
    /// <returns>The prefixed identifier.</returns>
    public static string Build(string name, string prefix)
    {
        var underscoreCount = 0;
        while (underscoreCount < name.Length && name[underscoreCount] == '_')
        {
            underscoreCount++;
        }

        var leading = name.Substring(0, underscoreCount);
        var original = name.Substring(underscoreCount);
        if (original.Length == 0)
        {
            return name;
        }

        var isPascalCase = char.IsUpper(original[0]);
        var core = StripBooleanPrefix(original);

        if (isPascalCase)
        {
            return leading + char.ToUpperInvariant(prefix[0]) + prefix.Substring(1) + core;
        }

        return leading + prefix + char.ToUpperInvariant(core[0]) + core.Substring(1);
    }

    /// <summary>
    ///     Removes a leading boolean prefix when the next character starts a new word, so
    ///     <c>IsKnown</c> loses its prefix but <c>Island</c> does not.
    /// </summary>
    private static string StripBooleanPrefix(string core)
    {
        foreach (var candidate in BooleanPrefixes)
        {
            if (core.Length <= candidate.Length
                || !core.StartsWith(candidate, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var remainder = core.Substring(candidate.Length);
            if (char.IsUpper(remainder[0]))
            {
                return remainder;
            }
        }

        return core;
    }
}
