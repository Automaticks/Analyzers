using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.LanguageFeatures;

/// <summary>
///     Flags any parameter that declares a default value in a method, constructor, local function, lambda expression, anonymous method, or indexer.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ParameterDefaultValueAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a parameter has a default value.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static ParameterDefaultValueAnalyzer()
    {
        Rule = new(
            DiagnosticIds.CSharp.ParameterDefaultValue,
            "Parameter must not have a default value",
            "Parameter '{0}' must not have a default value",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "Remove the default value from the parameter declaration and update every call site to pass the argument explicitly. Default parameter values are invisible at call sites, create hidden coupling between caller and callee, and make overload resolution ambiguous.");
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
        context.RegisterSymbolAction(AnalyzeIndexer, SymbolKind.Property);
        context.RegisterSyntaxNodeAction(AnalyzeLocalFunction, SyntaxKind.LocalFunctionStatement);
        context.RegisterSyntaxNodeAction(AnalyzeLambda, SyntaxKind.ParenthesizedLambdaExpression);
        context.RegisterSyntaxNodeAction(AnalyzeAnonymousMethod, SyntaxKind.AnonymousMethodExpression);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [Rule];
        }
    }

    private void AnalyzeAnonymousMethod(SyntaxNodeAnalysisContext context)
    {
        var anonymousMethod = (context.Node as AnonymousMethodExpressionSyntax)!;
        if (anonymousMethod.ParameterList is null)
        {
            return;
        }

        foreach (var parameter in anonymousMethod.ParameterList.Parameters)
        {
            if (parameter.Default is not null)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, parameter.GetLocation(), parameter.Identifier.Text));
            }
        }
    }

    private void AnalyzeIndexer(SymbolAnalysisContext context)
    {
        var property = (context.Symbol as IPropertySymbol)!;
        if (!property.IsIndexer)
        {
            return;
        }

        if (property.IsOverride && HasExternalIndexerOverride(property))
        {
            return;
        }

        if (HasImplicitExternalInterfaceIndexerImplementation(property))
        {
            return;
        }

        foreach (var param in property.Parameters)
        {
            if (!param.HasExplicitDefaultValue)
            {
                continue;
            }

            var location = param.Locations[0];
            context.ReportDiagnostic(Diagnostic.Create(Rule, location, param.Name));
        }
    }

    private void AnalyzeLambda(SyntaxNodeAnalysisContext context)
    {
        var lambda = (context.Node as ParenthesizedLambdaExpressionSyntax)!;
        foreach (var parameter in lambda.ParameterList.Parameters)
        {
            if (parameter.Default is not null)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, parameter.GetLocation(), parameter.Identifier.Text));
            }
        }
    }

    private void AnalyzeLocalFunction(SyntaxNodeAnalysisContext context)
    {
        var localFunction = (context.Node as LocalFunctionStatementSyntax)!;
        foreach (var parameter in localFunction.ParameterList.Parameters)
        {
            if (parameter.Default is not null)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, parameter.GetLocation(), parameter.Identifier.Text));
            }
        }
    }

    private void AnalyzeMethod(SymbolAnalysisContext context)
    {
        var method = (context.Symbol as IMethodSymbol)!;
        if (method.MethodKind is not (MethodKind.Ordinary or MethodKind.Constructor or MethodKind.ExplicitInterfaceImplementation))
        {
            return;
        }

        if (method.MethodKind != MethodKind.Constructor && HasParameterDefaultValueExemption(method))
        {
            return;
        }

        ReportParametersWithDefaultValues(context, method.Parameters);
    }

    private bool HasExternalExplicitInterfaceImplementation(IMethodSymbol method)
    {
        foreach (var ifaceMethod in method.ExplicitInterfaceImplementations)
        {
            if (ifaceMethod.DeclaringSyntaxReferences.IsEmpty)
            {
                return true;
            }
        }

        return false;
    }

    private bool HasExternalIndexerOverride(IPropertySymbol property)
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

    /// <summary>
    ///     Returns when is the implicit implementation of a method declared on an external (metadata-only) interface.
    /// </summary>
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

    /// <summary>
    ///     Returns when is the implicit implementation of an indexer declared on an external (metadata-only) interface.
    /// </summary>
    private bool HasImplicitExternalInterfaceIndexerImplementation(IPropertySymbol property)
    {
        var containingType = property.ContainingType;
        foreach (var iface in containingType.AllInterfaces)
        {
            if (!iface.DeclaringSyntaxReferences.IsEmpty)
            {
                continue;
            }

            foreach (var member in iface.GetMembers())
            {
                if (member is not IPropertySymbol { IsIndexer: true } ifaceProp)
                {
                    continue;
                }

                var impl = containingType.FindImplementationForInterfaceMember(ifaceProp);
                if (impl is IPropertySymbol implProp &&
                    SymbolEqualityComparer.Default.Equals(implProp, property))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool HasParameterDefaultValueExemption(IMethodSymbol method)
    {
        if (method.IsOverride && HasExternalOverride(method))
        {
            return true;
        }

        if (HasExternalExplicitInterfaceImplementation(method))
        {
            return true;
        }

        return method.MethodKind == MethodKind.Ordinary && HasImplicitExternalInterfaceImplementation(method);
    }

    private void ReportParametersWithDefaultValues(SymbolAnalysisContext context, ImmutableArray<IParameterSymbol> parameters)
    {
        foreach (var param in parameters)
        {
            if (!param.HasExplicitDefaultValue)
            {
                continue;
            }

            var location = param.Locations[0];
            context.ReportDiagnostic(Diagnostic.Create(Rule, location, param.Name));
        }
    }
}
