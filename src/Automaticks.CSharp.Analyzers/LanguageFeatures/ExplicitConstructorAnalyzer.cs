using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.LanguageFeatures;

/// <summary>
///     Flags any type declaration that uses a primary constructor — class Foo(int x), struct Foo(int x), record Foo(int x), or record struct Foo(int x).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ExplicitConstructorAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a primary constructor is detected.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static ExplicitConstructorAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.ExplicitConstructor,
            "Explicit constructors are required",
            "Declare an explicit constructor for '{0}' instead of using a primary constructor",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "Replace the primary constructor with an explicit constructor body. Example: change `class Foo(string name) { }` to a class with a field `private readonly string _name;` and a constructor `public Foo(string name) { _name = name; }`. Move all primary-constructor parameters to fields or properties initialized in the constructor body.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(
            AnalyzeType,
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKind.RecordDeclaration,
            SyntaxKind.RecordStructDeclaration);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [Rule];
        }
    }

    private void AnalyzeType(SyntaxNodeAnalysisContext context)
    {
        var typeDeclaration = (context.Node as TypeDeclarationSyntax)!;
        if (typeDeclaration.ParameterList is null)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, typeDeclaration.Identifier.GetLocation(), typeDeclaration.Identifier.Text));
    }
}
