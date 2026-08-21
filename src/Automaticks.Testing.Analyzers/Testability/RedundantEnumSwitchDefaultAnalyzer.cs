using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Automaticks.Testing.Testability;

/// <summary>
///     Flags a default label on a switch over an enum whose members are all already handled.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RedundantEnumSwitchDefaultAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule;

    static RedundantEnumSwitchDefaultAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.Testing.RedundantEnumSwitchDefault,
            "Default label on an exhaustive enum switch blocks full branch coverage",
            "Every member of '{0}' already has its own case, so this default is reachable only by casting an out-of-range value, which ordinary tests never do. Drop the default, or add a test that casts an undefined value.",
            "Testing",
            DiagnosticSeverity.Warning,
            true,
            "When every declared enum member has its own case, the default branch can only be reached through an invalid cast such as `(Color)99`, something ordinary test cases never produce. That branch therefore permanently blocks full branch coverage. Remove the default so the compiler proves exhaustiveness, or add a test that exercises the cast.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeSwitchStatement, SyntaxKind.SwitchStatement);
        context.RegisterSyntaxNodeAction(AnalyzeSwitchExpression, SyntaxKind.SwitchExpression);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [Rule];
        }
    }

    private void AnalyzeSwitchExpression(SyntaxNodeAnalysisContext context)
    {
        var switchExpression = (context.Node as SwitchExpressionSyntax)!;
        var enumType = GetEnumType(context, switchExpression.GoverningExpression);
        if (enumType is null)
        {
            return;
        }

        var handledMembers = new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default);
        SyntaxNode? defaultNode = null;

        foreach (var arm in switchExpression.Arms)
        {
            if (arm.Pattern is DiscardPatternSyntax)
            {
                defaultNode = arm.Pattern;
                continue;
            }

            var fieldSymbol = GetPatternFieldSymbol(context, arm.Pattern);
            if (fieldSymbol is not null)
            {
                handledMembers.Add(fieldSymbol);
            }
        }

        if (defaultNode is null || !IsExhaustive(enumType, handledMembers))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, defaultNode.GetLocation(), enumType.Name));
    }

    private void AnalyzeSwitchStatement(SyntaxNodeAnalysisContext context)
    {
        var switchStatement = (context.Node as SwitchStatementSyntax)!;
        var enumType = GetEnumType(context, switchStatement.Expression);
        if (enumType is null)
        {
            return;
        }

        var handledMembers = new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default);
        SyntaxNode? defaultNode = null;

        foreach (var section in switchStatement.Sections)
        {
            foreach (var label in section.Labels)
            {
                if (label is DefaultSwitchLabelSyntax)
                {
                    defaultNode = label;
                    continue;
                }

                var fieldSymbol = GetLabelFieldSymbol(context, label);
                if (fieldSymbol is not null)
                {
                    handledMembers.Add(fieldSymbol);
                }
            }
        }

        if (defaultNode is null || !IsExhaustive(enumType, handledMembers))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, defaultNode.GetLocation(), enumType.Name));
    }

    private INamedTypeSymbol? GetEnumType(SyntaxNodeAnalysisContext context, ExpressionSyntax governingExpression)
    {
        var type = context.SemanticModel.GetTypeInfo(governingExpression, context.CancellationToken).Type;
        return type is INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType ? enumType : null;
    }

    private IFieldSymbol? GetLabelFieldSymbol(SyntaxNodeAnalysisContext context, SwitchLabelSyntax label)
    {
        if (label is CaseSwitchLabelSyntax caseLabel)
        {
            return context.SemanticModel.GetSymbolInfo(caseLabel.Value, context.CancellationToken).Symbol as IFieldSymbol;
        }

        var patternLabel = (label as CasePatternSwitchLabelSyntax)!;
        return GetPatternFieldSymbol(context, patternLabel.Pattern);
    }

    private IFieldSymbol? GetPatternFieldSymbol(SyntaxNodeAnalysisContext context, PatternSyntax pattern)
    {
        if (pattern is not ConstantPatternSyntax constantPattern)
        {
            return null;
        }

        return context.SemanticModel.GetSymbolInfo(constantPattern.Expression, context.CancellationToken).Symbol as IFieldSymbol;
    }

    private bool IsExhaustive(INamedTypeSymbol enumType, HashSet<IFieldSymbol> handledMembers)
    {
        foreach (var member in enumType.GetMembers())
        {
            if (member is IFieldSymbol field && field.HasConstantValue && !handledMembers.Contains(field))
            {
                return false;
            }
        }

        return true;
    }
}
