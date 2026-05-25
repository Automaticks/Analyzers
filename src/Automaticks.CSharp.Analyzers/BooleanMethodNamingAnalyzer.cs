using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp;

/// <summary>
///     Flags methods and local functions that return <c>bool</c> or <c>bool?</c>
///     but whose names do not begin with an allowed prefix: <c>can</c> or <c>has</c> (case-insensitive).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BooleanMethodNamingAnalyzer : DiagnosticAnalyzer
{
    private static readonly string[] AllowedPrefixes = ["can", "has"];

    /// <summary>
    ///     The diagnostic rule reported when a boolean-returning method or local function
    ///     does not start with an allowed prefix.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.CSharp.BooleanMethodNaming,
        "Methods returning bool must use an allowed prefix",
        "'{0}' returns bool or bool? but its name does not start with an allowed prefix ('can' or 'has', case-insensitive). Rename it to start with one of those prefixes (e.g. '{0}' \u2192 'Can{0}' or 'Has{0}'). This convention signals intent at every call site and is required for codebase consistency.",
        "CSharp",
        DiagnosticSeverity.Error,
        true,
        "Rename the method or local function so its name begins with 'can' or 'has' (any casing). Examples: 'Validate' \u2192 'CanValidate', 'hasAccess' is already valid. Exempt: overrides and interface implementations where renaming would break an external contract.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeLocalFunction, SyntaxKind.LocalFunctionStatement);
    }

    private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        var method = (MethodDeclarationSyntax)context.Node;
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

        if (!IsBooleanType(symbol.ReturnType))
        {
            return;
        }

        if (symbol.IsOverride && IsExternalMethodOverride(symbol))
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

        if (IsImplicitExternalInterfaceImplementation(symbol))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, method.Identifier.GetLocation(), name));
    }

    private static void AnalyzeLocalFunction(SyntaxNodeAnalysisContext context)
    {
        var localFunc = (LocalFunctionStatementSyntax)context.Node;
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

        if (!IsBooleanType(symbol.ReturnType))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, localFunc.Identifier.GetLocation(), name));
    }

    private static bool HasAllowedPrefix(string name)
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

    private static bool IsBooleanType(ITypeSymbol type)
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

    private static bool IsExternalMethodOverride(IMethodSymbol method)
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
