namespace Automaticks.CommunityToolkit.Mvvm;

/// <summary>
///     Centralised registry of all diagnostic IDs emitted by the CommunityToolkit.Mvvm Roslyn analyzers.
/// </summary>
public static class DiagnosticIds
{
    /// <summary>
    ///     Diagnostic IDs for CommunityToolkit.Mvvm usage rules.
    /// </summary>
    public static class ModelViewViewModel
    {
        /// <summary>
        ///     Diagnostic ID for ATXMV001: command constructors must use method groups, not lambdas.
        /// </summary>
        public const string CommandLambda = "ATXMV001";
    }
}
