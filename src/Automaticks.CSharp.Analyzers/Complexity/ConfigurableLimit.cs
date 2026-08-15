using Microsoft.CodeAnalysis.Diagnostics;

namespace Automaticks.CSharp.Complexity;

/// <summary>
///     Reads a positive integer limit from <c>.editorconfig</c>, falling back to a built-in default.
///     Complexity rules document their key so a repository can tune the threshold.
/// </summary>
public static class ConfigurableLimit
{
    /// <summary>
    ///     Reads the limit configured for the file under analysis.
    /// </summary>
    /// <param name="context">The syntax node analysis context.</param>
    /// <param name="key">The <c>.editorconfig</c> key holding the limit.</param>
    /// <param name="defaultValue">The limit to use when the key is absent or malformed.</param>
    /// <returns>The configured limit, or <paramref name="defaultValue" />.</returns>
    public static int Read(SyntaxNodeAnalysisContext context, string key, int defaultValue)
    {
        var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
        if (options.TryGetValue(key, out var raw) && int.TryParse(raw, out var parsed) && parsed > 0)
        {
            return parsed;
        }

        return defaultValue;
    }
}
