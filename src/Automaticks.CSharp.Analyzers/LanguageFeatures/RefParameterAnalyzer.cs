using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Automaticks.CSharp.LanguageFeatures;

/// <summary>
///     Enforces three rules around ref parameters: ATXCS025 — ref parameters are forbidden in any method not named SetProperty.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RefParameterAnalyzer : DiagnosticAnalyzer
{
    private const string SetPropertyName = "SetProperty";
    private static readonly DiagnosticDescriptor CountRule;
    private static readonly DiagnosticDescriptor ForbiddenRule;
    private static readonly DiagnosticDescriptor PositionRule;

    static RefParameterAnalyzer()
    {
        var countRule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.RefParameterCount,
            "Method defines more than one ref parameter",
            "'{0}' defines {1} ref parameters; at most one is allowed",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "The method has more than one `ref` parameter, which is not allowed. Replace additional `ref` parameters with a return type or a result `struct`. Note: only methods named `SetProperty` may use a `ref` parameter at all; all other methods must have zero `ref` parameters.");
        CountRule = countRule;
        var forbiddenRule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.RefParameterForbidden,
            "ref parameter is forbidden",
            "'{0}' uses a ref parameter, which is only allowed in methods named 'SetProperty'. Remove the ref modifier and redesign the method to return or output the value instead.",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "ref parameters are forbidden in all methods except those named 'SetProperty'. Return the value or use a single out parameter at the end of the parameter list.");
        ForbiddenRule = forbiddenRule;
        var positionRule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.RefParameterPosition,
            "ref parameter is not the first parameter",
            "The ref parameter '{0}' in '{1}' must be the first parameter in the parameter list",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "A ref parameter must always appear as the first parameter. Move it to the beginning of the parameter list.");
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
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [ForbiddenRule, PositionRule, CountRule];

    private void AnalyzeLocalFunction(SyntaxNodeAnalysisContext context)
    {
        var localFunction = (context.Node as LocalFunctionStatementSyntax)!;
        var parameters = localFunction.ParameterList.Parameters;
        var refIndices = GetRefParamIndicesSyntax(parameters);
        var info = new RefViolationSyntaxInfo(
            localFunction.Identifier.GetLocation(),
            localFunction.Identifier.Text,
            parameters,
            refIndices);
        ReportRefViolationsSyntax(context, info);
    }

    private void AnalyzeMethod(SymbolAnalysisContext context)
    {
        var method = (context.Symbol as IMethodSymbol)!;
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
        ReportRefViolations(context, method.Name, parameters, method.Locations);
    }

    private List<int> GetRefParamIndicesSyntax(SeparatedSyntaxList<ParameterSyntax> parameters)
    {
        var indices = new List<int>();
        for (var index = 0; index < parameters.Count; index++)
        {
            foreach (var modifier in parameters[index].Modifiers)
            {
                if (modifier.IsKind(SyntaxKind.RefKeyword))
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

    private void ReportRefViolations(
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

        var location = methodLocations[0];

        if (methodName != SetPropertyName)
        {
            context.ReportDiagnostic(Diagnostic.Create(ForbiddenRule, location, methodName));
        }

        if (refCount > 1)
        {
            context.ReportDiagnostic(Diagnostic.Create(CountRule, location, methodName, refCount));
        }

        for (var index = 0; index < parameters.Length; index++)
        {
            var parameter = parameters[index];
            if (parameter.RefKind == RefKind.Ref && index != 0)
            {
                var paramLocation = parameter.Locations[0];
                context.ReportDiagnostic(Diagnostic.Create(PositionRule, paramLocation, parameter.Name, methodName));
            }
        }
    }

    private void ReportRefViolationsSyntax(SyntaxNodeAnalysisContext context, RefViolationSyntaxInfo info)
    {
        if (info.RefIndices.Count == 0)
        {
            return;
        }

        if (info.MethodName != SetPropertyName)
        {
            context.ReportDiagnostic(Diagnostic.Create(ForbiddenRule, info.MethodLocation, info.MethodName));
        }

        if (info.RefIndices.Count > 1)
        {
            context.ReportDiagnostic(Diagnostic.Create(CountRule, info.MethodLocation, info.MethodName, info.RefIndices.Count));
        }

        foreach (var index in info.RefIndices)
        {
            if (index != 0)
            {
                var paramName = info.Parameters[index].Identifier.Text;
                context.ReportDiagnostic(Diagnostic.Create(PositionRule, info.Parameters[index].GetLocation(), paramName, info.MethodName));
            }
        }
    }

    /// <summary>
    ///     Groups the data needed to report <c>ref</c> parameter violations for a local function.
    /// </summary>
    private readonly struct RefViolationSyntaxInfo
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
        ///     The local function's declared parameters.
        /// </summary>
        public SeparatedSyntaxList<ParameterSyntax> Parameters { get; }

        /// <summary>
        ///     The indices, within <see cref="Parameters" />, of every <c>ref</c> parameter.
        /// </summary>
        public List<int> RefIndices { get; }

        public RefViolationSyntaxInfo(
            Location methodLocation,
            string methodName,
            SeparatedSyntaxList<ParameterSyntax> parameters,
            List<int> refIndices)
        {
            MethodLocation = methodLocation;
            MethodName = methodName;
            Parameters = parameters;
            RefIndices = refIndices;
        }
    }
}
