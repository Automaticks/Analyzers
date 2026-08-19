namespace Automaticks.Threading.Tasks;

/// <summary>
///     Diagnostic IDs emitted by the Threading.Tasks analyzers.
/// </summary>
public static class DiagnosticIds
{
    /// <summary>
    ///     Diagnostic IDs for System.Threading.Tasks usage rules.
    /// </summary>
    public static class ThreadingTasks
    {
        /// <summary>
        ///     Diagnostic ID for <c>ATXTA008</c>: task-returning methods must accept
        ///     <see cref="System.Threading.CancellationToken" /> as the last parameter.
        /// </summary>
        public const string AsyncCancellationToken = "ATXTA008";

        /// <summary>
        ///     Diagnostic ID for <c>ATXTA010</c>: unobserved <see cref="System.Threading.Tasks.Task" /> invocation.
        /// </summary>
        public const string UnobservedTask = "ATXTA010";

        /// <summary>
        ///     Diagnostic ID for <c>ATXTA011</c>: a declared
        ///     <see cref="System.Threading.CancellationToken" /> parameter is never used.
        /// </summary>
        public const string UnusedCancellationToken = "ATXTA011";
    }
}
