using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using static Automaticks.CSharp.Naming.AsyncReturnTypeHelper;

namespace Automaticks.CSharp.Naming;

/// <summary>
///     Flags methods that return a task type or <c>IAsyncEnumerable&lt;T&gt;</c> but whose name does not end with the <c>Async</c> suffix.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AsyncMethodNamingAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when an async-returning method name is missing the <c>Async</c> suffix.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static AsyncMethodNamingAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.AsyncMethodNaming,
            "Async-returning methods must use the Async suffix",
            "Method '{0}' returns an async type but does not end with 'Async'. Rename it to '{0}Async'. A code fix is available (dotnet format analyzers --diagnostics ATXCS003).",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "Append `Async` to the method name. Example: rename `GetData` to `GetDataAsync`. Exempt: constructors, `Main` entry points, test methods (detected by test framework attributes), event handlers, and `override` or explicit interface implementations where renaming would break the contract.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        var method = (context.Node as MethodDeclarationSyntax)!;
        if (method.Identifier.Text.EndsWith("Async", StringComparison.Ordinal))
        {
            return;
        }

        var symbol = context.SemanticModel.GetDeclaredSymbol(method);
        if (symbol is null)
        {
            return;
        }

        if (!HasAsyncReturnType(symbol, context.SemanticModel.Compilation))
        {
            return;
        }

        if (symbol.IsOverride && HasExternalOverride(symbol))
        {
            return;
        }

        if (symbol.ExplicitInterfaceImplementations.Length > 0 && HasExternalExplicitInterfaceImplementation(symbol))
        {
            return;
        }

        if (HasImplicitExternalInterfaceImplementation(symbol))
        {
            return;
        }

        if (symbol.IsStatic && symbol.Name == "Main")
        {
            return;
        }

        if (HasTestAttribute(symbol))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, method.Identifier.GetLocation(), method.Identifier.Text));
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
        if (method.IsOverride)
        {
            return false;
        }

        foreach (var iface in method.ContainingType.AllInterfaces)
        {
            foreach (var member in iface.GetMembers())
            {
                if (member is not IMethodSymbol ifaceMethod)
                {
                    continue;
                }

                if (ifaceMethod.DeclaringSyntaxReferences.IsEmpty &&
                    SymbolEqualityComparer.Default.Equals(
                        method.ContainingType.FindImplementationForInterfaceMember(ifaceMethod),
                        method))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool HasTestAttribute(IMethodSymbol method)
    {
        foreach (var attr in method.GetAttributes())
        {
            var name = attr.AttributeClass?.Name;
            if (name is "TestAttribute" or "FactAttribute" or "TheoryAttribute" or "TestMethodAttribute" or "ArgumentsAttribute")
            {
                return true;
            }
        }

        return false;
    }
}
