using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Automaticks.CSharp.Documentation;

/// <summary>
///     Shared regex helpers for detecting XML documentation comment elements.
/// </summary>
public static class DocumentationCommentText
{
    private static readonly Regex InheritDocRegex;
    private static readonly Regex ParamNameRegex;
    private static readonly Regex ReturnsOrInheritDocRegex;
    private static readonly Regex SummaryOrInheritDocRegex;

    static DocumentationCommentText()
    {
        InheritDocRegex = new("<\\s*inheritdoc\\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        ParamNameRegex = new("<\\s*param\\b[^>]*\\bname\\s*=\\s*\"([^\"]*)\"", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        ReturnsOrInheritDocRegex = new("<\\s*(returns|inheritdoc)\\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        SummaryOrInheritDocRegex = new("<\\s*(summary|inheritdoc)\\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
    }

    /// <summary>
    ///     Collects every documented parameter name from the leading trivia in a single pass.
    /// </summary>
    /// <param name="node">The syntax node whose leading trivia is inspected.</param>
    /// <returns>The set of documented parameter names, compared case-insensitively.</returns>
    public static HashSet<string> CollectParamNames(SyntaxNode node)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var text = node.GetLeadingTrivia().ToFullString();

        foreach (Match match in ParamNameRegex.Matches(text))
        {
            names.Add(match.Groups[1].Value);
        }

        return names;
    }

    /// <summary>
    ///     Determines whether <paramref name="node" /> has a leading <c>inheritdoc</c> tag.
    /// </summary>
    /// <param name="node">The syntax node whose leading trivia is inspected.</param>
    /// <returns><see langword="true" /> when an <c>inheritdoc</c> tag is present.</returns>
    public static bool HasInheritDoc(SyntaxNode node)
    {
        return InheritDocRegex.IsMatch(node.GetLeadingTrivia().ToFullString());
    }

    /// <summary>
    ///     Determines whether <paramref name="node" /> has a leading <c>returns</c> or <c>inheritdoc</c> tag.
    /// </summary>
    /// <param name="node">The syntax node whose leading trivia is inspected.</param>
    /// <returns><see langword="true" /> when either tag is present.</returns>
    public static bool HasReturnsOrInheritDoc(SyntaxNode node)
    {
        return ReturnsOrInheritDocRegex.IsMatch(node.GetLeadingTrivia().ToFullString());
    }

    /// <summary>
    ///     Determines whether <paramref name="node" /> has a leading <c>summary</c> or <c>inheritdoc</c> tag.
    /// </summary>
    /// <param name="node">The syntax node whose leading trivia is inspected.</param>
    /// <returns><see langword="true" /> when either tag is present.</returns>
    public static bool HasSummaryOrInheritDoc(SyntaxNode node)
    {
        return SummaryOrInheritDocRegex.IsMatch(node.GetLeadingTrivia().ToFullString());
    }
}
