using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Automaticks.CSharp;

/// <summary>
///     Flags a public init-only auto-property whose value is assigned in the containing type's
///     sole instance constructor from a constructor parameter. The <c>init</c> setter is
///     redundant in this shape: callers cannot use object-initializer syntax (the explicit
///     constructor shadows the synthesized parameterless one) yet the setter remains a
///     <c>with</c>-expression back door that bypasses any constructor-side validation.
///     The following positions are exempt from this rule:
///     <list type="bullet">
///         <item><c>required init</c> properties (canonical immutable-DTO form; the compiler enforces caller-side initialization).</item>
///         <item><c>private</c>, <c>protected</c>, or <c>internal</c> init accessors (the setter is not externally callable, so the <c>with</c>-expression bypass is closed off).</item>
///         <item>Static properties (different lifecycle; static <c>init</c> is rare and intentional).</item>
///         <item>Types with zero or two-or-more instance constructors (deserialization or multi-shape construction).</item>
///         <item>Types where any constructor carries <c>[JsonConstructor]</c> (deserialization seam).</item>
///         <item>Constructors that seed a computed default (e.g. <c>Prop = Guid.NewGuid()</c>, <c>Prop = []</c>) — the RHS does not reference any constructor parameter, so the <c>init</c> setter provides legitimate caller-override capability via object initializer.</item>
///     </list>
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RedundantInitSetterAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a public init-only auto-property is set in the
    ///     containing type's sole instance constructor from a constructor parameter.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.CSharp.RedundantInitSetter,
        "Init-only setter is redundant when the property is assigned in the constructor",
        "Property '{0}' is set in the constructor from a parameter; replace 'init' with no setter ('{{ get; }}') or remove the constructor and use 'required init'",
        "CSharp",
        DiagnosticSeverity.Error,
        true,
        "When a public init-only property's value is assigned from a constructor parameter, the init setter contributes nothing at construction (callers must invoke the constructor — the synthesized parameterless ctor is shadowed) yet exposes a 'with'-expression back door that bypasses any constructor-side validation. Refactor: drop the init setter so the property becomes '{ get; }' assigned in the constructor, or delete the constructor entirely and promote the property to 'required init' so callers use object-initializer syntax. Exempt cases: 'required init' (canonical immutable-DTO), non-public init accessors, static properties, [JsonConstructor]-annotated types, types with zero or multiple constructors, and constructors that seed computed defaults from non-parameter expressions (the init setter then provides legitimate caller override).");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.PropertyDeclaration);
    }

    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        var property = (PropertyDeclarationSyntax)context.Node;

        var initAccessor = GetInitAccessor(property);
        if (initAccessor is null)
        {
            return;
        }

        if (HasModifier(property.Modifiers, SyntaxKind.RequiredKeyword))
        {
            return;
        }

        if (HasModifier(property.Modifiers, SyntaxKind.StaticKeyword))
        {
            return;
        }

        if (HasModifier(initAccessor.Modifiers, SyntaxKind.PrivateKeyword) ||
            HasModifier(initAccessor.Modifiers, SyntaxKind.ProtectedKeyword) ||
            HasModifier(initAccessor.Modifiers, SyntaxKind.InternalKeyword))
        {
            return;
        }

        if (property.Parent is not TypeDeclarationSyntax typeDecl)
        {
            return;
        }

        if (typeDecl is InterfaceDeclarationSyntax)
        {
            return;
        }

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

        if (hasJsonCtor)
        {
            return;
        }

        if (instanceCtors.Count != 1)
        {
            return;
        }

        var sole = instanceCtors[0];
        var propName = property.Identifier.ValueText;

        var assignment = FindCtorAssignment(sole, propName);
        if (assignment is null)
        {
            return;
        }

        var ctorParamNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var p in sole.ParameterList.Parameters)
        {
            ctorParamNames.Add(p.Identifier.ValueText);
        }

        if (ctorParamNames.Count == 0)
        {
            return;
        }

        if (!UsesAnyCtorParameter(assignment.Right, ctorParamNames))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, initAccessor.Keyword.GetLocation(), propName));
    }

    private static AccessorDeclarationSyntax? GetInitAccessor(PropertyDeclarationSyntax property)
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

    private static bool HasModifier(SyntaxTokenList modifiers, SyntaxKind kind)
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

    private static AssignmentExpressionSyntax? FindCtorAssignment(ConstructorDeclarationSyntax ctor, string propName)
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

    private static string? GetAssignmentTargetName(ExpressionSyntax left)
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

    private static bool UsesAnyCtorParameter(ExpressionSyntax rhs, HashSet<string> ctorParamNames)
    {
        if (rhs is IdentifierNameSyntax direct && ctorParamNames.Contains(direct.Identifier.ValueText))
        {
            return true;
        }

        foreach (var node in rhs.DescendantNodes())
        {
            if (node is IdentifierNameSyntax id && ctorParamNames.Contains(id.Identifier.ValueText))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasJsonConstructorAttribute(ConstructorDeclarationSyntax ctor)
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
}
