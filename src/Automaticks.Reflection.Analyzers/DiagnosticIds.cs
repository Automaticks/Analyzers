namespace Automaticks.Reflection;

/// <summary>
///     Diagnostic IDs emitted by the Reflection analyzers.
/// </summary>
public static class DiagnosticIds
{
    /// <summary>
    ///     Diagnostic IDs for System.Reflection usage rules.
    /// </summary>
    public static class Reflection
    {
        /// <summary>
        ///     Diagnostic ID for <c>ATXRF030</c>: reflection is forbidden outside DI registration code.
        /// </summary>
        public const string ReflectionUsage = "ATXRF030";
    }
}
