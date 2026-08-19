using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Automaticks.CSharp.LanguageFeatures;

/// <summary>
///     Enforces two rules around out parameters: ATXCS023 — a method may not define more than one out parameter.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OutParameterAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor CountRule;
    private static readonly DiagnosticDescriptor PositionRule;

    static OutParameterAnalyzer()
    {
        var countRule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.OutParameterCount,
            "Method defines more than one out parameter",
            "'{0}' defines {1} out parameters; at most one is allowed. Consolidate the outputs into a return type or a dedicated result struct.",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "The method defines more than one `out` parameter. Replace the multiple `out` parameters with a dedicated result `record` or `struct` as the return type. Example: change `void Parse(out int x, out string s)` to `ParseResult Parse()` where `ParseResult` is a `record` or `struct` containing `int X` and `string S`.");
        CountRule = countRule;
        var positionRule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.OutParameterPosition,
            "out parameter is not the last parameter",
            "The out parameter '{0}' in '{1}' must be the last parameter in the parameter list",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "An out parameter must always appear as the last parameter. Move all out parameters to the end of the parameter list.");
        PositionRule = positionRule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
        context.RegisterSyntaxNodeAction(AnalyzeLocalFunction, SyntaxKind.LocalFunctionStatement);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [CountRule, PositionRule];

    private void AnalyzeLocalFunction(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not LocalFunctionStatementSyntax localFunction)
        {
            return;
        }

        var parameters = localFunction.ParameterList.Parameters;
        var outParams = GetOutParamIndicesSyntax(parameters);
        var info = new OutViolationSyntaxInfo(
            localFunction.Identifier.GetLocation(),
            localFunction.Identifier.Text,
            outParams,
            parameters);
        ReportOutViolationsSyntax(context, info);
    }

    private void AnalyzeMethod(SymbolAnalysisContext context)
    {
        if (context.Symbol is not IMethodSymbol method)
        {
            return;
        }

        if (method.MethodKind is not MethodKind.Ordinary)
        {
            return;
        }

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

        if (HasImplicitExternalInterfaceImplementation(method))
        {
            return;
        }

        var parameters = method.Parameters;
        ReportOutViolations(context, method.Name, parameters, method.Locations);
    }

    private List<int> GetOutParamIndicesSyntax(SeparatedSyntaxList<ParameterSyntax> parameters)
    {
        var indices = new List<int>();
        for (var index = 0; index < parameters.Count; index++)
        {
            foreach (var modifier in parameters[index].Modifiers)
            {
                if (modifier.IsKind(SyntaxKind.OutKeyword))
                {
                    indices.Add(index);
                    break;
                }
            }
        }

        return indices;
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

    private bool HasImplicitExternalInterfaceImplementation(IMethodSymbol method)
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

    private void ReportOutViolations(
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
        for (var index = 0; index < parameters.Length; index++)
        {
            var parameter = parameters[index];
            if (parameter.RefKind == RefKind.Out && index != lastIndex)
            {
                var location = parameter.Locations.Length > 0 ? parameter.Locations[0] : Location.None;
                context.ReportDiagnostic(Diagnostic.Create(PositionRule, location, parameter.Name, methodName));
            }
        }
    }

    private void ReportOutViolationsSyntax(SyntaxNodeAnalysisContext context, OutViolationSyntaxInfo info)
    {
        if (info.OutIndices.Count > 1)
        {
            context.ReportDiagnostic(Diagnostic.Create(CountRule, info.MethodLocation, info.MethodName, info.OutIndices.Count));
        }

        var lastIndex = info.Parameters.Count - 1;
        foreach (var index in info.OutIndices)
        {
            if (index != lastIndex)
            {
                var paramName = info.Parameters[index].Identifier.Text;
                context.ReportDiagnostic(Diagnostic.Create(PositionRule, info.Parameters[index].GetLocation(), paramName, info.MethodName));
            }
        }
    }

    /// <summary>
    ///     Groups the data needed to report <c>out</c> parameter violations for a local function.
    /// </summary>
    private readonly struct OutViolationSyntaxInfo
    {
        /// <summary>
        ///     The location used to anchor method-level diagnostics.
        /// </summary>
        public Location MethodLocation { get; }

        /// <summary>
        ///     The name of the local function being analyzed.
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        ///     The indices, within <see cref="Parameters" />, of every <c>out</c> parameter.
        /// </summary>
        public List<int> OutIndices { get; }

        /// <summary>
        ///     The local function's declared parameters.
        /// </summary>
        public SeparatedSyntaxList<ParameterSyntax> Parameters { get; }

        public OutViolationSyntaxInfo(
            Location methodLocation,
            string methodName,
            List<int> outIndices,
            SeparatedSyntaxList<ParameterSyntax> parameters)
        {
            MethodLocation = methodLocation;
            MethodName = methodName;
            OutIndices = outIndices;
            Parameters = parameters;
        }
    }
}
