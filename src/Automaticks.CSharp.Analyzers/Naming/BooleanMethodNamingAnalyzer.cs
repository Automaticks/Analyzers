using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.Naming;

/// <summary>
///     Flags methods and local functions that return <c>bool</c> or <c>bool?</c>
///     but whose names do not begin with an allowed prefix: <c>can</c>, <c>has</c>, or <c>is</c> (case-insensitive).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BooleanMethodNamingAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a boolean-returning method or local function
    ///     does not start with an allowed prefix.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;
    private readonly string[] AllowedPrefixes;

    static BooleanMethodNamingAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.BooleanMethodNaming,
            "Methods returning bool must use an allowed prefix",
            "'{0}' returns bool or bool? but its name does not start with an allowed prefix ('can', 'has', or 'is', case-insensitive). Rename it to start with one of those prefixes, replacing any existing prefix rather than adding to it. This convention signals intent at every call site and is required for codebase consistency. A code fix is available (dotnet format analyzers --diagnostics ATXCS063).",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "Rename the method or local function so its name begins with 'can', 'has', or 'is' (any casing). Examples: 'Validate' \u2192 'CanValidate', 'AllowRetry' \u2192 'CanRetry', 'hasAccess' is already valid. Exempt: overrides and interface implementations where renaming would break an external contract.");
        Rule = rule;
    }

    /// <summary>
    ///     Initializes the lookup tables used during analysis.
    /// </summary>
    public BooleanMethodNamingAnalyzer()
    {
        AllowedPrefixes = ["can", "has", "is"];
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeLocalFunction, SyntaxKind.LocalFunctionStatement);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeLocalFunction(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not LocalFunctionStatementSyntax localFunc)
        {
            return;
        }

        var name = localFunc.Identifier.Text;

        if (HasAllowedPrefix(name))
        {
            return;
        }

        var symbol = context.SemanticModel.GetDeclaredSymbol(localFunc) as IMethodSymbol;
        if (symbol is null)
        {
            return;
        }

        if (!HasBooleanType(symbol.ReturnType))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, localFunc.Identifier.GetLocation(), name));
    }

    private void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MethodDeclarationSyntax method)
        {
            return;
        }

        var name = method.Identifier.Text;

        if (HasAllowedPrefix(name))
        {
            return;
        }

        var symbol = context.SemanticModel.GetDeclaredSymbol(method);
        if (symbol is null)
        {
            return;
        }

        if (!HasBooleanType(symbol.ReturnType))
        {
            return;
        }

        if (symbol.IsOverride && HasExternalOverride(symbol))
        {
            return;
        }

        if (symbol.ExplicitInterfaceImplementations.Length > 0)
        {
            foreach (var ifaceMethod in symbol.ExplicitInterfaceImplementations)
            {
                if (ifaceMethod.DeclaringSyntaxReferences.IsEmpty)
                {
                    return;
                }
            }
        }

        if (HasImplicitExternalInterfaceImplementation(symbol))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, method.Identifier.GetLocation(), name));
    }

    private bool HasAllowedPrefix(string name)
    {
        foreach (var prefix in AllowedPrefixes)
        {
            if (name.Length >= prefix.Length &&
                string.Compare(name, 0, prefix, 0, prefix.Length, System.StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }
        }

        return false;
    }

    private bool HasBooleanType(ITypeSymbol type)
    {
        if (type.SpecialType == SpecialType.System_Boolean)
        {
            return true;
        }

        if (type is INamedTypeSymbol { IsValueType: true, ConstructedFrom.SpecialType: SpecialType.System_Nullable_T } namedType)
        {
            return namedType.TypeArguments.Length == 1 &&
                   namedType.TypeArguments[0].SpecialType == SpecialType.System_Boolean;
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
}
