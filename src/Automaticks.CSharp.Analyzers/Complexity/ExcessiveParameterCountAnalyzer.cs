using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.Complexity;

/// <summary>
///     Flags callable constructs with more than 4 parameters, exempting constructors, operators, and external overrides or interface implementations.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ExcessiveParameterCountAnalyzer : DiagnosticAnalyzer
{
    private const int MaxParameters = 4;

    /// <summary>
    ///     The diagnostic rule reported when a callable construct exceeds the maximum number
    ///     of parameters.
    /// </summary>
    private static readonly DiagnosticDescriptor Rule;

    static ExcessiveParameterCountAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.ExcessiveParameterCount,
            "Callable construct has too many parameters",
            "'{0}' defines {1} parameters; the maximum is {2}. Reduce the parameter count by grouping related parameters into a parameter object (a struct or record with required or init members).",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "The method, local function, lambda, or indexer defines more than 4 parameters. Group related parameters into a new `record` or `struct` using required or init-only properties, and replace multiple parameters with a single options/parameter object. Example: change `void Foo(string a, int b, bool c, Guid d, DateTime e)` to `void Foo(FooOptions options)` where `FooOptions` holds the individual values.");
        Rule = rule;
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

        var paramCount = anonymousMethod.ParameterList.Parameters.Count;
        if (paramCount <= MaxParameters)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, anonymousMethod.GetLocation(), "anonymous method", paramCount, MaxParameters));
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

        var paramCount = property.Parameters.Length;
        if (paramCount <= MaxParameters)
        {
            return;
        }

        var location = property.Locations[0];
        context.ReportDiagnostic(Diagnostic.Create(Rule, location, "this", paramCount, MaxParameters));
    }

    private void AnalyzeLambda(SyntaxNodeAnalysisContext context)
    {
        var lambda = (context.Node as ParenthesizedLambdaExpressionSyntax)!;
        var paramCount = lambda.ParameterList.Parameters.Count;
        if (paramCount <= MaxParameters)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, lambda.GetLocation(), "lambda expression", paramCount, MaxParameters));
    }

    private void AnalyzeLocalFunction(SyntaxNodeAnalysisContext context)
    {
        var localFunction = (context.Node as LocalFunctionStatementSyntax)!;
        var paramCount = localFunction.ParameterList.Parameters.Count;
        if (paramCount <= MaxParameters)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, localFunction.Identifier.GetLocation(), localFunction.Identifier.Text, paramCount, MaxParameters));
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

        if (HasImplicitExternalInterfaceImplementation(method))
        {
            return;
        }

        var paramCount = method.Parameters.Length;
        if (paramCount <= MaxParameters)
        {
            return;
        }

        var location = method.Locations[0];
        context.ReportDiagnostic(Diagnostic.Create(Rule, location, method.Name, paramCount, MaxParameters));
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
}
