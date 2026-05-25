namespace Automaticks.Diagnostics.CodeAnalysis;

/// <summary>
///     Centralised registry of all diagnostic IDs emitted by the Diagnostics.CodeAnalysis Roslyn analyzers.
///     Use these constants when suppressing a diagnostic or referencing it from documentation.
/// </summary>
public static class DiagnosticIds
{
    /// <summary>
    ///     Diagnostic IDs for System.Diagnostics.CodeAnalysis usage rules.
    /// </summary>
    public static class DiagnosticsCodeAnalysis
    {
        /// <summary>Diagnostic ID for <c>ATXDC018</c>: <c>#pragma warning disable</c> suppression directives are forbidden.</summary>
        public const string SuppressionPragma = "ATXDC018";

        /// <summary>Diagnostic ID for <c>ATXDC019</c>: <c>// ReSharper disable</c> suppression comments are forbidden.</summary>
        public const string SuppressionReSharper = "ATXDC019";

        /// <summary>Diagnostic ID for <c>ATXDC056</c>: <c>[SuppressMessage]</c> attribute is forbidden — fix the root cause instead.</summary>
        public const string SuppressMessage = "ATXDC056";
    }
}
