using Microsoft.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Automaticks.CSharp.Documentation;

/// <summary>
///     Shared regex helpers for detecting XML documentation comment elements.
/// </summary>
public static class DocumentationCommentText
{
    private static readonly Regex InheritDocRegex;
    private static readonly Regex ReturnsOrInheritDocRegex;
    private static readonly Regex SummaryOrInheritDocRegex;

    static DocumentationCommentText()
    {
        InheritDocRegex = new("<\\s*inheritdoc\\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        ReturnsOrInheritDocRegex = new("<\\s*(returns|inheritdoc)\\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        SummaryOrInheritDocRegex = new("<\\s*(summary|inheritdoc)\\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
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
    ///     Determines whether <paramref name="node" /> has a leading <c>param</c> tag for the given name.
    /// </summary>
    /// <param name="node">The syntax node whose leading trivia is inspected.</param>
    /// <param name="parameterName">The parameter name to search for.</param>
    /// <returns><see langword="true" /> when a matching <c>param</c> tag is present.</returns>
    public static bool HasParamElement(SyntaxNode node, string parameterName)
    {
        var parameterPattern = $"<\\s*param\\b[^>]*\\bname\\s*=\\s*\"{Regex.Escape(parameterName)}\"";
        return Regex.IsMatch(node.GetLeadingTrivia().ToFullString(), parameterPattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
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
