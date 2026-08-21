using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Automaticks.CSharp.Naming;

/// <summary>
///     Flags identifiers (variables, parameters, method names, type names) that contain abbreviated segments — for example ct, cts, sb, ctx, or single-let...
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AbbreviatedIdentifierAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when an identifier contains an abbreviated segment.
    /// </summary>
    private static readonly ImmutableHashSet<string> AxisSegments;
    private static readonly DiagnosticDescriptor Rule;
    private static readonly ImmutableHashSet<char> Vowels;

    static AbbreviatedIdentifierAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.AbbreviatedIdentifier,
            "Identifier contains an abbreviated segment",
            "Identifier '{0}' contains an abbreviated segment '{1}'; use a full descriptive name instead",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "Identifiers must use full, descriptive names — never abbreviations. Common banned abbreviations include: `ct`/`cts` (use `cancellationToken`), `sb` (use `stringBuilder`), `ctx` (use `context`), `vm` (use `viewModel`), `e` on event handlers (use the full event args name). Rename the flagged segment to its full English word or phrase.");
        Rule = rule;
        var axisSegments = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "x",
            "y",
            "z"
        };
        AxisSegments = axisSegments.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);
        Vowels = ['a', 'e', 'i', 'o', 'u', 'y'];
    }

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

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [Rule];
        }
    }

    private void AnalyzeForEachStatement(SyntaxNodeAnalysisContext context)
    {
        var forEach = (context.Node as ForEachStatementSyntax)!;
        ReportIfAbbreviated(context, forEach.Identifier.Text, forEach.Identifier.GetLocation());
    }

    private void AnalyzeMethod(SymbolAnalysisContext context)
    {
        var method = (context.Symbol as IMethodSymbol)!;
        if (method.IsOverride && HasExternalOverride(method))
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

        var location = method.Locations[0];
        context.ReportDiagnostic(Diagnostic.Create(Rule, location, method.Name, abbreviated));
    }

    private void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedType = (context.Symbol as INamedTypeSymbol)!;
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

        var location = namedType.Locations[0];
        context.ReportDiagnostic(Diagnostic.Create(Rule, location, namedType.Name, abbreviated));
    }

    private void AnalyzeParameter(SyntaxNodeAnalysisContext context)
    {
        var parameter = (context.Node as ParameterSyntax)!;
        if (parameter.Parent!.Parent is BaseMethodDeclarationSyntax methodDecl)
        {
            var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDecl);
            if (methodSymbol is { IsOverride: true } && HasExternalOverride(methodSymbol))
            {
                return;
            }
        }

        ReportIfAbbreviated(context, parameter.Identifier.Text, parameter.Identifier.GetLocation());
    }

    private void AnalyzeProperty(SymbolAnalysisContext context)
    {
        var property = (context.Symbol as IPropertySymbol)!;
        if (property.IsIndexer)
        {
            return;
        }

        if (property.IsOverride && HasExternalPropertyOverride(property))
        {
            return;
        }

        var abbreviated = FindFirstAbbreviatedSegment(property.Name);
        if (abbreviated is null)
        {
            return;
        }

        var location = property.Locations[0];
        context.ReportDiagnostic(Diagnostic.Create(Rule, location, property.Name, abbreviated));
    }

    private void AnalyzeSingleVariableDesignation(SyntaxNodeAnalysisContext context)
    {
        var designation = (context.Node as SingleVariableDesignationSyntax)!;
        ReportIfAbbreviated(context, designation.Identifier.Text, designation.Identifier.GetLocation());
    }

    private void AnalyzeVariableDeclarator(SyntaxNodeAnalysisContext context)
    {
        var declarator = (context.Node as VariableDeclaratorSyntax)!;
        ReportIfAbbreviated(context, declarator.Identifier.Text, declarator.Identifier.GetLocation());
    }

    private string? FindFirstAbbreviatedSegment(string identifier)
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
            if (HasAbbreviatedSegment(segments[index], previousSegment, nextSegment))
            {
                return segments[index];
            }
        }

        return null;
    }

    private bool HasAbbreviatedSegment(string segment, string? previousSegment, string? nextSegment)
    {
        var isExemptSegment = AxisSegments.Contains(segment) ||
            segment.Equals("Xml", StringComparison.OrdinalIgnoreCase) ||
            (segment.Equals("T", StringComparison.OrdinalIgnoreCase) && string.Equals(previousSegment, "Of", StringComparison.OrdinalIgnoreCase)) ||
            (segment.Equals("N", StringComparison.OrdinalIgnoreCase) && string.Equals(nextSegment, "Substitute", StringComparison.OrdinalIgnoreCase));
        if (isExemptSegment)
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

    private bool HasExternalOverride(IMethodSymbol method)
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

    private bool HasExternalPropertyOverride(IPropertySymbol property)
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

    private void ReportIfAbbreviated(SyntaxNodeAnalysisContext context, string name, Location location)
    {
        var abbreviated = FindFirstAbbreviatedSegment(name);
        if (abbreviated is null)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, location, name, abbreviated));
    }

    private IEnumerable<string> SplitCamelCase(string name)
    {
        var start = 0;
        for (var index = 1; index < name.Length; index++)
        {
            var current = name[index];
            var previous = name[index - 1];
            bool startsNewSegment;
            var isNewSegmentAfterAcronym = char.IsUpper(current) && index + 1 < name.Length && char.IsLower(name[index + 1]) && char.IsUpper(previous);

            if (char.IsUpper(current) && char.IsLower(previous))
            {
                startsNewSegment = true;
            }
            else if (isNewSegmentAfterAcronym)
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
