using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace Automaticks.Testing;

/// <summary>
///     Warns when a test class name does not match any real type in the compilation.
///     The analyzer strips the <c>Tests</c> suffix and searches for a progressively shorter
///     PascalCase prefix until a match is found.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TestClassNameAnalyzer : DiagnosticAnalyzer
{
    private static readonly Regex PascalWordPattern = new("[A-Z][a-z0-9]*", RegexOptions.Compiled);

    /// <summary>
    ///     The diagnostic rule reported when a test class name cannot be matched to a type
    ///     in the compilation.
    /// </summary>
    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.Testing.TestClassName,
        "Test class name must match the class under test",
        "Test class '{0}' does not match any type in the compilation. Expected a class named after the type under test (e.g. 'FooTests' for type 'Foo').",
        "Testing",
        DiagnosticSeverity.Warning,
        true,
        "Rename the test class to follow the pattern `{TypeUnderTest}Tests`. For example, a test class for `FooService` must be named `FooServiceTests`. A qualifier suffix is allowed: `FooService{Qualifier}Tests`. The type `{TypeUnderTest}` must be an actual type present in the compilation.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(compilationContext =>
        {
            var typeNames = BuildTypeNameSet(compilationContext.Compilation);
            compilationContext.RegisterSymbolAction(
                symbolContext => AnalyzeType(symbolContext, typeNames),
                SymbolKind.NamedType);
        });
    }

    private static ImmutableHashSet<string> BuildTypeNameSet(Compilation compilation)
    {
        var builder = ImmutableHashSet.CreateBuilder<string>(StringComparer.Ordinal);

        foreach (var symbol in compilation.GetSymbolsWithName(_ => true, SymbolFilter.Type))
        {
            builder.Add(symbol.Name);
        }

        foreach (var reference in compilation.References)
        {
            if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assembly
                && !IsFrameworkAssembly(assembly))
            {
                CollectTypeNames(assembly.GlobalNamespace, builder);
            }
        }

        return builder.ToImmutable();
    }

    private static bool IsFrameworkAssembly(IAssemblySymbol assembly)
    {
        var name = assembly.Identity.Name;
        return name.StartsWith("System", StringComparison.Ordinal)
               || name.StartsWith("Microsoft", StringComparison.Ordinal)
               || name.StartsWith("mscorlib", StringComparison.Ordinal)
               || name.StartsWith("netstandard", StringComparison.Ordinal)
               || name.StartsWith("WindowsBase", StringComparison.Ordinal)
               || name.StartsWith("TUnit", StringComparison.Ordinal)
               || name.StartsWith("Avalonia", StringComparison.Ordinal)
               || name.StartsWith("CommunityToolkit", StringComparison.Ordinal)
               || name.StartsWith("SonarAnalyzer", StringComparison.Ordinal);
    }

    private static void CollectTypeNames(INamespaceSymbol ns, ImmutableHashSet<string>.Builder builder)
    {
        foreach (var type in ns.GetTypeMembers())
        {
            builder.Add(type.Name);
        }

        foreach (var child in ns.GetNamespaceMembers())
        {
            CollectTypeNames(child, builder);
        }
    }

    private static void AnalyzeType(SymbolAnalysisContext context, ImmutableHashSet<string> typeNames)
    {
        var typeSymbol = (INamedTypeSymbol)context.Symbol;
        if (!typeSymbol.Name.EndsWith("Tests", StringComparison.Ordinal))
        {
            return;
        }

        if (!HasTestMethod(typeSymbol))
        {
            return;
        }

        var baseName = typeSymbol.Name.Substring(0, typeSymbol.Name.Length - "Tests".Length);
        if (baseName.Length == 0)
        {
            return;
        }

        var matches = PascalWordPattern.Matches(baseName);
        var parts = new List<string>(matches.Count);
        foreach (Match match in matches)
        {
            parts.Add(match.Value);
        }

        if (parts.Count == 0)
        {
            return;
        }

        for (var len = parts.Count; len >= 1; len--)
        {
            var candidate = string.Concat(parts.GetRange(0, len));
            if (typeNames.Contains(candidate))
            {
                return;
            }
        }

        Location location;
        if (typeSymbol.Locations.Length > 0)
        {
            location = typeSymbol.Locations[0];
        }
        else
        {
            location = Location.None;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, location, typeSymbol.Name));
    }

    private static bool HasTestMethod(INamedTypeSymbol typeSymbol)
    {
        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is not IMethodSymbol method)
            {
                continue;
            }

            foreach (var attr in method.GetAttributes())
            {
                if (attr.AttributeClass?.Name == "TestAttribute")
                {
                    return true;
                }
            }
        }

        return false;
    }
}
