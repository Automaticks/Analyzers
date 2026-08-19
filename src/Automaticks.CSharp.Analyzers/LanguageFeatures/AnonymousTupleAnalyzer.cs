using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.LanguageFeatures;

/// <summary>
///     Flags anonymous tuple types ((T1, T2) syntax) used as return types, field types, parameter types, or inline deconstruction targets.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AnonymousTupleAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when an anonymous tuple type is used.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static AnonymousTupleAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.AnonymousTuple,
            "Anonymous tuple types are forbidden",
            "Replace this tuple with a strongly typed entity (record, class, or struct) that explicitly models the data",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "Replace the anonymous tuple type with a named `record`, `class`, or `struct`. Example: instead of returning `(string Name, int Age)`, declare `public record PersonInfo(string Name, int Age)` and use that. Named types make intent explicit and improve refactorability.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeTupleType, SyntaxKind.TupleType);
        context.RegisterSyntaxNodeAction(AnalyzeTupleExpression, SyntaxKind.TupleExpression);
        context.RegisterSyntaxNodeAction(AnalyzeDeclarationExpression, SyntaxKind.DeclarationExpression);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeDeclarationExpression(SyntaxNodeAnalysisContext context)
    {
        var declaration = (context.Node as DeclarationExpressionSyntax)!;

        if (declaration.Designation is not ParenthesizedVariableDesignationSyntax)
        {
            return;
        }

        if (declaration.Parent is not AssignmentExpressionSyntax assignment)
        {
            return;
        }

        if (context.SemanticModel.GetTypeInfo(assignment.Right).Type is not INamedTypeSymbol { IsTupleType: true })
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
    }

    private void AnalyzeTupleExpression(SyntaxNodeAnalysisContext context)
    {
        var tuple = (context.Node as TupleExpressionSyntax)!;

        if (HasSwapAssignment(tuple))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
    }

    private void AnalyzeTupleType(SyntaxNodeAnalysisContext context)
    {
        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
    }

    private bool HasOnlyPlainIdentifiers(ExpressionSyntax? expression)
    {
        if (expression is not TupleExpressionSyntax tuple)
        {
            return false;
        }

        foreach (var argument in tuple.Arguments)
        {
            if (argument.NameColon is not null || argument.Expression is not IdentifierNameSyntax)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Recognises <c>(a, b) = (b, a)</c>, which uses tuple syntax for simultaneous assignment
    ///     rather than to carry a data shape, so there is no entity to model.
    /// </summary>
    private bool HasSwapAssignment(TupleExpressionSyntax tuple)
    {
        if (tuple.Parent is not AssignmentExpressionSyntax assignment)
        {
            return false;
        }

        return HasOnlyPlainIdentifiers(assignment.Left) && HasOnlyPlainIdentifiers(assignment.Right);
    }
}
