using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace Automaticks.CSharp;

/// <summary>
///     Enforces a maximum line count per method-like member. Any method, operator, property or event
///     accessor, or local function whose non-blank line count exceeds <see cref="MaxLines" /> is flagged.
///     Blank and whitespace-only lines are excluded from the count.
///     Expression-bodied members are always counted as a single line.
///     Members without a body (abstract, extern, interface declarations) are ignored.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MethodLineLimitAnalyzer : DiagnosticAnalyzer
{
    /// <summary>Diagnostic rule emitted when a method-like member exceeds <see cref="MaxLines" /> lines.</summary>
    public static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.CSharp.MethodLineLimit,
        "Methods must not exceed the maximum line limit",
        $"Method '{{0}}' is {{1}} lines long, which exceeds the maximum of {MaxLines}. Split it into smaller, focused methods.",
        "CSharp",
        DiagnosticSeverity.Error,
        true,
        "The method body exceeds the maximum number of lines. Extract cohesive blocks of logic into private helper methods with descriptive names. The limit is configurable via `.editorconfig` with key `automaticks.method_line_limit`.");

    private const int MaxLines = 50;

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(
            AnalyzeMember,
            SyntaxKind.MethodDeclaration,
            SyntaxKind.OperatorDeclaration,
            SyntaxKind.ConversionOperatorDeclaration,
            SyntaxKind.GetAccessorDeclaration,
            SyntaxKind.SetAccessorDeclaration,
            SyntaxKind.InitAccessorDeclaration,
            SyntaxKind.AddAccessorDeclaration,
            SyntaxKind.RemoveAccessorDeclaration,
            SyntaxKind.LocalFunctionStatement);
    }

    private static void AnalyzeMember(SyntaxNodeAnalysisContext context)
    {
        if (!TryGetMemberInfo(context.Node, out var name, out var location, out var isExpressionBodied))
        {
            return;
        }

        if (isExpressionBodied)
        {
            return;
        }

        var block = GetBodyBlock(context.Node);
        if (block is not null && IsExemptMethod(block))
        {
            return;
        }

        var lineSpan = context.Node.GetLocation().GetLineSpan();
        var sourceText = context.Node.SyntaxTree.GetText();
        var lineCount = CountNonBlankLines(sourceText, lineSpan.StartLinePosition.Line, lineSpan.EndLinePosition.Line);

        if (lineCount > MaxLines)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, location, name, lineCount));
        }
    }

    private static int CountNonBlankLines(SourceText sourceText, int startLine, int endLine)
    {
        var count = 0;
        for (var i = startLine; i <= endLine; i++)
        {
            if (!string.IsNullOrWhiteSpace(sourceText.Lines[i].ToString()))
            {
                count++;
            }
        }

        return count;
    }

    private static BlockSyntax? GetBodyBlock(SyntaxNode node)
    {
        return node switch
        {
            MethodDeclarationSyntax m => m.Body,
            AccessorDeclarationSyntax a => a.Body,
            LocalFunctionStatementSyntax l => l.Body,
            OperatorDeclarationSyntax o => o.Body,
            ConversionOperatorDeclarationSyntax c => c.Body,
            _ => null
        };
    }

    private static bool IsExemptMethod(BlockSyntax body)
    {
        return IsSingleSwitchMethod(body) || IsObjectInitializerReturnMethod(body);
    }

    private static bool IsObjectInitializerReturnMethod(BlockSyntax body)
    {
        if (body.Statements.Count is 0 or > 2)
        {
            return false;
        }

        if (body.Statements[body.Statements.Count - 1] is not ReturnStatementSyntax returnStatement)
        {
            return false;
        }

        return returnStatement.Expression is ObjectCreationExpressionSyntax { Initializer: not null }
            or ImplicitObjectCreationExpressionSyntax { Initializer: not null };
    }

    private static bool IsSingleSwitchMethod(BlockSyntax body)
    {
        return body.Statements.Count == 1 && body.Statements[0] is SwitchStatementSyntax;
    }

    private static bool TryGetAccessorInfo(
        SyntaxNode node,
        out string name,
        out Location location,
        out bool isExpressionBodied)
    {
        if (node is not AccessorDeclarationSyntax a || (a.Body is null && a.ExpressionBody is null))
        {
            name = null!;
            location = null!;
            isExpressionBodied = false;
            return false;
        }

        name = a.Keyword.Text;
        location = a.Keyword.GetLocation();
        isExpressionBodied = a.ExpressionBody != null;
        return true;
    }

    private static bool TryGetConversionOperatorInfo(
        SyntaxNode node,
        out string name,
        out Location location,
        out bool isExpressionBodied)
    {
        if (node is not ConversionOperatorDeclarationSyntax c || (c.Body is null && c.ExpressionBody is null))
        {
            name = null!;
            location = null!;
            isExpressionBodied = false;
            return false;
        }

        name = $"{c.ImplicitOrExplicitKeyword.Text} operator {c.Type}";
        location = c.ImplicitOrExplicitKeyword.GetLocation();
        isExpressionBodied = c.ExpressionBody != null;
        return true;
    }

    private static bool TryGetLocalFunctionInfo(
        SyntaxNode node,
        out string name,
        out Location location,
        out bool isExpressionBodied)
    {
        if (node is not LocalFunctionStatementSyntax l || (l.Body is null && l.ExpressionBody is null))
        {
            name = null!;
            location = null!;
            isExpressionBodied = false;
            return false;
        }

        name = l.Identifier.Text;
        location = l.Identifier.GetLocation();
        isExpressionBodied = l.ExpressionBody != null;
        return true;
    }

    private static bool TryGetMemberInfo(
        SyntaxNode node, out string name, out Location location, out bool isExpressionBodied)
    {
        if (TryGetMethodInfo(node, out name, out location, out isExpressionBodied))
        {
            return true;
        }

        if (TryGetOperatorInfo(node, out name, out location, out isExpressionBodied))
        {
            return true;
        }

        if (TryGetConversionOperatorInfo(node, out name, out location, out isExpressionBodied))
        {
            return true;
        }

        if (TryGetAccessorInfo(node, out name, out location, out isExpressionBodied))
        {
            return true;
        }

        if (TryGetLocalFunctionInfo(node, out name, out location, out isExpressionBodied))
        {
            return true;
        }

        name = null!;
        location = null!;
        isExpressionBodied = false;
        return false;
    }

    private static bool TryGetMethodInfo(
        SyntaxNode node,
        out string name,
        out Location location,
        out bool isExpressionBodied)
    {
        if (node is not MethodDeclarationSyntax m || (m.Body is null && m.ExpressionBody is null))
        {
            name = null!;
            location = null!;
            isExpressionBodied = false;
            return false;
        }

        name = m.Identifier.Text;
        location = m.Identifier.GetLocation();
        isExpressionBodied = m.ExpressionBody != null;
        return true;
    }

    private static bool TryGetOperatorInfo(
        SyntaxNode node,
        out string name,
        out Location location,
        out bool isExpressionBodied)
    {
        if (node is not OperatorDeclarationSyntax o || (o.Body is null && o.ExpressionBody is null))
        {
            name = null!;
            location = null!;
            isExpressionBodied = false;
            return false;
        }

        name = $"operator {o.OperatorToken.Text}";
        location = o.OperatorToken.GetLocation();
        isExpressionBodied = o.ExpressionBody != null;
        return true;
    }
}
