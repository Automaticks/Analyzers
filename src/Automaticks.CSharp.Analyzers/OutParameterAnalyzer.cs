using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Automaticks.CSharp;

/// <summary>
///     Enforces two rules around <c>out</c> parameters:
///     <list type="bullet">
///         <item>
///             <description>
///                 <b>ATXCS023</b> — a method may not define more than one <c>out</c> parameter.
///             </description>
///         </item>
///         <item>
///             <description>
///                 <b>ATXCS024</b> — an <c>out</c> parameter must be the last parameter of its method.
///             </description>
///         </item>
///     </list>
///     External overrides and external interface implementations are exempt.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OutParameterAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor CountRule = new(
        DiagnosticIds.CSharp.OutParameterCount,
        "Method defines more than one out parameter",
        "'{0}' defines {1} out parameters; at most one is allowed. Consolidate the outputs into a return type or a dedicated result struct.",
        "CSharp",
        DiagnosticSeverity.Error,
        true,
        "The method defines more than one `out` parameter. Replace the multiple `out` parameters with a dedicated result `record` or `struct` as the return type. Example: change `void Parse(out int x, out string s)` to `ParseResult Parse()` where `ParseResult` is a `record` or `struct` containing `int X` and `string S`.");

    private static readonly DiagnosticDescriptor PositionRule = new(
        DiagnosticIds.CSharp.OutParameterPosition,
        "out parameter is not the last parameter",
        "The out parameter '{0}' in '{1}' must be the last parameter in the parameter list",
        "CSharp",
        DiagnosticSeverity.Error,
        true,
        "An out parameter must always appear as the last parameter. Move all out parameters to the end of the parameter list.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [CountRule, PositionRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
        context.RegisterSyntaxNodeAction(AnalyzeLocalFunction, SyntaxKind.LocalFunctionStatement);
    }

    private static void AnalyzeLocalFunction(SyntaxNodeAnalysisContext context)
    {
        var localFunction = (LocalFunctionStatementSyntax)context.Node;
        var parameters = localFunction.ParameterList.Parameters;
        var outParams = GetOutParamIndicesSyntax(parameters);
        ReportOutViolationsSyntax(context, localFunction.Identifier.Text, parameters, outParams, localFunction.Identifier.GetLocation());
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context)
    {
        var method = (IMethodSymbol)context.Symbol;

        if (method.MethodKind is not MethodKind.Ordinary)
        {
            return;
        }

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

        if (IsImplicitExternalInterfaceImplementation(method))
        {
            return;
        }

        var parameters = method.Parameters;
        ReportOutViolations(context, method.Name, parameters, method.Locations);
    }

    private static List<int> GetOutParamIndicesSyntax(SeparatedSyntaxList<ParameterSyntax> parameters)
    {
        var indices = new List<int>();
        for (var i = 0; i < parameters.Count; i++)
        {
            foreach (var modifier in parameters[i].Modifiers)
            {
                if (modifier.IsKind(SyntaxKind.OutKeyword))
                {
                    indices.Add(i);
                    break;
                }
            }
        }

        return indices;
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

    private static bool IsImplicitExternalInterfaceImplementation(IMethodSymbol method)
    {
        var containingType = method.ContainingType;
        foreach (var iface in containingType.AllInterfaces)
        {
            if (!iface.DeclaringSyntaxReferences.IsEmpty)
            {
                continue;
            }

            foreach (var member in iface.GetMembers())
            {
                if (member is not IMethodSymbol ifaceMethod)
                {
                    continue;
                }

                var impl = containingType.FindImplementationForInterfaceMember(ifaceMethod);
                if (impl is IMethodSymbol implMethod &&
                    SymbolEqualityComparer.Default.Equals(implMethod, method))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static void ReportOutViolations(
        SymbolAnalysisContext context,
        string methodName,
        ImmutableArray<IParameterSymbol> parameters,
        ImmutableArray<Location> methodLocations)
    {
        var outCount = 0;
        foreach (var parameter in parameters)
        {
            if (parameter.RefKind == RefKind.Out)
            {
                outCount++;
            }
        }

        if (outCount > 1)
        {
            var location = methodLocations.Length > 0 ? methodLocations[0] : Location.None;
            context.ReportDiagnostic(Diagnostic.Create(CountRule, location, methodName, outCount));
        }

        var lastIndex = parameters.Length - 1;
        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            if (parameter.RefKind == RefKind.Out && i != lastIndex)
            {
                var location = parameter.Locations.Length > 0 ? parameter.Locations[0] : Location.None;
                context.ReportDiagnostic(Diagnostic.Create(PositionRule, location, parameter.Name, methodName));
            }
        }
    }

    private static void ReportOutViolationsSyntax(
        SyntaxNodeAnalysisContext context,
        string methodName,
        SeparatedSyntaxList<ParameterSyntax> parameters,
        List<int> outIndices,
        Location methodLocation)
    {
        if (outIndices.Count > 1)
        {
            context.ReportDiagnostic(Diagnostic.Create(CountRule, methodLocation, methodName, outIndices.Count));
        }

        var lastIndex = parameters.Count - 1;
        foreach (var index in outIndices)
        {
            if (index != lastIndex)
            {
                var paramName = parameters[index].Identifier.Text;
                context.ReportDiagnostic(Diagnostic.Create(PositionRule, parameters[index].GetLocation(), paramName, methodName));
            }
        }
    }
}
