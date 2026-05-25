using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp;

/// <summary>
///     Flags explicit use of BCL generic delegate types: <see cref="System.Action" />,
///     <c>Action&lt;T&gt;</c>, <c>Func&lt;T&gt;</c>, <c>Predicate&lt;T&gt;</c>,
///     <c>Comparison&lt;T&gt;</c>, and <c>Converter&lt;TInput,TOutput&gt;</c>.
///     Named delegate declarations must be used instead.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class GenericDelegateAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a forbidden BCL generic delegate type is referenced.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.CSharp.GenericDelegate,
        "Generic built-in delegate types are forbidden",
        "'{0}' is a built-in generic delegate type. Declare a named delegate instead.",
        "CSharp",
        DiagnosticSeverity.Error,
        true,
        "Replace the built-in generic delegate (`Action<>`, `Func<>`, `Predicate<T>`, `Comparison<T>`, or `Converter<TIn,TOut>`) with a named delegate declaration. Example: replace a field of type `Func<int, string>` with a `public delegate string IntFormatter(int value)` declaration and use `IntFormatter` as the type. Named delegates make intent explicit.");

    private static readonly string[] ForbiddenMetadataNames =
    [
        "System.Action",
        "System.Action`1",
        "System.Action`2",
        "System.Action`3",
        "System.Action`4",
        "System.Action`5",
        "System.Action`6",
        "System.Action`7",
        "System.Action`8",
        "System.Action`9",
        "System.Action`10",
        "System.Action`11",
        "System.Action`12",
        "System.Action`13",
        "System.Action`14",
        "System.Action`15",
        "System.Action`16",
        "System.Func`1",
        "System.Func`2",
        "System.Func`3",
        "System.Func`4",
        "System.Func`5",
        "System.Func`6",
        "System.Func`7",
        "System.Func`8",
        "System.Func`9",
        "System.Func`10",
        "System.Func`11",
        "System.Func`12",
        "System.Func`13",
        "System.Func`14",
        "System.Func`15",
        "System.Func`16",
        "System.Func`17",
        "System.Predicate`1",
        "System.Comparison`1",
        "System.Converter`2"
    ];

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(RegisterPerCompilation);
    }

    private static void AnalyzeNode(
        SyntaxNodeAnalysisContext context,
        ImmutableHashSet<INamedTypeSymbol> forbiddenTypes)
    {
        var symbolInfo = context.SemanticModel.GetSymbolInfo(context.Node);
        var symbol = symbolInfo.Symbol;

        INamedTypeSymbol? namedType = null;

        if (symbol is INamedTypeSymbol directType)
        {
            namedType = directType.OriginalDefinition;
        }
        else if (symbol is IAliasSymbol { Target: INamedTypeSymbol aliasTarget })
        {
            namedType = aliasTarget.OriginalDefinition;
        }

        if (namedType is null || !forbiddenTypes.Contains(namedType))
        {
            return;
        }

        if (IsInsideExpressionTypeArgument(context.Node))
        {
            return;
        }

        if (IsInExternalInterfaceImplementation(context))
        {
            return;
        }

        var displayName = context.Node switch
        {
            GenericNameSyntax generic => generic.Identifier.Text,
            IdentifierNameSyntax identifier => identifier.Identifier.Text,
            _ => namedType.Name
        };

        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), displayName));
    }

    private static bool IsInsideExpressionTypeArgument(SyntaxNode node)
    {
        var parent = node.Parent;
        if (parent is not TypeArgumentListSyntax typeArgList)
        {
            return false;
        }

        var grandParent = typeArgList.Parent;
        if (grandParent is not GenericNameSyntax genericName)
        {
            return false;
        }

        return genericName.Identifier.Text == "Expression";
    }

    private static bool IsInExternalInterfaceImplementation(SyntaxNodeAnalysisContext context)
    {
        var methodDecl = context.Node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (methodDecl is null)
        {
            return false;
        }

        if (context.SemanticModel.GetDeclaredSymbol(methodDecl) is not IMethodSymbol methodSymbol)
        {
            return false;
        }

        if (methodSymbol.ExplicitInterfaceImplementations.Length > 0)
        {
            foreach (var ifaceMethod in methodSymbol.ExplicitInterfaceImplementations)
            {
                if (ifaceMethod.DeclaringSyntaxReferences.IsEmpty)
                {
                    return true;
                }
            }
        }

        if (methodSymbol.IsOverride)
        {
            return false;
        }

        foreach (var iface in methodSymbol.ContainingType.AllInterfaces)
        {
            foreach (var member in iface.GetMembers())
            {
                if (member is not IMethodSymbol ifaceMethod)
                {
                    continue;
                }

                if (ifaceMethod.DeclaringSyntaxReferences.IsEmpty &&
                    SymbolEqualityComparer.Default.Equals(
                        methodSymbol.ContainingType.FindImplementationForInterfaceMember(ifaceMethod),
                        methodSymbol))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static ImmutableHashSet<INamedTypeSymbol> BuildForbiddenTypeSet(Compilation compilation)    {
        var builder = ImmutableHashSet.CreateBuilder<INamedTypeSymbol>(SymbolEqualityComparer.Default);

        foreach (var metadataName in ForbiddenMetadataNames)
        {
            var type = compilation.GetTypeByMetadataName(metadataName);
            if (type is not null)
            {
                builder.Add(type);
            }
        }

        return builder.ToImmutable();
    }

    private static void RegisterPerCompilation(CompilationStartAnalysisContext compilationContext)
    {
        var forbiddenTypes = BuildForbiddenTypeSet(compilationContext.Compilation);
        if (forbiddenTypes.IsEmpty)
        {
            return;
        }

        compilationContext.RegisterSyntaxNodeAction(
            ctx => AnalyzeNode(ctx, forbiddenTypes),
            SyntaxKind.IdentifierName,
            SyntaxKind.GenericName);
    }
}
