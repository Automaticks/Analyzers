namespace Automaticks.Testing;

/// <summary>
///     Centralised registry of all diagnostic IDs emitted by the Testing Roslyn analyzers.
///     Use these constants when suppressing a diagnostic or referencing it from documentation.
/// </summary>
public static class DiagnosticIds
{
    /// <summary>
    ///     Diagnostic IDs for testing-convention rules enforced inside test projects.
    /// </summary>
    public static class Testing
    {
        /// <summary>
        ///     Diagnostic ID for <c>ATXTST010</c>: ambient dependency used where no injectable seam exists.
        /// </summary>
        public const string AmbientDependency = "ATXTST010";

        /// <summary>
        ///     Diagnostic ID for <c>ATXTST009</c>: <c>Debug.Assert</c> condition performs side effects.
        /// </summary>
        public const string AssertSideEffect = "ATXTST009";

        /// <summary>
        ///     Diagnostic ID for <c>ATXTST006</c>: bitmask test against a multi-bit mask hides individual bits.
        /// </summary>
        public const string CompositeBitmaskTest = "ATXTST006";

        /// <summary>
        ///     Diagnostic ID for <c>ATXTST013</c>: file line coverage is below the configured minimum.
        /// </summary>
        public const string FileLineCoverage = "ATXTST013";

        /// <summary>
        ///     Diagnostic ID for <c>ATXTST001</c>: mocking frameworks are not allowed; use hand-written stubs.
        /// </summary>
        public const string MockingFramework = "ATXTST001";

        /// <summary>
        ///     Diagnostic ID for <c>ATXTST004</c>: <c>Task.Delay</c> is forbidden in test projects.
        /// </summary>
        public const string TaskDelay = "ATXTST004";

        /// <summary>
        ///     Diagnostic ID for <c>ATXTST002</c>: test class name must match the class under test.
        /// </summary>
        public const string TestClassName = "ATXTST002";

        /// <summary>
        ///     Diagnostic ID for <c>ATXTST003</c>: test method name must follow the
        ///     <c>Method_Scenario_ExpectedResult</c> convention.
        /// </summary>
        public const string TestMethodNaming = "ATXTST003";

        /// <summary>
        ///     Diagnostic ID for <c>ATXTST012</c>: public member has no coverage in the supplied report.
        /// </summary>
        public const string UncoveredPublicMember = "ATXTST012";
    }
}
