using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp;

/// <summary>
///     Flags any callable construct — ordinary methods, local functions, lambda expressions,
///     anonymous methods, and indexers — that defines more than 4 parameters.
///     Constructors, operators, overrides of external APIs, and both explicit and implicit
///     implementations of methods from external (third-party) interfaces are all exempt.
/// </summary>
/// <remarks>
///     When a violation is detected, consolidate the excess parameters into a dedicated
///     parameter object.  Prefer a <c>record</c> or <c>struct</c> with <c>required</c>
///     and/or <c>init</c>-only members so that all required data must be supplied at
///     construction time.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ExcessiveParameterCountAnalyzer : DiagnosticAnalyzer
{
    private const int MaxParameters = 4;

    /// <summary>
    ///     The diagnostic rule reported when a callable construct exceeds the maximum number
    ///     of parameters.
    /// </summary>
    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.CSharp.ExcessiveParameterCount,
        "Callable construct has too many parameters",
        "'{0}' defines {1} parameters; the maximum is {2}. Reduce the parameter count by grouping related parameters into a parameter object (a struct or record with required or init members).",
        "CSharp",
        DiagnosticSeverity.Error,
        true,
        "The method, local function, lambda, or indexer defines more than 4 parameters. Group related parameters into a new `record` or `struct` using required or init-only properties, and replace multiple parameters with a single options/parameter object. Example: change `void Foo(string a, int b, bool c, Guid d, DateTime e)` to `void Foo(FooOptions options)` where `FooOptions` holds the individual values.");

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

    private static void AnalyzeAnonymousMethod(SyntaxNodeAnalysisContext context)
    {
        var anonymousMethod = (AnonymousMethodExpressionSyntax)context.Node;
        if (anonymousMethod.ParameterList is null)
        {
            return;
        }

        var paramCount = anonymousMethod.ParameterList.Parameters.Count;
        if (paramCount <= MaxParameters)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, anonymousMethod.GetLocation(), "anonymous method", paramCount, MaxParameters));
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

        var paramCount = property.Parameters.Length;
        if (paramCount <= MaxParameters)
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

        context.ReportDiagnostic(Diagnostic.Create(Rule, location, "this", paramCount, MaxParameters));
    }

    private static void AnalyzeLambda(SyntaxNodeAnalysisContext context)
    {
        var lambda = (ParenthesizedLambdaExpressionSyntax)context.Node;
        var paramCount = lambda.ParameterList.Parameters.Count;
        if (paramCount <= MaxParameters)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, lambda.GetLocation(), "lambda expression", paramCount, MaxParameters));
    }

    private static void AnalyzeLocalFunction(SyntaxNodeAnalysisContext context)
    {
        var localFunction = (LocalFunctionStatementSyntax)context.Node;
        var paramCount = localFunction.ParameterList.Parameters.Count;
        if (paramCount <= MaxParameters)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, localFunction.Identifier.GetLocation(), localFunction.Identifier.Text, paramCount, MaxParameters));
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

        var paramCount = method.Parameters.Length;
        if (paramCount <= MaxParameters)
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

        context.ReportDiagnostic(Diagnostic.Create(Rule, location, method.Name, paramCount, MaxParameters));
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
