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
    /// <summary>The diagnostic rule reported when a reflection API is used.</summary>
    public static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.Reflection.ReflectionUsage,
        "Reflection is forbidden",
        "Use of reflection ('{0}') is forbidden. Auto-exempted only in IServiceCollection extension methods and DispatchProxy subclasses.",
        "Reflection",
        DiagnosticSeverity.Error,
        true,
        "Remove the reflective API call and redesign using dependency injection interfaces, compile-time generics, or source generators. Reflection bypasses static type safety, breaks ahead-of-time compilation, and complicates trimming. Auto-exemptions: reflection inside `IServiceCollection` extension methods and `DispatchProxy` subclasses is allowed.");

    private static readonly ImmutableHashSet<string> BannedReflectionTypeNames =
    [
        "Assembly", "BindingFlags", "ConstructorInfo", "EventInfo", "FieldInfo",
        "MemberInfo", "MethodBase", "MethodInfo", "ParameterInfo", "PropertyInfo", "TypeInfo"
    ];

    private static readonly ImmutableHashSet<string> BannedTypeMethodNames =
    [
        "GetConstructor", "GetConstructors", "GetEvent", "GetEvents", "GetField", "GetFields",
        "GetGenericArguments", "GetGenericTypeDefinition", "GetInterface", "GetInterfaces",
        "GetMember", "GetMembers", "GetMethod", "GetMethods", "GetProperty", "GetProperties",
        "GetTypeInfo", "MakeGenericType"
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

    private static void RegisterPerCompilation(CompilationStartAnalysisContext compilationContext)
    {
        var serviceCollectionType = compilationContext.Compilation.GetTypeByMetadataName(
            "Microsoft.Extensions.DependencyInjection.IServiceCollection");
        var dispatchProxyType = compilationContext.Compilation.GetTypeByMetadataName(
            "System.Reflection.DispatchProxy");
        var methodInfoType = compilationContext.Compilation.GetTypeByMetadataName(
            "System.Reflection.MethodInfo");
        var systemTypeSymbol = compilationContext.Compilation.GetTypeByMetadataName("System.Type");

        compilationContext.RegisterSyntaxNodeAction(
            ctx => AnalyzeIdentifier(ctx, serviceCollectionType, dispatchProxyType, methodInfoType, systemTypeSymbol),
            SyntaxKind.IdentifierName);
        compilationContext.RegisterSyntaxNodeAction(
            ctx => AnalyzeInvocation(ctx, serviceCollectionType, dispatchProxyType, methodInfoType, systemTypeSymbol),
            SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeIdentifier(
        SyntaxNodeAnalysisContext context,
        INamedTypeSymbol? serviceCollectionType,
        INamedTypeSymbol? dispatchProxyType,
        INamedTypeSymbol? methodInfoType,
        INamedTypeSymbol? systemTypeSymbol)
    {
        if (IsInExemptContext(context, serviceCollectionType, dispatchProxyType, methodInfoType, systemTypeSymbol))
        {
            return;
        }

        var identifier = (IdentifierNameSyntax)context.Node;
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

    private static void AnalyzeInvocation(
        SyntaxNodeAnalysisContext context,
        INamedTypeSymbol? serviceCollectionType,
        INamedTypeSymbol? dispatchProxyType,
        INamedTypeSymbol? methodInfoType,
        INamedTypeSymbol? systemTypeSymbol)
    {
        if (IsInExemptContext(context, serviceCollectionType, dispatchProxyType, methodInfoType, systemTypeSymbol))
        {
            return;
        }

        var invocation = (InvocationExpressionSyntax)context.Node;
        if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        if (IsReflectionNamespaceMethod(methodSymbol))
        {
            Report(context, invocation, $"{methodSymbol.ContainingType.Name}.{methodSymbol.Name}");
            return;
        }

        if (IsBannedTypeMethod(methodSymbol))
        {
            Report(context, invocation, $"Type.{methodSymbol.Name}");
            return;
        }

        if (IsBannedActivatorCall(methodSymbol))
        {
            Report(context, invocation, "Activator.CreateInstance");
        }
    }

    private static bool IsInExemptContext(
        SyntaxNodeAnalysisContext context,
        INamedTypeSymbol? serviceCollectionType,
        INamedTypeSymbol? dispatchProxyType,
        INamedTypeSymbol? methodInfoType,
        INamedTypeSymbol? systemTypeSymbol)
    {
        return IsInServiceCollectionExtension(context, serviceCollectionType)
               || IsInDispatchProxySubclass(context, dispatchProxyType)
               || IsInDispatchProxyHelperClass(context, methodInfoType)
               || IsInTypeExtensionClass(context, systemTypeSymbol);
    }

    private static bool IsInServiceCollectionExtension(
        SyntaxNodeAnalysisContext context,
        INamedTypeSymbol? serviceCollectionType)
    {
        if (serviceCollectionType is null)
        {
            return false;
        }

        var methodDecl = context.Node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (methodDecl is null)
        {
            return false;
        }

        if (context.SemanticModel.GetDeclaredSymbol(methodDecl) is not IMethodSymbol methodSymbol)
        {
            return false;
        }

        if (!methodSymbol.ContainingType.IsStatic)
        {
            return false;
        }

        foreach (var param in methodSymbol.Parameters)
        {
            if (SymbolEqualityComparer.Default.Equals(param.Type.OriginalDefinition, serviceCollectionType))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsInDispatchProxySubclass(
        SyntaxNodeAnalysisContext context,
        INamedTypeSymbol? dispatchProxyType)
    {
        if (dispatchProxyType is null)
        {
            return false;
        }

        var classDecl = context.Node.FirstAncestorOrSelf<ClassDeclarationSyntax>();
        if (classDecl is null)
        {
            return false;
        }

        if (context.SemanticModel.GetDeclaredSymbol(classDecl) is not INamedTypeSymbol classSymbol)
        {
            return false;
        }

        var baseType = classSymbol.BaseType;
        while (baseType is not null)
        {
            if (SymbolEqualityComparer.Default.Equals(baseType.OriginalDefinition, dispatchProxyType))
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    private static bool IsInDispatchProxyHelperClass(
        SyntaxNodeAnalysisContext context,
        INamedTypeSymbol? methodInfoType)
    {
        if (methodInfoType is null)
        {
            return false;
        }

        var methodDecl = context.Node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (methodDecl is null)
        {
            return false;
        }

        if (context.SemanticModel.GetDeclaredSymbol(methodDecl) is not IMethodSymbol methodSymbol)
        {
            return false;
        }

        if (!methodSymbol.ContainingType.IsStatic)
        {
            return false;
        }

        foreach (var param in methodSymbol.Parameters)
        {
            if (SymbolEqualityComparer.Default.Equals(param.Type.OriginalDefinition, methodInfoType))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsInTypeExtensionClass(
        SyntaxNodeAnalysisContext context,
        INamedTypeSymbol? systemTypeSymbol)
    {
        if (systemTypeSymbol is null)
        {
            return false;
        }

        var methodDecl = context.Node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (methodDecl is null)
        {
            return false;
        }

        if (context.SemanticModel.GetDeclaredSymbol(methodDecl) is not IMethodSymbol methodSymbol)
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

        return SymbolEqualityComparer.Default.Equals(methodSymbol.Parameters[0].Type.OriginalDefinition, systemTypeSymbol);
    }

    private static bool IsBannedActivatorCall(IMethodSymbol methodSymbol)
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

    private static bool IsBannedTypeMethod(IMethodSymbol methodSymbol)
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

    private static bool IsReflectionNamespaceMethod(IMethodSymbol methodSymbol)
    {
        var namespaceName = methodSymbol.ContainingType?.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        return namespaceName.StartsWith("System.Reflection", StringComparison.Ordinal);
    }

    private static void Report(SyntaxNodeAnalysisContext context, SyntaxNode node, string reflectionItem)
    {
        context.ReportDiagnostic(Diagnostic.Create(Rule, node.GetLocation(), reflectionItem));
    }
}
