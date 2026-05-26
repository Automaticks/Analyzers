using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Automaticks.CSharp;

/// <summary>
///     Flags identifiers (variables, parameters, method names, type names) that contain abbreviated
///     segments — for example <c>ct</c>, <c>cts</c>, <c>sb</c>, <c>ctx</c>, or single-letter names.
///     Spatial axis names (<c>x</c>, <c>y</c>, <c>z</c>) are exempt.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AbbreviatedIdentifierAnalyzer : DiagnosticAnalyzer
{
    private static readonly HashSet<string> AxisSegments =
        new(StringComparer.OrdinalIgnoreCase) { "x", "y", "z" };

    /// <summary>
    ///     The diagnostic rule reported when an identifier contains an abbreviated segment.
    /// </summary>
    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.CSharp.AbbreviatedIdentifier,
        "Identifier contains an abbreviated segment",
        "Identifier '{0}' contains an abbreviated segment '{1}'; use a full descriptive name instead",
        "CSharp",
        DiagnosticSeverity.Error,
        true,
        "Identifiers must use full, descriptive names — never abbreviations. Common banned abbreviations include: `ct`/`cts` (use `cancellationToken`), `sb` (use `stringBuilder`), `ctx` (use `context`), `vm` (use `viewModel`), `e` on event handlers (use the full event args name). Rename the flagged segment to its full English word or phrase.");

    private static readonly HashSet<char> Vowels = ['a', 'e', 'i', 'o', 'u', 'y'];

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeVariableDeclarator, SyntaxKind.VariableDeclarator);
        context.RegisterSyntaxNodeAction(AnalyzeParameter, SyntaxKind.Parameter);
        context.RegisterSyntaxNodeAction(AnalyzeForEachStatement, SyntaxKind.ForEachStatement);
        context.RegisterSyntaxNodeAction(AnalyzeSingleVariableDesignation, SyntaxKind.SingleVariableDesignation);
        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
        context.RegisterSymbolAction(AnalyzeProperty, SymbolKind.Property);
    }

    private static void AnalyzeForEachStatement(SyntaxNodeAnalysisContext context)
    {
        var forEach = (ForEachStatementSyntax)context.Node;
        ReportIfAbbreviated(context, forEach.Identifier.Text, forEach.Identifier.GetLocation());
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context)
    {
        var method = (IMethodSymbol)context.Symbol;

        if (method.IsOverride && IsExternalOverride(method))
        {
            return;
        }

        if (method.ExplicitInterfaceImplementations.Length > 0)
        {
            foreach (var ifaceMethod in method.ExplicitInterfaceImplementations)
            {
                if (ifaceMethod.DeclaringSyntaxReferences.IsEmpty)
                {
                    return;
                }
            }
        }

        var abbreviated = FindFirstAbbreviatedSegment(method.Name);
        if (abbreviated is null)
        {
            return;
        }

        Location location;
        if (method.Locations.Length > 0)
        {
            location = method.Locations[0];
        }
        else
        {
            location = Location.None;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, location, method.Name, abbreviated));
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedType = (INamedTypeSymbol)context.Symbol;

        string nameToCheck;
        if (namedType is { TypeKind: TypeKind.Interface, Name.Length: > 1 } &&
            namedType.Name[0] == 'I' &&
            char.IsUpper(namedType.Name[1]))
        {
            nameToCheck = namedType.Name.Substring(1);
        }
        else
        {
            nameToCheck = namedType.Name;
        }

        var abbreviated = FindFirstAbbreviatedSegment(nameToCheck);
        if (abbreviated is null)
        {
            return;
        }

        Location location;
        if (namedType.Locations.Length > 0)
        {
            location = namedType.Locations[0];
        }
        else
        {
            location = Location.None;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, location, namedType.Name, abbreviated));
    }

    private static void AnalyzeParameter(SyntaxNodeAnalysisContext context)
    {
        var parameter = (ParameterSyntax)context.Node;

        if (parameter.Parent?.Parent is BaseMethodDeclarationSyntax methodDecl)
        {
            var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDecl);
            if (methodSymbol is { IsOverride: true } && IsExternalOverride(methodSymbol))
            {
                return;
            }
        }

        ReportIfAbbreviated(context, parameter.Identifier.Text, parameter.Identifier.GetLocation());
    }

    private static void AnalyzeSingleVariableDesignation(SyntaxNodeAnalysisContext context)
    {
        var designation = (SingleVariableDesignationSyntax)context.Node;
        ReportIfAbbreviated(context, designation.Identifier.Text, designation.Identifier.GetLocation());
    }

    private static void AnalyzeVariableDeclarator(SyntaxNodeAnalysisContext context)
    {
        var declarator = (VariableDeclaratorSyntax)context.Node;
        ReportIfAbbreviated(context, declarator.Identifier.Text, declarator.Identifier.GetLocation());
    }

    private static string? FindFirstAbbreviatedSegment(string identifier)
    {
        var stripped = identifier.TrimStart('_').TrimEnd('_');
        if (stripped.Length == 0)
        {
            return null;
        }

        var segments = new List<string>(SplitCamelCase(stripped));
        for (var index = 0; index < segments.Count; index++)
        {
            var previousSegment = index > 0 ? segments[index - 1] : null;
            var nextSegment = index + 1 < segments.Count ? segments[index + 1] : null;
            if (IsAbbreviatedSegment(segments[index], previousSegment, nextSegment))
            {
                return segments[index];
            }
        }

        return null;
    }

    private static bool IsAbbreviatedSegment(string segment, string? previousSegment, string? nextSegment)
    {
        if (AxisSegments.Contains(segment) ||
            segment.Equals("Xml", StringComparison.OrdinalIgnoreCase) ||
            (segment.Equals("T", StringComparison.OrdinalIgnoreCase) && string.Equals(previousSegment, "Of", StringComparison.OrdinalIgnoreCase)) ||
            (segment.Equals("N", StringComparison.OrdinalIgnoreCase) && string.Equals(nextSegment, "Substitute", StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        if (segment.Length == 1)
        {
            return true;
        }

        if (segment.Length <= 4)
        {
            var vowelCount = 0;
            foreach (var character in segment)
            {
                if (Vowels.Contains(char.ToLowerInvariant(character)))
                {
                    vowelCount++;
                }
            }

            return vowelCount == 0;
        }

        return false;
    }

    private static bool IsExternalOverride(IMethodSymbol method)
    {
        var overridden = method.OverriddenMethod;
        while (overridden is not null)
        {
            if (overridden.DeclaringSyntaxReferences.IsEmpty)
            {
                return true;
            }

            overridden = overridden.OverriddenMethod;
        }

        return false;
    }

    private static bool IsExternalPropertyOverride(IPropertySymbol property)
    {
        var overridden = property.OverriddenProperty;
        while (overridden is not null)
        {
            if (overridden.DeclaringSyntaxReferences.IsEmpty)
            {
                return true;
            }

            overridden = overridden.OverriddenProperty;
        }

        return false;
    }

    private static void AnalyzeProperty(SymbolAnalysisContext context)
    {
        var property = (IPropertySymbol)context.Symbol;

        if (property.IsIndexer)
        {
            return;
        }

        if (property.IsOverride && IsExternalPropertyOverride(property))
        {
            return;
        }

        var abbreviated = FindFirstAbbreviatedSegment(property.Name);
        if (abbreviated is null)
        {
            return;
        }

        Location location;
        if (property.Locations.Length > 0)
        {
            location = property.Locations[0];
        }
        else
        {
            location = Location.None;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, location, property.Name, abbreviated));
    }

    private static void ReportIfAbbreviated(SyntaxNodeAnalysisContext context, string name, Location location)
    {
        var abbreviated = FindFirstAbbreviatedSegment(name);
        if (abbreviated is null)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, location, name, abbreviated));
    }

    private static IEnumerable<string> SplitCamelCase(string name)
    {
        var start = 0;
        for (var index = 1; index < name.Length; index++)
        {
            var current = name[index];
            var previous = name[index - 1];
            bool startsNewSegment;

            if (char.IsUpper(current) && char.IsLower(previous))
            {
                startsNewSegment = true;
            }
            else if (char.IsUpper(current) && index + 1 < name.Length && char.IsLower(name[index + 1]) && char.IsUpper(previous))
            {
                startsNewSegment = true;
            }
            else
            {
                startsNewSegment = false;
            }

            if (startsNewSegment)
            {
                yield return name.Substring(start, index - start);
                start = index;
            }
        }

        yield return name.Substring(start);
    }
}
