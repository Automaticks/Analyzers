using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace Automaticks.Reflection;

/// <summary>
///     Flags usage of reflection APIs — <c>System.Reflection.*</c> types, reflection-related methods on
///     <c>System.Type</c>, and <c>Activator.CreateInstance(Type, ...)</c> non-generic overloads.
///     Auto-exempted in <c>IServiceCollection</c> extension methods and <c>DispatchProxy</c> subclasses.
///     Enforced in all projects (production, test, and analyzer).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReflectionAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableHashSet<string> BannedReflectionTypeNames;
    private static readonly ImmutableHashSet<string> BannedTypeMethodNames;
    private static readonly DiagnosticDescriptor Rule;

    static ReflectionAnalyzer()
    {
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

        var rule = new DiagnosticDescriptor(
            DiagnosticIds.Reflection.ReflectionUsage,
            "Reflection is forbidden",
            "Use of reflection ('{0}') is forbidden. Auto-exempted only in IServiceCollection extension methods and DispatchProxy subclasses.",
            "Reflection",
            DiagnosticSeverity.Error,
            true,
            "Remove the reflective API call and redesign using dependency injection interfaces, compile-time generics, or source generators. Reflection bypasses static type safety, breaks ahead-of-time compilation, and complicates trimming. Auto-exemptions: reflection inside `IServiceCollection` extension methods and `DispatchProxy` subclasses is allowed.");
        Rule = rule;
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

    private void AnalyzeIdentifier(SyntaxNodeAnalysisContext context, CompilationSymbols symbols)
    {
        if (HasExemptContext(context, symbols))
        {
            return;
        }

        if (context.Node is not IdentifierNameSyntax identifier)
        {
            return;
        }

        if (context.SemanticModel.GetSymbolInfo(identifier).Symbol is not ITypeSymbol typeSymbol)
        {
            return;
        }

        var namespaceName = typeSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        if (!namespaceName.StartsWith("System.Reflection", StringComparison.Ordinal))
        {
            return;
        }

        if (BannedReflectionTypeNames.Contains(typeSymbol.Name))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, identifier.GetLocation(), typeSymbol.Name));
        }
    }

    private void AnalyzeInvocation(SyntaxNodeAnalysisContext context, CompilationSymbols symbols)
    {
        if (HasExemptContext(context, symbols))
        {
            return;
        }

        if (context.Node is not InvocationExpressionSyntax invocation)
        {
            return;
        }

        if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        if (HasReflectionNamespaceMethod(methodSymbol))
        {
            ReportDiagnostic(context, invocation, $"{methodSymbol.ContainingType.Name}.{methodSymbol.Name}");
            return;
        }

        if (HasBannedTypeMethod(methodSymbol))
        {
            ReportDiagnostic(context, invocation, $"Type.{methodSymbol.Name}");
            return;
        }

        if (HasBannedActivatorCall(methodSymbol))
        {
            ReportDiagnostic(context, invocation, "Activator.CreateInstance");
        }
    }

    private bool HasBannedActivatorCall(IMethodSymbol methodSymbol)
    {
        if (methodSymbol.IsGenericMethod || methodSymbol.Parameters.Length == 0)
        {
            return false;
        }

        var containingType = methodSymbol.ContainingType;
        if (containingType is null)
        {
            return false;
        }

        var namespaceName = containingType.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        if (namespaceName != "System" || containingType.Name != "Activator" || methodSymbol.Name != "CreateInstance")
        {
            return false;
        }

        return methodSymbol.Parameters[0].Type is INamedTypeSymbol { Name: "Type" };
    }

    private bool HasBannedTypeMethod(IMethodSymbol methodSymbol)
    {
        var containingType = methodSymbol.ContainingType;
        if (containingType is null)
        {
            return false;
        }

        var namespaceName = containingType.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        return namespaceName == "System"
               && containingType.Name == "Type"
               && BannedTypeMethodNames.Contains(methodSymbol.Name);
    }

    private bool HasDispatchProxyHelperClassContext(SyntaxNodeAnalysisContext context, CompilationSymbols symbols)
    {
        if (symbols.MethodInfoType is null)
        {
            return false;
        }

        var methodDeclaration = context.Node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (methodDeclaration is null)
        {
            return false;
        }

        if (context.SemanticModel.GetDeclaredSymbol(methodDeclaration) is not IMethodSymbol methodSymbol)
        {
            return false;
        }

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
        if (symbols.DispatchProxyType is null)
        {
            return false;
        }

        var classDeclaration = context.Node.FirstAncestorOrSelf<ClassDeclarationSyntax>();
        if (classDeclaration is null)
        {
            return false;
        }

        if (context.SemanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol classSymbol)
        {
            return false;
        }

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
        return HasDispatchProxyHelperClassContext(context, symbols)
               || HasDispatchProxySubclassContext(context, symbols)
               || HasServiceCollectionExtensionContext(context, symbols)
               || HasTypeExtensionClassContext(context, symbols);
    }

    private bool HasReflectionNamespaceMethod(IMethodSymbol methodSymbol)
    {
        var namespaceName = methodSymbol.ContainingType?.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        return namespaceName.StartsWith("System.Reflection", StringComparison.Ordinal);
    }

    private bool HasServiceCollectionExtensionContext(SyntaxNodeAnalysisContext context, CompilationSymbols symbols)
    {
        if (symbols.ServiceCollectionType is null)
        {
            return false;
        }

        var methodDeclaration = context.Node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (methodDeclaration is null)
        {
            return false;
        }

        if (context.SemanticModel.GetDeclaredSymbol(methodDeclaration) is not IMethodSymbol methodSymbol)
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

    private bool HasTypeExtensionClassContext(SyntaxNodeAnalysisContext context, CompilationSymbols symbols)
    {
        if (symbols.SystemTypeSymbol is null)
        {
            return false;
        }

        var methodDeclaration = context.Node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (methodDeclaration is null)
        {
            return false;
        }

        if (context.SemanticModel.GetDeclaredSymbol(methodDeclaration) is not IMethodSymbol methodSymbol)
        {
            return false;
        }

        if (!methodSymbol.IsExtensionMethod)
        {
            return false;
        }

        if (methodSymbol.Parameters.IsEmpty)
        {
            return false;
        }

        return SymbolEqualityComparer.Default.Equals(methodSymbol.Parameters[0].Type.OriginalDefinition, symbols.SystemTypeSymbol);
    }

    private void RegisterPerCompilation(CompilationStartAnalysisContext compilationContext)
    {
        var symbols = new CompilationSymbols(
            compilationContext.Compilation.GetTypeByMetadataName("Microsoft.Extensions.DependencyInjection.IServiceCollection"),
            compilationContext.Compilation.GetTypeByMetadataName("System.Reflection.DispatchProxy"),
            compilationContext.Compilation.GetTypeByMetadataName("System.Reflection.MethodInfo"),
            compilationContext.Compilation.GetTypeByMetadataName("System.Type"));
        compilationContext.RegisterSyntaxNodeAction(
            context => AnalyzeIdentifier(context, symbols),
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
