using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Automaticks.CSharp.CodeFixes.LanguageFeatures;

/// <summary>
///     Builds the block body that replaces an expression body.
/// </summary>
public static class ExpressionBodyBlockBuilder
{
    private const string IndentStep = "    ";

    /// <summary>
    ///     Builds a get accessor wrapping the expression behind a return.
    /// </summary>
    /// <param name="expressionBody">The expression body being replaced.</param>
    /// <param name="baseIndent">The indentation of the declaration.</param>
    /// <param name="lineBreak">The line break used by the document.</param>
    /// <returns>The replacement text.</returns>
    public static string BuildGetAccessorBlock(ArrowExpressionClauseSyntax expressionBody, string baseIndent, string lineBreak)
    {
        var builder = new StringBuilder();
        var innerIndent = baseIndent + IndentStep;
        builder.Append(lineBreak).Append(baseIndent).Append('{');
        builder.Append(lineBreak).Append(innerIndent).Append("get");
        builder.Append(lineBreak).Append(innerIndent).Append('{');
        builder.Append(lineBreak).Append(innerIndent).Append(IndentStep).Append("return ");
        builder.Append(expressionBody.Expression.ToString()).Append(';');
        builder.Append(lineBreak).Append(innerIndent).Append('}');
        builder.Append(lineBreak).Append(baseIndent).Append('}');
        return builder.ToString();
    }

    /// <summary>
    ///     Builds a block holding the expression behind a return.
    /// </summary>
    /// <param name="expressionBody">The expression body being replaced.</param>
    /// <param name="baseIndent">The indentation of the declaration.</param>
    /// <param name="lineBreak">The line break used by the document.</param>
    /// <returns>The replacement text.</returns>
    public static string BuildReturnBlock(ArrowExpressionClauseSyntax expressionBody, string baseIndent, string lineBreak)
    {
        return BuildBlock(expressionBody, baseIndent, lineBreak, "return ");
    }

    /// <summary>
    ///     Builds a block holding the expression as a bare statement.
    /// </summary>
    /// <param name="expressionBody">The expression body being replaced.</param>
    /// <param name="baseIndent">The indentation of the declaration.</param>
    /// <param name="lineBreak">The line break used by the document.</param>
    /// <returns>The replacement text.</returns>
    public static string BuildStatementBlock(ArrowExpressionClauseSyntax expressionBody, string baseIndent, string lineBreak)
    {
        return BuildBlock(expressionBody, baseIndent, lineBreak, string.Empty);
    }

    /// <summary>
    ///     Reads the leading whitespace of the line holding a position.
    /// </summary>
    /// <param name="text">The document text.</param>
    /// <param name="position">A position on the line to measure.</param>
    /// <returns>The leading whitespace.</returns>
    public static string GetIndentation(SourceText text, int position)
    {
        var line = text.Lines.GetLineFromPosition(position);
        var builder = new StringBuilder();
        for (var offset = line.Start; offset < line.End; offset++)
        {
            var character = text[offset];
            if (character != ' ' && character != '\t')
            {
                break;
            }

            builder.Append(character);
        }

        return builder.ToString();
    }

    /// <summary>
    ///     Reads the line break the document uses, falling back to a line feed.
    /// </summary>
    /// <param name="text">The document text.</param>
    /// <returns>The line break.</returns>
    public static string GetLineBreak(SourceText text)
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

    /// <summary>
    ///     Tells whether a symbol returns nothing a caller can use.
    /// </summary>
    /// <param name="method">The method being converted.</param>
    /// <param name="compilation">The compilation supplying the task types.</param>
    /// <returns><see langword="true" /> when no return statement is needed.</returns>
    public static bool HasVoidLikeReturn(IMethodSymbol method, Compilation compilation)
    {
        if (method.ReturnsVoid)
        {
            return true;
        }

        if (!method.IsAsync)
        {
            return false;
        }

        var taskType = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
        var valueTaskType = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask");
        return SymbolEqualityComparer.Default.Equals(method.ReturnType, taskType)
            || SymbolEqualityComparer.Default.Equals(method.ReturnType, valueTaskType);
    }

    private static string BuildBlock(ArrowExpressionClauseSyntax expressionBody, string baseIndent, string lineBreak, string prefix)
    {
        var builder = new StringBuilder();
        builder.Append(lineBreak).Append(baseIndent).Append('{');
        builder.Append(lineBreak).Append(baseIndent).Append(IndentStep).Append(prefix);
        builder.Append(expressionBody.Expression.ToString()).Append(';');
        builder.Append(lineBreak).Append(baseIndent).Append('}');
        return builder.ToString();
    }
}
