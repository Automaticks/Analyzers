using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;

namespace Automaticks.CSharp.Naming;

/// <summary>
///     Reads a comma-separated list of type names from <c>.editorconfig</c>, falling back to a
///     built-in default.
/// </summary>
public static class ConfigurableTypeList
{
    /// <summary>
    ///     Reads the type names configured for the file under analysis.
    /// </summary>
    /// <param name="context">The syntax node analysis context.</param>
    /// <param name="key">The <c>.editorconfig</c> key holding the list.</param>
    /// <param name="defaultValue">The names to use when the key is absent or empty.</param>
    /// <returns>The configured names, or <paramref name="defaultValue" />.</returns>
    public static HashSet<string> Read(
        SyntaxNodeAnalysisContext context,
        string key,
        IReadOnlyList<string> defaultValue)
    {
        var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
        var names = new HashSet<string>(StringComparer.Ordinal);

        if (!options.TryGetValue(key, out var raw) || raw.Length == 0)
        {
            foreach (var name in defaultValue)
            {
                names.Add(name);
            }

            return names;
        }

        foreach (var entry in raw.Split(','))
        {
            var trimmed = entry.Trim();
            if (trimmed.Length > 0)
            {
                names.Add(trimmed);
            }
        }

        return names;
    }
}
