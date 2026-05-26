using Microsoft.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Automaticks.CSharp;

internal static class DocumentationCommentText
{
    private static readonly Regex SummaryOrInheritDocRegex = new("<\\s*(summary|inheritdoc)\\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
    private static readonly Regex ReturnsOrInheritDocRegex = new("<\\s*(returns|inheritdoc)\\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
    private static readonly Regex InheritDocRegex = new("<\\s*inheritdoc\\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    public static bool HasInheritDoc(SyntaxNode node)
    {
        return InheritDocRegex.IsMatch(node.GetLeadingTrivia().ToFullString());
    }

    public static bool HasParamElement(SyntaxNode node, string parameterName)
    {
        var parameterPattern = $"<\\s*param\\b[^>]*\\bname\\s*=\\s*\"{Regex.Escape(parameterName)}\"";
        return Regex.IsMatch(node.GetLeadingTrivia().ToFullString(), parameterPattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
    }

    public static bool HasReturnsOrInheritDoc(SyntaxNode node)
    {
        return ReturnsOrInheritDocRegex.IsMatch(node.GetLeadingTrivia().ToFullString());
    }

    public static bool HasSummaryOrInheritDoc(SyntaxNode node)
    {
        return SummaryOrInheritDocRegex.IsMatch(node.GetLeadingTrivia().ToFullString());
    }
}
