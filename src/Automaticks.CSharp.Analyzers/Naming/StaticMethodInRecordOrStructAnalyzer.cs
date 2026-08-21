using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.Naming;

/// <summary>
///     Flags static methods declared inside a record, record struct, or struct.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class StaticMethodInRecordOrStructAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule;

    static StaticMethodInRecordOrStructAnalyzer()
    {
        Rule = new(
            DiagnosticIds.CSharp.StaticMethodInRecordOrStruct,
            "Static methods must only exist in static classes",
            "Method '{0}' is static but is declared in '{1}', which is a record or struct. Move it to a dedicated static class.",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "Move the static method to a dedicated `public static class`. A record or struct is still an instantiable type, so a static method on it carries the same problems ATXCS011 describes for non-static classes: it cannot participate in dependency injection and is hard to substitute in tests.");
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [Rule];
        }
    }

    private void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        var method = (context.Node as MethodDeclarationSyntax)!;
        if (!method.Modifiers.Any(SyntaxKind.StaticKeyword))
        {
            return;
        }

        if (method.Parent is not (RecordDeclarationSyntax or StructDeclarationSyntax))
        {
            return;
        }

        var containingType = (method.Parent as TypeDeclarationSyntax)!;
        context.ReportDiagnostic(Diagnostic.Create(
            Rule,
            method.Identifier.GetLocation(),
            method.Identifier.Text,
            containingType.Identifier.Text));
    }
}
