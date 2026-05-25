namespace Automaticks.EntityFrameworkCore;

/// <summary>
///     Centralised registry of all suppression IDs emitted by the EntityFrameworkCore Roslyn analyzers.
///     Use these constants when referencing a suppression from documentation or tests.
/// </summary>
public static class SuppressionIds
{
    /// <summary>
    ///     Suppression IDs for Entity Framework Core-specific rules.
    /// </summary>
    public static class EFCore
    {
        /// <summary>Diagnostic ID for <c>ATXLQ002</c>: LINQ is not allowed in production code.</summary>
        public const string LinqUsage = "ATXLQ002";
    }
}
