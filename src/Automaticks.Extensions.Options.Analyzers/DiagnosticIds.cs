namespace Automaticks.Extensions.Options;

/// <summary>
///     Diagnostic IDs emitted by the Extensions.Options analyzers.
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
