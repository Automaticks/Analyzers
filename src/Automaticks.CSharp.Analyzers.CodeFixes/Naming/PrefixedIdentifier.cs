namespace Automaticks.CSharp.CodeFixes.Naming;

/// <summary>
///     Builds a prefixed identifier that keeps the original casing convention.
/// </summary>
public static class PrefixedIdentifier
{
    /// <summary>
    ///     Prepends <paramref name="prefix" /> to <paramref name="name" />, preserving any leading
    ///     underscores and matching the original pascal or camel casing.
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
        var core = name.Substring(underscoreCount);
        if (core.Length == 0)
        {
            return name;
        }

        if (char.IsUpper(core[0]))
        {
            return leading + char.ToUpperInvariant(prefix[0]) + prefix.Substring(1) + core;
        }

        return leading + prefix + char.ToUpperInvariant(core[0]) + core.Substring(1);
    }
}
