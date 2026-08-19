using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.Formatting;

/// <summary>
///     Enforces that object, collection, array, and with-expression initializers always use the multi-line format where the opening brace, every member, a...
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ObjectInitializerCodeStyleAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when an initializer block contains no members.
    /// </summary>
    public static readonly DiagnosticDescriptor EmptyBracesRule;

    /// <summary>
    ///     The diagnostic rule reported when an initializer member does not appear on its own line,
    ///     or when the opening / closing brace is not on its own line.
    /// </summary>
    public static readonly DiagnosticDescriptor FormatRule;

    static ObjectInitializerCodeStyleAnalyzer()
    {
        EmptyBracesRule = new(
            DiagnosticIds.CSharp.ObjectInitializerEmptyBraces,
            "Empty initializer braces are forbidden",
            "Empty initializer braces are forbidden; remove the initializer block. A code fix is available (dotnet format analyzers --diagnostics ATXCS060).",
            "Style",
            DiagnosticSeverity.Error,
            true,
            "Empty initializer braces add noise without providing value. Remove them and rely on the constructor or default values instead.");
        FormatRule = new(
            DiagnosticIds.CSharp.ObjectInitializerFormat,
            "Initializer must use one member per line",
            "Each initializer member and the surrounding braces must be on their own line. A code fix is available (dotnet format analyzers --diagnostics ATXCS059).",
            "Style",
            DiagnosticSeverity.Error,
            true,
            "Format the object or collection initializer so that every member assignment and both braces are on their own lines. Example: `new Foo { X = 1, Y = 2 }` must become `new Foo\n{\n    X = 1,\n    Y = 2\n}`. Single-element initializers are not exempt.");
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(
            Analyze,
            SyntaxKind.ObjectInitializerExpression,
            SyntaxKind.CollectionInitializerExpression,
            SyntaxKind.ArrayInitializerExpression,
            SyntaxKind.WithInitializerExpression);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [FormatRule, EmptyBracesRule];

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InitializerExpressionSyntax initializer)
        {
            return;
        }

        if (initializer.Expressions.Count == 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(EmptyBracesRule, initializer.OpenBraceToken.GetLocation()));
            return;
        }

        CheckOpenBrace(context, initializer);
        CheckMembers(context, initializer);
        CheckCloseBrace(context, initializer);
    }

    private void CheckCloseBrace(SyntaxNodeAnalysisContext context, InitializerExpressionSyntax initializer)
    {
        var closeBraceLine = GetLine(initializer.CloseBraceToken);
        var lastExpression = initializer.Expressions[initializer.Expressions.Count - 1];

        if (GetLine(lastExpression.GetLastToken()) == closeBraceLine)
        {
            context.ReportDiagnostic(Diagnostic.Create(FormatRule, initializer.CloseBraceToken.GetLocation()));
        }
    }

    private void CheckMembers(SyntaxNodeAnalysisContext context, InitializerExpressionSyntax initializer)
    {
        var openBraceLine = GetLine(initializer.OpenBraceToken);
        var closeBraceLine = GetLine(initializer.CloseBraceToken);
        var count = initializer.Expressions.Count;
        var startLines = new int[count];
        var endLines = new int[count];

        for (var index = 0; index < count; index++)
        {
            startLines[index] = GetLine(initializer.Expressions[index].GetFirstToken());
            endLines[index] = GetLine(initializer.Expressions[index].GetLastToken());
        }

        for (var index = 0; index < count; index++)
        {
            var sharesWithOpenBrace = startLines[index] == openBraceLine;
            var sharesWithCloseBrace = endLines[index] == closeBraceLine;
            var sharesWithPrevious = index > 0 && startLines[index] == endLines[index - 1];
            var sharesWithNext = index < count - 1 && endLines[index] == startLines[index + 1];

            if (sharesWithOpenBrace || sharesWithCloseBrace || sharesWithPrevious || sharesWithNext)
            {
                context.ReportDiagnostic(Diagnostic.Create(FormatRule, initializer.Expressions[index].GetLocation()));
            }
        }
    }

    private void CheckOpenBrace(SyntaxNodeAnalysisContext context, InitializerExpressionSyntax initializer)
    {
        var openBraceLine = GetLine(initializer.OpenBraceToken);
        var tokenBeforeOpen = initializer.OpenBraceToken.GetPreviousToken();

        if (!tokenBeforeOpen.IsKind(SyntaxKind.None) && GetLine(tokenBeforeOpen) == openBraceLine)
        {
            context.ReportDiagnostic(Diagnostic.Create(FormatRule, initializer.OpenBraceToken.GetLocation()));
        }
    }

    private int GetLine(SyntaxToken token)
    {
        return token.GetLocation().GetLineSpan().StartLinePosition.Line;
    }
}
