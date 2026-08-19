using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.Testing.Testability;

/// <summary>
///     Flags a bitmask test performed against a mask that has more than one bit set.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CompositeBitmaskTestAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule;

    static CompositeBitmaskTestAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.Testing.CompositeBitmaskTest,
            "Bitmask test must use a single-bit mask",
            "This test uses a mask with {0} bits set, so no test case can prove that each bit individually changes the outcome. Test one bit at a time.",
            "Testing",
            DiagnosticSeverity.Warning,
            true,
            "A test such as `(flags & (A | B)) != 0` passes whenever either bit is set, so a test suite can reach full branch coverage while never exercising one of the bits. Split the check into one test per bit so every bit is independently observable, matching the way safety-critical code proves each condition in a mask affects the result.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeBitwiseAnd, SyntaxKind.BitwiseAndExpression);
        context.RegisterSyntaxNodeAction(AnalyzeHasFlag, SyntaxKind.InvocationExpression);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeBitwiseAnd(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not BinaryExpressionSyntax binary)
        {
            return;
        }

        if (!HasZeroComparisonParent(binary))
        {
            return;
        }

        var bits = CountMaskBits(context, binary.Right);
        if (bits < 2)
        {
            bits = CountMaskBits(context, binary.Left);
        }

        if (bits >= 2)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, binary.GetLocation(), bits));
        }
    }

    private void AnalyzeHasFlag(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InvocationExpressionSyntax invocation)
        {
            return;
        }

        var arguments = invocation.ArgumentList.Arguments;
        if (arguments.Count != 1 || !HasEnumHasFlagMethod(context, invocation))
        {
            return;
        }

        var bits = CountMaskBits(context, arguments[0].Expression);
        if (bits >= 2)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation(), bits));
        }
    }

    private int CountMaskBits(SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
    {
        var constant = context.SemanticModel.GetConstantValue(expression);
        if (!constant.HasValue)
        {
            return 0;
        }

        var bits = ToBitPattern(constant.Value);
        var count = 0;
        while (bits != 0)
        {
            bits &= bits - 1;
            count += 1;
        }

        return count;
    }

    private bool HasEnumHasFlagMethod(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation)
    {
        if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol method)
        {
            return false;
        }

        if (method.Name != "HasFlag")
        {
            return false;
        }

        var enumType = context.SemanticModel.Compilation.GetTypeByMetadataName("System.Enum");
        return enumType is not null && SymbolEqualityComparer.Default.Equals(method.ContainingType, enumType);
    }

    private bool HasZeroComparisonParent(BinaryExpressionSyntax binary)
    {
        SyntaxNode current = binary;
        while (current.Parent is ParenthesizedExpressionSyntax parenthesized)
        {
            current = parenthesized;
        }

        if (current.Parent is not BinaryExpressionSyntax comparison)
        {
            return false;
        }

        if (!comparison.IsKind(SyntaxKind.EqualsExpression) && !comparison.IsKind(SyntaxKind.NotEqualsExpression))
        {
            return false;
        }

        return HasZeroLiteral(comparison.Left) || HasZeroLiteral(comparison.Right);
    }

    private bool HasZeroLiteral(ExpressionSyntax expression)
    {
        return expression is LiteralExpressionSyntax literal
               && literal.IsKind(SyntaxKind.NumericLiteralExpression)
               && literal.Token.ValueText == "0";
    }

    private ulong ToBitPattern(object? value)
    {
        switch (value)
        {
            case int intValue:
                return unchecked((ulong)intValue);
            case uint uintValue:
                return uintValue;
            case long longValue:
                return unchecked((ulong)longValue);
            case ulong ulongValue:
                return ulongValue;
            case short shortValue:
                return unchecked((ulong)shortValue);
            case ushort ushortValue:
                return ushortValue;
            case byte byteValue:
                return byteValue;
            case sbyte sbyteValue:
                return unchecked((ulong)sbyteValue);
            default:
                return 0;
        }
    }
}
