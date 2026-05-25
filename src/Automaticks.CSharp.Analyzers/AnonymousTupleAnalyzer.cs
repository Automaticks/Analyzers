using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp;

/// <summary>
///     Flags anonymous tuple types (<c>(T1, T2)</c> syntax) used as return types, field types,
///     parameter types, or inline deconstruction targets. Named records, classes, or structs
///     must be used instead.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AnonymousTupleAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when an anonymous tuple type is used.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.CSharp.AnonymousTuple,
        "Anonymous tuple types are forbidden",
        "Replace this tuple with a strongly typed entity (record, class, or struct) that explicitly models the data",
        "CSharp",
        DiagnosticSeverity.Error,
        true,
        "Replace the anonymous tuple type with a named `record`, `class`, or `struct`. Example: instead of returning `(string Name, int Age)`, declare `public record PersonInfo(string Name, int Age)` and use that. Named types make intent explicit and improve refactorability.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeTupleType, SyntaxKind.TupleType);
        context.RegisterSyntaxNodeAction(AnalyzeTupleExpression, SyntaxKind.TupleExpression);
        context.RegisterSyntaxNodeAction(AnalyzeDeclarationExpression, SyntaxKind.DeclarationExpression);
    }

    private static void AnalyzeDeclarationExpression(SyntaxNodeAnalysisContext context)
    {
        var declaration = (DeclarationExpressionSyntax)context.Node;

        if (declaration.Designation is not ParenthesizedVariableDesignationSyntax)
        {
            return;
        }

        if (declaration.Parent is not AssignmentExpressionSyntax assignment)
        {
            return;
        }

        if (assignment.Left != declaration)
        {
            return;
        }

        if (context.SemanticModel.GetTypeInfo(assignment.Right).Type is not INamedTypeSymbol { IsTupleType: true })
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
    }

    private static void AnalyzeTupleExpression(SyntaxNodeAnalysisContext context)
    {
        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
    }

    private static void AnalyzeTupleType(SyntaxNodeAnalysisContext context)
    {
        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
    }
}
