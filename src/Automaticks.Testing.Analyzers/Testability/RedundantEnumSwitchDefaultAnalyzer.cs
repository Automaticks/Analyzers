using Automaticks.Testing.Coverage;
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
            "Every member of '{0}' already has its own case, so this default runs only when a value outside the enum is cast in. Cover it with a test that passes an out-of-range value.",
            "Testing",
            DiagnosticSeverity.Warning,
            true,
            "When every declared enum member has its own case, the default branch runs only when a caller casts in a value the enum does not declare. Leaving that branch untested blocks full branch coverage. Keep the branch and add a test that passes an out-of-range value: an enum does not constrain its underlying value at runtime, so deleting the branch turns invalid input into a silent fall-through. This rule goes quiet once a supplied coverage report shows the branch was executed, and warns as usual when no report is supplied.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(RegisterCompilationStart);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [Rule];
        }
    }

    private void AnalyzeSwitchExpression(SyntaxNodeAnalysisContext context, CoverageReport? report)
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

        if (HasCoveredDefault(context, report, defaultNode))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, defaultNode.GetLocation(), enumType.Name));
    }

    private void AnalyzeSwitchStatement(SyntaxNodeAnalysisContext context, CoverageReport? report)
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

        if (HasCoveredDefault(context, report, defaultNode))
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

    private bool HasCoveredDefault(SyntaxNodeAnalysisContext context, CoverageReport? report, SyntaxNode defaultNode)
    {
        if (report is null)
        {
            return false;
        }

        var file = report.FindFile(context.Node.SyntaxTree.FilePath);
        if (file is null)
        {
            return false;
        }

        var lineSpan = defaultNode.Parent!.GetLocation().GetLineSpan();
        return file.HasCoveredLine(lineSpan.StartLinePosition.Line + 1, lineSpan.EndLinePosition.Line + 1);
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

    private void RegisterCompilationStart(CompilationStartAnalysisContext context)
    {
        var report = CoverageReportLocator.Find(context.Options, context.CancellationToken);
        context.RegisterSyntaxNodeAction(
            nodeContext => AnalyzeSwitchStatement(nodeContext, report),
            SyntaxKind.SwitchStatement);
        context.RegisterSyntaxNodeAction(
            nodeContext => AnalyzeSwitchExpression(nodeContext, report),
            SyntaxKind.SwitchExpression);
    }
}
