using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Automaticks.CSharp.LanguageFeatures;

/// <summary>
///     Flags a public init-only auto-property whose value is assigned in the containing type's sole instance constructor from a constructor parameter.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RedundantInitSetterAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a public init-only auto-property is set in the containing type's sole instance constructor from a constructor par...
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static RedundantInitSetterAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.RedundantInitSetter,
            "Init-only setter is redundant when the property is assigned in the constructor",
            "Property '{0}' is set in the constructor from a parameter; replace 'init' with no setter ('{{ get; }}') or remove the constructor and use 'required init'",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "When a public init-only property's value is assigned from a constructor parameter, the init setter contributes nothing at construction (callers must invoke the constructor — the synthesized parameterless ctor is shadowed) yet exposes a 'with'-expression back door that bypasses any constructor-side validation. Refactor: drop the init setter so the property becomes '{ get; }' assigned in the constructor, or delete the constructor entirely and promote the property to 'required init' so callers use object-initializer syntax. Exempt cases: 'required init' (canonical immutable-DTO), non-public init accessors, static properties, [JsonConstructor]-annotated types, types with zero or multiple constructors, and constructors that seed computed defaults from non-parameter expressions (the init setter then provides legitimate caller override).");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.PropertyDeclaration);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [Rule];
        }
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        var property = (context.Node as PropertyDeclarationSyntax)!;
        var initAccessor = GetInitAccessor(property);
        if (initAccessor is null)
        {
            return;
        }

        if (HasExemptModifiers(property, initAccessor))
        {
            return;
        }

        if (property.Parent is not TypeDeclarationSyntax typeDecl || typeDecl is InterfaceDeclarationSyntax)
        {
            return;
        }

        var soleCtor = GetSoleEligibleConstructor(typeDecl);
        if (soleCtor is null)
        {
            return;
        }

        var propertyName = property.Identifier.ValueText;
        var assignment = FindCtorAssignment(soleCtor, propertyName);
        if (assignment is null)
        {
            return;
        }

        var ctorParameterNames = GetParameterNames(soleCtor);
        if (ctorParameterNames.Count == 0 || !HasCtorParameterReference(assignment.Right, ctorParameterNames))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, initAccessor.Keyword.GetLocation(), propertyName));
    }

    private AssignmentExpressionSyntax? FindCtorAssignment(ConstructorDeclarationSyntax ctor, string propName)
    {
        var body = ctor.Body;
        if (body is null)
        {
            return null;
        }

        foreach (var statement in body.Statements)
        {
            if (statement is not ExpressionStatementSyntax expressionStatement)
            {
                continue;
            }

            if (expressionStatement.Expression is not AssignmentExpressionSyntax assignment)
            {
                continue;
            }

            if (!assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
            {
                continue;
            }

            var targetName = GetAssignmentTargetName(assignment.Left);
            if (string.Equals(targetName, propName, StringComparison.Ordinal))
            {
                return assignment;
            }
        }

        return null;
    }

    private string? GetAssignmentTargetName(ExpressionSyntax left)
    {
        if (left is IdentifierNameSyntax identifier)
        {
            return identifier.Identifier.ValueText;
        }

        if (left is MemberAccessExpressionSyntax memberAccess &&
            memberAccess.Expression is ThisExpressionSyntax)
        {
            return memberAccess.Name.Identifier.ValueText;
        }

        return null;
    }

    private AccessorDeclarationSyntax? GetInitAccessor(PropertyDeclarationSyntax property)
    {
        var accessorList = property.AccessorList;
        if (accessorList is null)
        {
            return null;
        }

        foreach (var accessor in accessorList.Accessors)
        {
            if (accessor.IsKind(SyntaxKind.InitAccessorDeclaration))
            {
                return accessor;
            }
        }

        return null;
    }

    private HashSet<string> GetParameterNames(ConstructorDeclarationSyntax ctor)
    {
        var parameterNames = new HashSet<string>(StringComparer.Ordinal);

        foreach (var parameter in ctor.ParameterList.Parameters)
        {
            parameterNames.Add(parameter.Identifier.ValueText);
        }

        return parameterNames;
    }

    private ConstructorDeclarationSyntax? GetSoleEligibleConstructor(TypeDeclarationSyntax typeDecl)
    {
        var instanceCtors = new List<ConstructorDeclarationSyntax>();
        var hasJsonCtor = false;

        foreach (var member in typeDecl.Members)
        {
            if (member is not ConstructorDeclarationSyntax ctor)
            {
                continue;
            }

            if (HasJsonConstructorAttribute(ctor))
            {
                hasJsonCtor = true;
            }

            if (!HasModifier(ctor.Modifiers, SyntaxKind.StaticKeyword))
            {
                instanceCtors.Add(ctor);
            }
        }

        if (hasJsonCtor || instanceCtors.Count != 1)
        {
            return null;
        }

        return instanceCtors[0];
    }

    private bool HasCtorParameterReference(ExpressionSyntax rightHandSide, HashSet<string> ctorParameterNames)
    {
        if (rightHandSide is IdentifierNameSyntax direct && ctorParameterNames.Contains(direct.Identifier.ValueText))
        {
            return true;
        }

        foreach (var node in rightHandSide.DescendantNodes())
        {
            if (node is IdentifierNameSyntax id && ctorParameterNames.Contains(id.Identifier.ValueText))
            {
                return true;
            }
        }

        return false;
    }

    private bool HasExemptModifiers(PropertyDeclarationSyntax property, AccessorDeclarationSyntax initAccessor)
    {
        if (HasModifier(property.Modifiers, SyntaxKind.RequiredKeyword))
        {
            return true;
        }

        if (HasModifier(property.Modifiers, SyntaxKind.StaticKeyword))
        {
            return true;
        }

        return HasModifier(initAccessor.Modifiers, SyntaxKind.PrivateKeyword) ||
               HasModifier(initAccessor.Modifiers, SyntaxKind.ProtectedKeyword) ||
               HasModifier(initAccessor.Modifiers, SyntaxKind.InternalKeyword);
    }

    private bool HasJsonConstructorAttribute(ConstructorDeclarationSyntax ctor)
    {
        foreach (var attributeList in ctor.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var name = attribute.Name.ToString();
                if (name.EndsWith("JsonConstructor", StringComparison.Ordinal) ||
                    name.EndsWith("JsonConstructorAttribute", StringComparison.Ordinal))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool HasModifier(SyntaxTokenList modifiers, SyntaxKind kind)
    {
        foreach (var modifier in modifiers)
        {
            if (modifier.IsKind(kind))
            {
                return true;
            }
        }

        return false;
    }
}
