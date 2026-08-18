namespace Automaticks.Linq;

/// <summary>
///     Centralised registry of all diagnostic IDs emitted by the Linq Roslyn analyzers.
///     Use these constants when suppressing a diagnostic or referencing it from documentation.
/// </summary>
public static class DiagnosticIds
{
    /// <summary>
    ///     Diagnostic IDs for System.Linq usage rules.
    /// </summary>
    public static class Linq
    {
        /// <summary>
        ///     Diagnostic ID for <c>ATXLQ003</c>: a LINQ operator was called.
        /// </summary>
        public const string LinqOperatorInvocation = "ATXLQ003";

        /// <summary>
        ///     Diagnostic ID for <c>ATXLQ002</c>: LINQ is not allowed in production code.
        /// </summary>
        public const string LinqUsage = "ATXLQ002";
    }
}
