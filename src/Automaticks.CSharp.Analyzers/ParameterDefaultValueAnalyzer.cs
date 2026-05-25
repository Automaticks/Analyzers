using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp;

/// <summary>
///     Flags any parameter that declares a default value in a method, constructor, local function,
///     lambda expression, anonymous method, or indexer.
///     Callers must always supply explicit arguments — optional parameters create hidden coupling
///     and make method signatures ambiguous.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ParameterDefaultValueAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a parameter has a default value.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.CSharp.ParameterDefaultValue,
        "Parameter must not have a default value",
        "Parameter '{0}' must not have a default value",
        "CSharp",
        DiagnosticSeverity.Error,
        true,
        "Remove the default value from the parameter declaration and update every call site to pass the argument explicitly. Default parameter values are invisible at call sites, create hidden coupling between caller and callee, and make overload resolution ambiguous.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

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

    private static void AnalyzeMethod(SymbolAnalysisContext context)
    {
        var method = (IMethodSymbol)context.Symbol;

        if (method.MethodKind is not (MethodKind.Ordinary or MethodKind.Constructor or MethodKind.ExplicitInterfaceImplementation))
        {
            return;
        }

        if (method.MethodKind != MethodKind.Constructor)
        {
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

            if (method.MethodKind == MethodKind.Ordinary && IsImplicitExternalInterfaceImplementation(method))
            {
                return;
            }
        }

        foreach (var param in method.Parameters)
        {
            if (!param.HasExplicitDefaultValue)
            {
                continue;
            }

            var location = param.Locations.Length > 0 ? param.Locations[0] : Location.None;
            context.ReportDiagnostic(Diagnostic.Create(Rule, location, param.Name));
        }
    }

    private static void AnalyzeIndexer(SymbolAnalysisContext context)
    {
        var property = (IPropertySymbol)context.Symbol;

        if (!property.IsIndexer)
        {
            return;
        }

        if (property.IsOverride && IsExternalIndexerOverride(property))
        {
            return;
        }

        if (IsImplicitExternalInterfaceIndexerImplementation(property))
        {
            return;
        }

        foreach (var param in property.Parameters)
        {
            if (!param.HasExplicitDefaultValue)
            {
                continue;
            }

            var location = param.Locations.Length > 0 ? param.Locations[0] : Location.None;
            context.ReportDiagnostic(Diagnostic.Create(Rule, location, param.Name));
        }
    }

    private static void AnalyzeLocalFunction(SyntaxNodeAnalysisContext context)
    {
        var localFunction = (LocalFunctionStatementSyntax)context.Node;

        foreach (var parameter in localFunction.ParameterList.Parameters)
        {
            if (parameter.Default is not null)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, parameter.GetLocation(), parameter.Identifier.Text));
            }
        }
    }

    private static void AnalyzeLambda(SyntaxNodeAnalysisContext context)
    {
        var lambda = (ParenthesizedLambdaExpressionSyntax)context.Node;

        foreach (var parameter in lambda.ParameterList.Parameters)
        {
            if (parameter.Default is not null)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, parameter.GetLocation(), parameter.Identifier.Text));
            }
        }
    }

    private static void AnalyzeAnonymousMethod(SyntaxNodeAnalysisContext context)
    {
        var anonymousMethod = (AnonymousMethodExpressionSyntax)context.Node;

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

    private static bool IsExternalIndexerOverride(IPropertySymbol property)
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

    /// <summary>
    ///     Returns <see langword="true" /> when <paramref name="method" /> is the implicit
    ///     implementation of a method declared on an external (metadata-only) interface.
    ///     Such methods cannot be changed by the developer, so they are exempt from the rule.
    /// </summary>
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

    /// <summary>
    ///     Returns <see langword="true" /> when <paramref name="property" /> is the implicit
    ///     implementation of an indexer declared on an external (metadata-only) interface.
    ///     Such indexers cannot be changed by the developer, so they are exempt from the rule.
    /// </summary>
    private static bool IsImplicitExternalInterfaceIndexerImplementation(IPropertySymbol property)
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
}
