using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace Automaticks.CSharp.Complexity;

/// <summary>
///     Enforces a maximum line count per method-like member.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MethodLineLimitAnalyzer : DiagnosticAnalyzer
{
    private const string LimitKey = "automaticks.method_line_limit";
    private const int MaxLines = 50;

    /// <summary>
    ///     Diagnostic rule emitted when a method-like member exceeds <see cref="MaxLines" /> lines.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static MethodLineLimitAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.MethodLineLimit,
            "Methods must not exceed the maximum line limit",
            "Method '{0}' is {1} lines long, which exceeds the maximum of {2}. Split it into smaller, focused methods.",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "The method body exceeds the maximum number of lines. Extract cohesive blocks of logic into private helper methods with descriptive names. The limit is configurable via `.editorconfig` with key `automaticks.method_line_limit`.");
        Rule = rule;
    }

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

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeMember(SyntaxNodeAnalysisContext context)
    {
        var memberInfo = GetMemberInfo(context.Node);
        if (memberInfo is null)
        {
            return;
        }

        if (memberInfo.IsExpressionBodied)
        {
            return;
        }

        var block = GetBodyBlock(context.Node);
        if (block is not null && CanSkipLineLimit(block))
        {
            return;
        }

        var lineSpan = context.Node.GetLocation().GetLineSpan();
        var sourceText = context.Node.SyntaxTree.GetText();
        var lineCount = CountNonBlankLines(sourceText, lineSpan.StartLinePosition.Line, lineSpan.EndLinePosition.Line);

        var maxLines = ConfigurableLimit.Read(context, LimitKey, MaxLines);
        if (lineCount > maxLines)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, memberInfo.Location, memberInfo.Name, lineCount, maxLines));
        }
    }

    private bool CanSkipLineLimit(BlockSyntax body)
    {
        return HasSingleSwitchStatement(body) || HasObjectInitializerReturn(body);
    }

    private int CountNonBlankLines(SourceText sourceText, int startLine, int endLine)
    {
        var count = 0;
        for (var lineIndex = startLine; lineIndex <= endLine; lineIndex++)
        {
            if (!string.IsNullOrWhiteSpace(sourceText.Lines[lineIndex].ToString()))
            {
                count++;
            }
        }

        return count;
    }

    private MemberInfo? GetAccessorInfo(SyntaxNode node)
    {
        if (node is not AccessorDeclarationSyntax accessor || (accessor.Body is null && accessor.ExpressionBody is null))
        {
            return null;
        }

        return new MemberInfo(accessor.ExpressionBody != null, accessor.Keyword.GetLocation(), accessor.Keyword.Text);
    }

    private BlockSyntax? GetBodyBlock(SyntaxNode node)
    {
        return node switch
        {
            MethodDeclarationSyntax methodDeclaration => methodDeclaration.Body,
            AccessorDeclarationSyntax accessorDeclaration => accessorDeclaration.Body,
            LocalFunctionStatementSyntax localFunctionStatement => localFunctionStatement.Body,
            OperatorDeclarationSyntax operatorDeclaration => operatorDeclaration.Body,
            ConversionOperatorDeclarationSyntax conversionOperatorDeclaration => conversionOperatorDeclaration.Body,
            _ => null
        };
    }

    private MemberInfo? GetConversionOperatorInfo(SyntaxNode node)
    {
        if (node is not ConversionOperatorDeclarationSyntax conversionOperator ||
            (conversionOperator.Body is null && conversionOperator.ExpressionBody is null))
        {
            return null;
        }

        var name = $"{conversionOperator.ImplicitOrExplicitKeyword.Text} operator {conversionOperator.Type}";
        return new MemberInfo(conversionOperator.ExpressionBody != null, conversionOperator.ImplicitOrExplicitKeyword.GetLocation(), name);
    }

    private MemberInfo? GetLocalFunctionInfo(SyntaxNode node)
    {
        if (node is not LocalFunctionStatementSyntax localFunction || (localFunction.Body is null && localFunction.ExpressionBody is null))
        {
            return null;
        }

        return new MemberInfo(localFunction.ExpressionBody != null, localFunction.Identifier.GetLocation(), localFunction.Identifier.Text);
    }

    private MemberInfo? GetMemberInfo(SyntaxNode node)
    {
        var methodInfo = GetMethodInfo(node);
        if (methodInfo is not null)
        {
            return methodInfo;
        }

        var operatorInfo = GetOperatorInfo(node);
        if (operatorInfo is not null)
        {
            return operatorInfo;
        }

        var conversionOperatorInfo = GetConversionOperatorInfo(node);
        if (conversionOperatorInfo is not null)
        {
            return conversionOperatorInfo;
        }

        var accessorInfo = GetAccessorInfo(node);
        if (accessorInfo is not null)
        {
            return accessorInfo;
        }

        return GetLocalFunctionInfo(node);
    }

    private MemberInfo? GetMethodInfo(SyntaxNode node)
    {
        if (node is not MethodDeclarationSyntax method || (method.Body is null && method.ExpressionBody is null))
        {
            return null;
        }

        return new MemberInfo(method.ExpressionBody != null, method.Identifier.GetLocation(), method.Identifier.Text);
    }

    private MemberInfo? GetOperatorInfo(SyntaxNode node)
    {
        if (node is not OperatorDeclarationSyntax operatorDeclaration || (operatorDeclaration.Body is null && operatorDeclaration.ExpressionBody is null))
        {
            return null;
        }

        var name = $"operator {operatorDeclaration.OperatorToken.Text}";
        return new MemberInfo(operatorDeclaration.ExpressionBody != null, operatorDeclaration.OperatorToken.GetLocation(), name);
    }

    private bool HasObjectInitializerReturn(BlockSyntax body)
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

    private bool HasSingleSwitchStatement(BlockSyntax body)
    {
        return body.Statements.Count == 1 && body.Statements[0] is SwitchStatementSyntax;
    }

    /// <summary>
    ///     Holds the resolved name, location, and expression-bodied flag for a method-like member.
    /// </summary>
    private sealed class MemberInfo
    {
        /// <summary>
        ///     Gets a value indicating whether the member has an expression body.
        /// </summary>
        public bool IsExpressionBodied { get; }

        /// <summary>
        ///     Gets the location used to report a diagnostic for the member.
        /// </summary>
        public Location Location { get; }

        /// <summary>
        ///     Gets the display name of the member.
        /// </summary>
        public string Name { get; }

        public MemberInfo(bool isExpressionBodied, Location location, string name)
        {
            IsExpressionBodied = isExpressionBodied;
            Location = location;
            Name = name;
        }
    }
}
