using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Automaticks.CSharp.LanguageFeatures;

/// <summary>
///     Flags explicit use of built-in BCL generic delegate types (Action, Func, Predicate, Comparison, Converter).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class GenericDelegateAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a forbidden BCL generic delegate type is referenced.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;
    private static readonly ImmutableArray<string> ForbiddenMetadataNames;
    private static readonly ImmutableHashSet<string> ForbiddenSimpleNames;

    static GenericDelegateAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.GenericDelegate,
            "Generic built-in delegate types are forbidden",
            "'{0}' is a built-in generic delegate type. Declare a named delegate instead.",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "Replace the built-in generic delegate (`Action<>`, `Func<>`, `Predicate<T>`, `Comparison<T>`, or `Converter<TIn,TOut>`) with a named delegate declaration. Example: replace a field of type `Func<int, string>` with a `public delegate string IntFormatter(int value)` declaration and use `IntFormatter` as the type. Named delegates make intent explicit.");
        Rule = rule;

        var forbiddenSimpleNames = ImmutableHashSet.CreateBuilder<string>(StringComparer.Ordinal);
        forbiddenSimpleNames.Add("Action");
        forbiddenSimpleNames.Add("Comparison");
        forbiddenSimpleNames.Add("Converter");
        forbiddenSimpleNames.Add("Func");
        forbiddenSimpleNames.Add("Predicate");
        ForbiddenSimpleNames = forbiddenSimpleNames.ToImmutable();
        var forbiddenMetadataNames = new[]
        {
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
        };
        ForbiddenMetadataNames = forbiddenMetadataNames.ToImmutableArray();
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(RegisterPerCompilation);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [Rule];
        }
    }

    private void AnalyzeNode(
        SyntaxNodeAnalysisContext context,
        ImmutableHashSet<INamedTypeSymbol> forbiddenTypes,
        ConcurrentDictionary<SyntaxTree, bool> aliasTrees)
    {
        if (!ForbiddenSimpleNames.Contains(GetSimpleName(context.Node))
            && !HasUsingAlias(context.Node.SyntaxTree, aliasTrees))
        {
            return;
        }

        if (context.Node.IsPartOfStructuredTrivia())
        {
            return;
        }

        var symbolInfo = context.SemanticModel.GetSymbolInfo(context.Node);

        if (symbolInfo.Symbol is not INamedTypeSymbol directType)
        {
            return;
        }

        var namedType = directType.OriginalDefinition;

        if (!forbiddenTypes.Contains(namedType))
        {
            return;
        }

        if (HasExpressionTypeArgumentAncestor(context.Node))
        {
            return;
        }

        if (HasExternalInterfaceImplementation(context))
        {
            return;
        }

        var displayName = GetSimpleName(context.Node);

        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), displayName));
    }

    private ImmutableHashSet<INamedTypeSymbol> BuildForbiddenTypeSet(Compilation compilation)
    {
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

    private string GetSimpleName(SyntaxNode node)
    {
        if (node is GenericNameSyntax generic)
        {
            return generic.Identifier.ValueText;
        }

        return (node as IdentifierNameSyntax)!.Identifier.ValueText;
    }

    private bool HasAliasInTree(SyntaxTree tree)
    {
        var root = tree.GetRoot();

        foreach (var node in root.DescendantNodes(candidate => candidate is CompilationUnitSyntax or BaseNamespaceDeclarationSyntax))
        {
            if (node is UsingDirectiveSyntax { Alias: not null })
            {
                return true;
            }
        }

        return false;
    }

    private bool HasExpressionTypeArgumentAncestor(SyntaxNode node)
    {
        var parent = node.Parent;
        if (parent is not TypeArgumentListSyntax typeArgumentList)
        {
            return false;
        }

        var grandParent = (typeArgumentList.Parent as GenericNameSyntax)!;
        return grandParent.Identifier.Text == "Expression";
    }

    private bool HasExternalExplicitInterfaceImplementation(IMethodSymbol methodSymbol)
    {
        foreach (var interfaceMethod in methodSymbol.ExplicitInterfaceImplementations)
        {
            if (interfaceMethod.DeclaringSyntaxReferences.IsEmpty)
            {
                return true;
            }
        }

        return false;
    }

    private bool HasExternalImplicitInterfaceImplementation(IMethodSymbol methodSymbol)
    {
        foreach (var interfaceType in methodSymbol.ContainingType.AllInterfaces)
        {
            foreach (var member in interfaceType.GetMembers())
            {
                if (member is not IMethodSymbol interfaceMethod)
                {
                    continue;
                }

                if (interfaceMethod.DeclaringSyntaxReferences.IsEmpty &&
                    SymbolEqualityComparer.Default.Equals(
                        methodSymbol.ContainingType.FindImplementationForInterfaceMember(interfaceMethod),
                        methodSymbol))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool HasExternalInterfaceImplementation(SyntaxNodeAnalysisContext context)
    {
        var methodDeclaration = context.Node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (methodDeclaration is null)
        {
            return false;
        }

        var methodSymbol = (context.SemanticModel.GetDeclaredSymbol(methodDeclaration) as IMethodSymbol)!;
        if (HasExternalExplicitInterfaceImplementation(methodSymbol))
        {
            return true;
        }

        if (methodSymbol.IsOverride)
        {
            return false;
        }

        return HasExternalImplicitInterfaceImplementation(methodSymbol);
    }

    private bool HasUsingAlias(SyntaxTree tree, ConcurrentDictionary<SyntaxTree, bool> aliasTrees)
    {
        if (aliasTrees.TryGetValue(tree, out var hasAlias))
        {
            return hasAlias;
        }

        hasAlias = HasAliasInTree(tree);
        aliasTrees[tree] = hasAlias;
        return hasAlias;
    }

    private void RegisterPerCompilation(CompilationStartAnalysisContext compilationContext)
    {
        var forbiddenTypes = BuildForbiddenTypeSet(compilationContext.Compilation);
        var aliasTrees = new ConcurrentDictionary<SyntaxTree, bool>();
        compilationContext.RegisterSyntaxNodeAction(
            context => AnalyzeNode(context, forbiddenTypes, aliasTrees),
            SyntaxKind.IdentifierName,
            SyntaxKind.GenericName);
    }
}
