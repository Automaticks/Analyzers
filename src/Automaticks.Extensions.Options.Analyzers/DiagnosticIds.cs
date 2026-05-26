namespace Automaticks.Extensions.Options;

/// <summary>
///     Centralised registry of all diagnostic IDs emitted by the Extensions.Options Roslyn analyzers.
///     Use these constants when suppressing a diagnostic or referencing it from documentation.
/// </summary>
public static class DiagnosticIds
{
    /// <summary>
    ///     Diagnostic IDs for Microsoft.Extensions.Options usage rules.
    /// </summary>
    public static class Options
    {
        /// <summary>
        ///     Diagnostic ID for <c>ATXEO049</c>: <c>BindConfiguration</c> is forbidden — use <c>Configure&lt;T&gt;(configuration.GetRequiredSection(...))</c>.
        /// </summary>
        public const string BindConfiguration = "ATXEO049";
    }
}
