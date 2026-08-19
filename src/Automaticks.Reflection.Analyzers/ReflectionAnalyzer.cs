using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Automaticks.Reflection;

/// <summary>
///     Flags usage of reflection APIs — System.Reflection.* types, reflection-related methods on System.Type, and Activator.CreateInstance(Type, ...) non-...
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReflectionAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableHashSet<string> BannedReflectionTypeNames;
    private static readonly ImmutableHashSet<string> BannedTypeMethodNames;
    private static readonly DiagnosticDescriptor Rule;

    static ReflectionAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.Reflection.ReflectionUsage,
            "Reflection is forbidden",
            "Use of reflection ('{0}') is forbidden. Auto-exempted only in IServiceCollection extension methods and DispatchProxy subclasses.",
            "Reflection",
            DiagnosticSeverity.Error,
            true,
            "Remove the reflective API call and redesign using dependency injection interfaces, compile-time generics, or source generators. Reflection bypasses static type safety, breaks ahead-of-time compilation, and complicates trimming. Auto-exemptions: reflection inside `IServiceCollection` extension methods and `DispatchProxy` subclasses is allowed.");
        Rule = rule;
        var bannedReflectionTypeNames = ImmutableHashSet.CreateBuilder<string>(StringComparer.Ordinal);
        bannedReflectionTypeNames.Add("Assembly");
        bannedReflectionTypeNames.Add("BindingFlags");
        bannedReflectionTypeNames.Add("ConstructorInfo");
        bannedReflectionTypeNames.Add("EventInfo");
        bannedReflectionTypeNames.Add("FieldInfo");
        bannedReflectionTypeNames.Add("MemberInfo");
        bannedReflectionTypeNames.Add("MethodBase");
        bannedReflectionTypeNames.Add("MethodInfo");
        bannedReflectionTypeNames.Add("ParameterInfo");
        bannedReflectionTypeNames.Add("PropertyInfo");
        bannedReflectionTypeNames.Add("TypeInfo");
        BannedReflectionTypeNames = bannedReflectionTypeNames.ToImmutable();
        var bannedTypeMethodNames = ImmutableHashSet.CreateBuilder<string>(StringComparer.Ordinal);
        bannedTypeMethodNames.Add("GetConstructor");
        bannedTypeMethodNames.Add("GetConstructors");
        bannedTypeMethodNames.Add("GetEvent");
        bannedTypeMethodNames.Add("GetEvents");
        bannedTypeMethodNames.Add("GetField");
        bannedTypeMethodNames.Add("GetFields");
        bannedTypeMethodNames.Add("GetGenericArguments");
        bannedTypeMethodNames.Add("GetGenericTypeDefinition");
        bannedTypeMethodNames.Add("GetInterface");
        bannedTypeMethodNames.Add("GetInterfaces");
        bannedTypeMethodNames.Add("GetMember");
        bannedTypeMethodNames.Add("GetMembers");
        bannedTypeMethodNames.Add("GetMethod");
        bannedTypeMethodNames.Add("GetMethods");
        bannedTypeMethodNames.Add("GetProperty");
        bannedTypeMethodNames.Add("GetProperties");
        bannedTypeMethodNames.Add("GetTypeInfo");
        bannedTypeMethodNames.Add("MakeGenericType");
        BannedTypeMethodNames = bannedTypeMethodNames.ToImmutable();
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(RegisterPerCompilation);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeIdentifier(SyntaxNodeAnalysisContext context, CompilationSymbols symbols, ConcurrentDictionary<SyntaxTree, bool> aliasTrees)
    {
        var identifier = (context.Node as IdentifierNameSyntax)!;

        if (!BannedReflectionTypeNames.Contains(identifier.Identifier.ValueText)
            && !HasUsingAlias(context.Node.SyntaxTree, aliasTrees))
        {
            return;
        }

        if (context.SemanticModel.GetSymbolInfo(identifier).Symbol is not ITypeSymbol typeSymbol)
        {
            return;
        }

        if (!BannedReflectionTypeNames.Contains(typeSymbol.Name))
        {
            return;
        }

        var namespaceName = typeSymbol.ContainingNamespace!.ToDisplayString();
        if (!namespaceName.StartsWith("System.Reflection", StringComparison.Ordinal))
        {
            return;
        }

        if (HasExemptContext(context, symbols))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, identifier.GetLocation(), typeSymbol.Name));
    }

    private void AnalyzeInvocation(SyntaxNodeAnalysisContext context, CompilationSymbols symbols)
    {
        var invocation = (context.Node as InvocationExpressionSyntax)!;

        if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        if (HasReflectionNamespaceMethod(methodSymbol))
        {
            if (!HasExemptContext(context, symbols))
            {
                ReportDiagnostic(context, invocation, $"{methodSymbol.ContainingType.Name}.{methodSymbol.Name}");
            }

            return;
        }

        if (HasBannedTypeMethod(methodSymbol))
        {
            if (!HasExemptContext(context, symbols))
            {
                ReportDiagnostic(context, invocation, $"Type.{methodSymbol.Name}");
            }

            return;
        }

        if (HasBannedActivatorCall(methodSymbol) && !HasExemptContext(context, symbols))
        {
            ReportDiagnostic(context, invocation, "Activator.CreateInstance");
        }
    }

    private bool HasAliasInTree(SyntaxTree tree)
    {
        var root = tree.GetRoot();

        foreach (var node in root.DescendantNodes(node => node is CompilationUnitSyntax or BaseNamespaceDeclarationSyntax))
        {
            if (node is UsingDirectiveSyntax { Alias: not null })
            {
                return true;
            }
        }

        return false;
    }

    private bool HasBannedActivatorCall(IMethodSymbol methodSymbol)
    {
        if (methodSymbol.IsGenericMethod || methodSymbol.Parameters.Length == 0)
        {
            return false;
        }

        var containingType = methodSymbol.ContainingType;
        var namespaceName = containingType.ContainingNamespace!.ToDisplayString();
        if (namespaceName != "System" || containingType.Name != "Activator" || methodSymbol.Name != "CreateInstance")
        {
            return false;
        }

        return methodSymbol.Parameters[0].Type.Name == "Type";
    }

    private bool HasBannedTypeMethod(IMethodSymbol methodSymbol)
    {
        var containingType = methodSymbol.ContainingType;
        var namespaceName = containingType.ContainingNamespace!.ToDisplayString();
        return namespaceName == "System"
               && containingType.Name == "Type"
               && BannedTypeMethodNames.Contains(methodSymbol.Name);
    }

    private bool HasDispatchProxyHelperClassContext(IMethodSymbol methodSymbol, CompilationSymbols symbols)
    {
        if (!methodSymbol.ContainingType.IsStatic)
        {
            return false;
        }

        foreach (var parameter in methodSymbol.Parameters)
        {
            if (SymbolEqualityComparer.Default.Equals(parameter.Type.OriginalDefinition, symbols.MethodInfoType))
            {
                return true;
            }
        }

        return false;
    }

    private bool HasDispatchProxySubclassContext(SyntaxNodeAnalysisContext context, CompilationSymbols symbols)
    {
        var classDeclaration = context.Node.FirstAncestorOrSelf<ClassDeclarationSyntax>();
        if (classDeclaration is null)
        {
            return false;
        }

        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration)!;
        var baseType = classSymbol.BaseType;
        while (baseType is not null)
        {
            if (SymbolEqualityComparer.Default.Equals(baseType.OriginalDefinition, symbols.DispatchProxyType))
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    private bool HasExemptContext(SyntaxNodeAnalysisContext context, CompilationSymbols symbols)
    {
        if (HasDispatchProxySubclassContext(context, symbols))
        {
            return true;
        }

        var methodDeclaration = context.Node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (methodDeclaration is null)
        {
            return false;
        }

        var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration)!;
        return HasDispatchProxyHelperClassContext(methodSymbol, symbols)
               || HasServiceCollectionExtensionContext(methodSymbol, symbols)
               || HasTypeExtensionClassContext(methodSymbol, symbols);
    }

    private bool HasReflectionNamespaceMethod(IMethodSymbol methodSymbol)
    {
        var namespaceName = methodSymbol.ContainingType.ContainingNamespace!.ToDisplayString();
        return namespaceName.StartsWith("System.Reflection", StringComparison.Ordinal);
    }

    private bool HasServiceCollectionExtensionContext(IMethodSymbol methodSymbol, CompilationSymbols symbols)
    {
        if (symbols.ServiceCollectionType is null)
        {
            return false;
        }

        if (!methodSymbol.ContainingType.IsStatic)
        {
            return false;
        }

        foreach (var parameter in methodSymbol.Parameters)
        {
            if (SymbolEqualityComparer.Default.Equals(parameter.Type.OriginalDefinition, symbols.ServiceCollectionType))
            {
                return true;
            }
        }

        return false;
    }

    private bool HasTypeExtensionClassContext(IMethodSymbol methodSymbol, CompilationSymbols symbols)
    {
        if (!methodSymbol.IsExtensionMethod)
        {
            return false;
        }

        return SymbolEqualityComparer.Default.Equals(methodSymbol.Parameters[0].Type.OriginalDefinition, symbols.SystemTypeSymbol);
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
        var symbols = new CompilationSymbols(
            compilationContext.Compilation.GetTypeByMetadataName("Microsoft.Extensions.DependencyInjection.IServiceCollection"),
            compilationContext.Compilation.GetTypeByMetadataName("System.Reflection.DispatchProxy"),
            compilationContext.Compilation.GetTypeByMetadataName("System.Reflection.MethodInfo"),
            compilationContext.Compilation.GetTypeByMetadataName("System.Type"));
        var aliasTrees = new ConcurrentDictionary<SyntaxTree, bool>();
        compilationContext.RegisterSyntaxNodeAction(
            context => AnalyzeIdentifier(context, symbols, aliasTrees),
            SyntaxKind.IdentifierName);
        compilationContext.RegisterSyntaxNodeAction(
            context => AnalyzeInvocation(context, symbols),
            SyntaxKind.InvocationExpression);
    }

    private void ReportDiagnostic(SyntaxNodeAnalysisContext context, SyntaxNode node, string reflectionItem)
    {
        context.ReportDiagnostic(Diagnostic.Create(Rule, node.GetLocation(), reflectionItem));
    }

    private readonly struct CompilationSymbols
    {
        public INamedTypeSymbol? DispatchProxyType { get; }

        public INamedTypeSymbol? MethodInfoType { get; }

        public INamedTypeSymbol? ServiceCollectionType { get; }

        public INamedTypeSymbol? SystemTypeSymbol { get; }

        public CompilationSymbols(
            INamedTypeSymbol? serviceCollectionType,
            INamedTypeSymbol? dispatchProxyType,
            INamedTypeSymbol? methodInfoType,
            INamedTypeSymbol? systemTypeSymbol)
        {
            ServiceCollectionType = serviceCollectionType;
            DispatchProxyType = dispatchProxyType;
            MethodInfoType = methodInfoType;
            SystemTypeSymbol = systemTypeSymbol;
        }
    }
}
