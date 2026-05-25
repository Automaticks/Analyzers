using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Automaticks.CSharp;

/// <summary>
///     Enforces three rules around <c>ref</c> parameters:
///     <list type="bullet">
///         <item>
///             <description>
///                 <b>ATXCS025</b> — <c>ref</c> parameters are forbidden in any method not named <c>SetProperty</c>.
///             </description>
///         </item>
///         <item>
///             <description>
///                 <b>ATXCS026</b> — a <c>ref</c> parameter must be the first parameter of its method.
///             </description>
///         </item>
///         <item>
///             <description>
///                 <b>ATXCS027</b> — a method may not define more than one <c>ref</c> parameter.
///             </description>
///         </item>
///     </list>
///     External overrides and external interface implementations are exempt.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RefParameterAnalyzer : DiagnosticAnalyzer
{
    private const string SetPropertyName = "SetProperty";

    private static readonly DiagnosticDescriptor CountRule = new(
        DiagnosticIds.CSharp.RefParameterCount,
        "Method defines more than one ref parameter",
        "'{0}' defines {1} ref parameters; at most one is allowed",
        "CSharp",
        DiagnosticSeverity.Error,
        true,
        "The method has more than one `ref` parameter, which is not allowed. Replace additional `ref` parameters with a return type or a result `struct`. Note: only methods named `SetProperty` may use a `ref` parameter at all; all other methods must have zero `ref` parameters.");

    private static readonly DiagnosticDescriptor ForbiddenRule = new(
        DiagnosticIds.CSharp.RefParameterForbidden,
        "ref parameter is forbidden",
        "'{0}' uses a ref parameter, which is only allowed in methods named 'SetProperty'. Remove the ref modifier and redesign the method to return or output the value instead.",
        "CSharp",
        DiagnosticSeverity.Error,
        true,
        "ref parameters are forbidden in all methods except those named 'SetProperty'. Return the value or use a single out parameter at the end of the parameter list.");

    private static readonly DiagnosticDescriptor PositionRule = new(
        DiagnosticIds.CSharp.RefParameterPosition,
        "ref parameter is not the first parameter",
        "The ref parameter '{0}' in '{1}' must be the first parameter in the parameter list",
        "CSharp",
        DiagnosticSeverity.Error,
        true,
        "A ref parameter must always appear as the first parameter. Move it to the beginning of the parameter list.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [ForbiddenRule, PositionRule, CountRule];

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
        var refIndices = GetRefParamIndicesSyntax(parameters);
        ReportRefViolationsSyntax(context, localFunction.Identifier.Text, parameters, refIndices, localFunction.Identifier.GetLocation());
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
        ReportRefViolations(context, method.Name, parameters, method.Locations);
    }

    private static List<int> GetRefParamIndicesSyntax(SeparatedSyntaxList<ParameterSyntax> parameters)
    {
        var indices = new List<int>();
        for (var i = 0; i < parameters.Count; i++)
        {
            foreach (var modifier in parameters[i].Modifiers)
            {
                if (modifier.IsKind(SyntaxKind.RefKeyword))
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

    private static void ReportRefViolations(
        SymbolAnalysisContext context,
        string methodName,
        ImmutableArray<IParameterSymbol> parameters,
        ImmutableArray<Location> methodLocations)
    {
        var refCount = 0;
        foreach (var parameter in parameters)
        {
            if (parameter.RefKind == RefKind.Ref)
            {
                refCount++;
            }
        }

        if (refCount == 0)
        {
            return;
        }

        var location = methodLocations.Length > 0 ? methodLocations[0] : Location.None;

        if (methodName != SetPropertyName)
        {
            context.ReportDiagnostic(Diagnostic.Create(ForbiddenRule, location, methodName));
        }

        if (refCount > 1)
        {
            context.ReportDiagnostic(Diagnostic.Create(CountRule, location, methodName, refCount));
        }

        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            if (parameter.RefKind == RefKind.Ref && i != 0)
            {
                var paramLocation = parameter.Locations.Length > 0 ? parameter.Locations[0] : Location.None;
                context.ReportDiagnostic(Diagnostic.Create(PositionRule, paramLocation, parameter.Name, methodName));
            }
        }
    }

    private static void ReportRefViolationsSyntax(
        SyntaxNodeAnalysisContext context,
        string methodName,
        SeparatedSyntaxList<ParameterSyntax> parameters,
        List<int> refIndices,
        Location methodLocation)
    {
        if (refIndices.Count == 0)
        {
            return;
        }

        if (methodName != SetPropertyName)
        {
            context.ReportDiagnostic(Diagnostic.Create(ForbiddenRule, methodLocation, methodName));
        }

        if (refIndices.Count > 1)
        {
            context.ReportDiagnostic(Diagnostic.Create(CountRule, methodLocation, methodName, refIndices.Count));
        }

        foreach (var index in refIndices)
        {
            if (index != 0)
            {
                var paramName = parameters[index].Identifier.Text;
                context.ReportDiagnostic(Diagnostic.Create(PositionRule, parameters[index].GetLocation(), paramName, methodName));
            }
        }
    }
}
