using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;

namespace Automaticks.CSharp.Analyzers.Tests;

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
    ///     Finds the first diagnostic that has the specified identifier.
    /// </summary>
    /// <param name="diagnostics">The diagnostics to inspect.</param>
    /// <param name="diagnosticId">The diagnostic identifier to locate.</param>
    /// <returns>The first matching diagnostic when present; otherwise, <see langword="null" />.</returns>
    public static Diagnostic? FindById(ImmutableArray<Diagnostic> diagnostics, string diagnosticId)
    {
        foreach (var diagnostic in diagnostics)
        {
            if (diagnostic.Id == diagnosticId)
            {
                return diagnostic;
            }
        }

        return null;
    }

    /// <summary>
    ///     Gets the single diagnostic that has the specified identifier.
    /// </summary>
    /// <param name="diagnostics">The diagnostics to inspect.</param>
    /// <param name="diagnosticId">The diagnostic identifier to locate.</param>
    /// <returns>The single matching diagnostic.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no matching diagnostic exists or multiple matching diagnostics exist.</exception>
    public static Diagnostic GetSingleById(ImmutableArray<Diagnostic> diagnostics, string diagnosticId)
    {
        Diagnostic? match = null;
        foreach (var diagnostic in diagnostics)
        {
            if (diagnostic.Id != diagnosticId)
            {
                continue;
            }

            if (match is not null)
            {
                throw new InvalidOperationException($"Multiple diagnostics found for ID '{diagnosticId}'.");
            }

            match = diagnostic;
        }

        return match ?? throw new InvalidOperationException($"No diagnostic found for ID '{diagnosticId}'.");
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
