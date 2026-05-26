using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;

namespace Automaticks.EntityFrameworkCore.Analyzers.Tests;

/// <summary>
///     Provides helper methods for asserting diagnostics without relying on LINQ.
/// </summary>
public static class DiagnosticCollectionAssertions
{

    /// <summary>
    ///     Counts the diagnostics that have the specified identifier.
    /// </summary>
    /// <param name="diagnostics">The diagnostics to inspect.</param>
    /// <param name="diagnosticId">The diagnostic identifier to count.</param>
    /// <returns>The number of diagnostics with the specified identifier.</returns>
    public static int CountId(ImmutableArray<Diagnostic> diagnostics, string diagnosticId)
    {
        var count = 0;
        foreach (var diagnostic in diagnostics)
        {
            if (diagnostic.Id == diagnosticId)
            {
                count += 1;
            }
        }

        return count;
    }

    /// <summary>
    ///     Determines whether the diagnostic collection contains a diagnostic whose identifier matches any expected identifier.
    /// </summary>
    /// <param name="diagnostics">The diagnostics to inspect.</param>
    /// <param name="diagnosticIds">The diagnostic identifiers to locate.</param>
    /// <returns><see langword="true" /> when a matching diagnostic exists; otherwise, <see langword="false" />.</returns>
    public static bool HasAnyId(ImmutableArray<Diagnostic> diagnostics, IReadOnlyList<string> diagnosticIds)
    {
        foreach (var diagnostic in diagnostics)
        {
            foreach (var diagnosticId in diagnosticIds)
            {
                if (diagnostic.Id == diagnosticId)
                {
                    return true;
                }
            }
        }

        return false;
    }
    /// <summary>
    ///     Determines whether the diagnostic collection contains a diagnostic with the specified identifier.
    /// </summary>
    /// <param name="diagnostics">The diagnostics to inspect.</param>
    /// <param name="diagnosticId">The diagnostic identifier to locate.</param>
    /// <returns><see langword="true" /> when a matching diagnostic exists; otherwise, <see langword="false" />.</returns>
    public static bool HasId(ImmutableArray<Diagnostic> diagnostics, string diagnosticId)
    {
        foreach (var diagnostic in diagnostics)
        {
            if (diagnostic.Id == diagnosticId)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Determines whether the diagnostic collection contains a diagnostic with the specified identifier and message content.
    /// </summary>
    /// <param name="diagnostics">The diagnostics to inspect.</param>
    /// <param name="diagnosticId">The diagnostic identifier to locate.</param>
    /// <param name="messageSubstring">The message substring to locate.</param>
    /// <returns><see langword="true" /> when a matching diagnostic exists; otherwise, <see langword="false" />.</returns>
    public static bool HasIdWithMessageSubstring(ImmutableArray<Diagnostic> diagnostics, string diagnosticId, string messageSubstring)
    {
        foreach (var diagnostic in diagnostics)
        {
            if (diagnostic.Id == diagnosticId
                && diagnostic.GetMessage(CultureInfo.InvariantCulture).Contains(messageSubstring, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
