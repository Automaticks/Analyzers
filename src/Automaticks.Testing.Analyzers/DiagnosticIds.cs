namespace Automaticks.Testing;

/// <summary>
///     Diagnostic IDs emitted by the Testing analyzers.
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
        ///     Diagnostic ID for <c>ATXTST008</c>: conditional compilation branches on the DEBUG symbol.
        /// </summary>
        public const string DebugConditionalCompilation = "ATXTST008";

        /// <summary>
        ///     Diagnostic ID for <c>ATXTST005</c>: a boolean decision combines too many leaf conditions.
        /// </summary>
        public const string ExcessiveDecisionConditions = "ATXTST005";

        /// <summary>
        ///     Diagnostic ID for <c>ATXTST013</c>: file line coverage is below the configured minimum.
        /// </summary>
        public const string FileLineCoverage = "ATXTST013";

        /// <summary>
        ///     Diagnostic ID for <c>ATXTST015</c>: method branch coverage is below the configured minimum.
        /// </summary>
        public const string MethodBranchCoverage = "ATXTST015";

        /// <summary>
        ///     Diagnostic ID for <c>ATXTST011</c>: a test method contains no assertion.
        /// </summary>
        public const string MissingAssertion = "ATXTST011";

        /// <summary>
        ///     Diagnostic ID for <c>ATXTST001</c>: mocking frameworks are not allowed; use hand-written stubs.
        /// </summary>
        public const string MockingFramework = "ATXTST001";

        /// <summary>
        ///     Diagnostic ID for <c>ATXTST007</c>: a redundant default label on an exhaustive enum switch.
        /// </summary>
        public const string RedundantEnumSwitchDefault = "ATXTST007";

        /// <summary>
        ///     Diagnostic ID for <c>ATXTST014</c>: the real <c>TimeProvider.System</c> is used in a test project.
        /// </summary>
        public const string SystemTimeProviderInTest = "ATXTST014";

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

        /// <summary>
        ///     Diagnostic ID for <c>ATXTST016</c>: the supplied coverage report is unusable.
        /// </summary>
        public const string UnusableCoverageReport = "ATXTST016";
    }
}
