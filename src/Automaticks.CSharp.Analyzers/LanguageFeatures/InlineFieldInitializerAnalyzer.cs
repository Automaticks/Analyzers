using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace Automaticks.CSharp.LanguageFeatures;

/// <summary>
///     Flags fields and auto-properties with inline initializers instead of constructor-based initialization.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InlineFieldInitializerAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when an inline initializer is detected on a field or auto-property.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static InlineFieldInitializerAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.InlineFieldInitializer,
            "Inline field or property initialization is forbidden",
            "'{0}' must not be initialized inline; initialize it in the constructor instead",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "Move the field or property initializer from the declaration to the constructor. Example: change `private readonly Foo _foo = new Foo();` to `private readonly Foo _foo;` and assign `_foo = new Foo();` inside the constructor body. This centralizes all object initialization in one place.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeField, SyntaxKind.FieldDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeProperty, SyntaxKind.PropertyDeclaration);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeField(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not FieldDeclarationSyntax field)
        {
            return;
        }

        if (HasConstModifier(field))
        {
            return;
        }

        var enclosingType = field.Parent as TypeDeclarationSyntax;

        foreach (var variable in field.Declaration.Variables)
        {
            if (variable.Initializer is null)
            {
                continue;
            }

            if (HasPrimaryConstructorParameterReference(variable.Initializer.Value, enclosingType))
            {
                continue;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, variable.Initializer.GetLocation(), variable.Identifier.Text));
        }
    }

    private void AnalyzeProperty(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not PropertyDeclarationSyntax property)
        {
            return;
        }

        if (property.Initializer is null)
        {
            return;
        }

        var enclosingType = property.Parent as TypeDeclarationSyntax;

        if (HasPrimaryConstructorParameterReference(property.Initializer.Value, enclosingType))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, property.Initializer.GetLocation(), property.Identifier.Text));
    }

    private bool HasConstModifier(FieldDeclarationSyntax field)
    {
        foreach (var modifier in field.Modifiers)
        {
            if (modifier.IsKind(SyntaxKind.ConstKeyword))
            {
                return true;
            }
        }

        return false;
    }

    private bool HasPrimaryConstructorParameterReference(ExpressionSyntax expression, TypeDeclarationSyntax? enclosingType)
    {
        if (expression is not IdentifierNameSyntax identifierName)
        {
            return false;
        }

        if (enclosingType?.ParameterList is null)
        {
            return false;
        }

        foreach (var parameter in enclosingType.ParameterList.Parameters)
        {
            if (parameter.Identifier.Text.Equals(identifierName.Identifier.Text, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
