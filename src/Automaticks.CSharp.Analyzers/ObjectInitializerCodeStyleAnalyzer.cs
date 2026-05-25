using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp;

/// <summary>
///     Enforces that object, collection, array, and <c>with</c>-expression initializers always use
///     the multi-line format where the opening brace, every member, and the closing brace each
///     appear on their own line.
///     <para>
///         Bad:
///         <code>
///             var x = new Foo { A = 1, B = 2 };
///         </code>
///     </para>
///     <para>
///         Good:
///         <code>
///             var x = new Foo
///             {
///                 A = 1,
///                 B = 2
///             };
///         </code>
///     </para>
///     Empty initializer braces (<c>{ }</c>) are reported as a separate violation
///     (<see cref="EmptyBracesRule" />).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ObjectInitializerCodeStyleAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when an initializer member does not appear on its own line,
    ///     or when the opening / closing brace is not on its own line.
    /// </summary>
    public static readonly DiagnosticDescriptor FormatRule = new(
        DiagnosticIds.CSharp.ObjectInitializerFormat,
        "Initializer must use one member per line",
        "Each initializer member and the surrounding braces must be on their own line",
        "Style",
        DiagnosticSeverity.Error,
        true,
        "Format the object or collection initializer so that every member assignment and both braces are on their own lines. Example: `new Foo { X = 1, Y = 2 }` must become `new Foo\n{\n    X = 1,\n    Y = 2\n}`. Single-element initializers are not exempt.");

    /// <summary>
    ///     The diagnostic rule reported when an initializer block contains no members.
    /// </summary>
    public static readonly DiagnosticDescriptor EmptyBracesRule = new(
        DiagnosticIds.CSharp.ObjectInitializerEmptyBraces,
        "Empty initializer braces are forbidden",
        "Empty initializer braces are forbidden; remove the initializer block",
        "Style",
        DiagnosticSeverity.Error,
        true,
        "Empty initializer braces add noise without providing value. Remove them and rely on the constructor or default values instead.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [FormatRule, EmptyBracesRule];

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

    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        var initializer = (InitializerExpressionSyntax)context.Node;

        if (initializer.Expressions.Count == 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(EmptyBracesRule, initializer.OpenBraceToken.GetLocation()));
            return;
        }

        CheckOpenBrace(context, initializer);
        CheckMembers(context, initializer);
        CheckCloseBrace(context, initializer);
    }

    private static void CheckOpenBrace(SyntaxNodeAnalysisContext context, InitializerExpressionSyntax initializer)
    {
        var openBraceLine = GetLine(initializer.OpenBraceToken);
        var tokenBeforeOpen = initializer.OpenBraceToken.GetPreviousToken();

        if (!tokenBeforeOpen.IsKind(SyntaxKind.None) && GetLine(tokenBeforeOpen) == openBraceLine)
        {
            context.ReportDiagnostic(Diagnostic.Create(FormatRule, initializer.OpenBraceToken.GetLocation()));
        }
    }

    private static void CheckMembers(SyntaxNodeAnalysisContext context, InitializerExpressionSyntax initializer)
    {
        var openBraceLine = GetLine(initializer.OpenBraceToken);
        var closeBraceLine = GetLine(initializer.CloseBraceToken);
        var count = initializer.Expressions.Count;
        var startLines = new int[count];
        var endLines = new int[count];

        for (var i = 0; i < count; i++)
        {
            startLines[i] = GetLine(initializer.Expressions[i].GetFirstToken());
            endLines[i] = GetLine(initializer.Expressions[i].GetLastToken());
        }

        for (var i = 0; i < count; i++)
        {
            var sharesWithOpenBrace = startLines[i] == openBraceLine;
            var sharesWithCloseBrace = endLines[i] == closeBraceLine;
            var sharesWithPrevious = i > 0 && startLines[i] == endLines[i - 1];
            var sharesWithNext = i < count - 1 && endLines[i] == startLines[i + 1];

            if (sharesWithOpenBrace || sharesWithCloseBrace || sharesWithPrevious || sharesWithNext)
            {
                context.ReportDiagnostic(Diagnostic.Create(FormatRule, initializer.Expressions[i].GetLocation()));
            }
        }
    }

    private static void CheckCloseBrace(SyntaxNodeAnalysisContext context, InitializerExpressionSyntax initializer)
    {
        var closeBraceLine = GetLine(initializer.CloseBraceToken);
        var lastExpression = initializer.Expressions[initializer.Expressions.Count - 1];

        if (GetLine(lastExpression.GetLastToken()) == closeBraceLine)
        {
            context.ReportDiagnostic(Diagnostic.Create(FormatRule, initializer.CloseBraceToken.GetLocation()));
        }
    }

    private static int GetLine(SyntaxToken token)
    {
        return token.GetLocation().GetLineSpan().StartLinePosition.Line;
    }
}
