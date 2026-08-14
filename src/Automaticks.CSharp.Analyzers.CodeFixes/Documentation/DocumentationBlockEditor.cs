using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Automaticks.CSharp.CodeFixes.Documentation;

/// <summary>
///     Appends a line to a member's XML documentation block, creating the block when the member
///     has none.
/// </summary>
public static class DocumentationBlockEditor
{
    /// <summary>
    ///     Inserts a documentation line after the member's existing documentation block.
    /// </summary>
    /// <param name="text">The document text to edit.</param>
    /// <param name="member">The member whose documentation is extended.</param>
    /// <param name="element">The documentation line to insert, without indentation.</param>
    /// <returns>The updated document text.</returns>
    public static SourceText InsertLine(SourceText text, MemberDeclarationSyntax member, string element)
    {
        var memberLine = text.Lines.GetLineFromPosition(member.SpanStart);
        var indentSpan = TextSpan.FromBounds(memberLine.Start, member.SpanStart);
        var indent = text.ToString(indentSpan);
        var lineBreak = GetLineBreak(text);
        var insertPosition = GetInsertPosition(text, member, memberLine.Start);
        var builder = new StringBuilder();
        builder.Append(indent).Append(element).Append(lineBreak);
        var insertionPoint = new TextSpan(insertPosition, 0);
        return text.Replace(insertionPoint, builder.ToString());
    }

    private static int GetInsertPosition(SourceText text, MemberDeclarationSyntax member, int fallback)
    {
        foreach (var trivia in member.GetLeadingTrivia())
        {
            if (!trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) &&
                !trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
            {
                continue;
            }

            var lastLine = text.Lines.GetLineFromPosition(trivia.FullSpan.End - 1);
            return lastLine.EndIncludingLineBreak;
        }

        return fallback;
    }

    private static string GetLineBreak(SourceText text)
    {
        foreach (var line in text.Lines)
        {
            if (line.EndIncludingLineBreak > line.End)
            {
                var breakSpan = TextSpan.FromBounds(line.End, line.EndIncludingLineBreak);
                return text.ToString(breakSpan);
            }
        }

        return "\n";
    }
}
