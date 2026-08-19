using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace Automaticks.Testing;

/// <summary>
///     Warns when a test class name does not match any real type in the compilation.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TestClassNameAnalyzer : DiagnosticAnalyzer
{
    private static readonly Regex PascalWordPattern;
    private static readonly DiagnosticDescriptor Rule;

    static TestClassNameAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.Testing.TestClassName,
            "Test class name must match the class under test",
            "Test class '{0}' does not match any type in the compilation. Expected a class named after the type under test (e.g. 'FooTests' for type 'Foo').",
            "Testing",
            DiagnosticSeverity.Warning,
            true,
            "Rename the test class to follow the pattern `{TypeUnderTest}Tests`. For example, a test class for `FooService` must be named `FooServiceTests`. A qualifier suffix is allowed: `FooService{Qualifier}Tests`. The type `{TypeUnderTest}` must be an actual type present in the compilation.");
        Rule = rule;
        var pascalWordPattern = new Regex("[A-Z][a-z0-9]*", RegexOptions.Compiled);
        PascalWordPattern = pascalWordPattern;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(RegisterCompilationStart);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeType(SymbolAnalysisContext context, ImmutableHashSet<string> typeNames)
    {
        if (context.Symbol is not INamedTypeSymbol typeSymbol)
        {
            return;
        }

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

        for (var length = parts.Count; length >= 1; length--)
        {
            var candidate = string.Concat(parts.GetRange(0, length));
            if (typeNames.Contains(candidate))
            {
                return;
            }
        }

        var location = typeSymbol.Locations.Length > 0
            ? typeSymbol.Locations[0]
            : Location.None;
        context.ReportDiagnostic(Diagnostic.Create(Rule, location, typeSymbol.Name));
    }

    private ImmutableHashSet<string> BuildTypeNameSet(Compilation compilation)
    {
        var builder = ImmutableHashSet.CreateBuilder<string>(StringComparer.Ordinal);

        foreach (var symbol in compilation.GetSymbolsWithName(static _ => true, SymbolFilter.Type))
        {
            builder.Add(symbol.Name);
        }

        foreach (var reference in compilation.References)
        {
            if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assembly
                && !HasFrameworkAssembly(assembly))
            {
                CollectTypeNames(assembly.GlobalNamespace, builder);
            }
        }

        return builder.ToImmutable();
    }

    private void CollectTypeNames(INamespaceSymbol namespaceSymbol, ImmutableHashSet<string>.Builder builder)
    {
        foreach (var type in namespaceSymbol.GetTypeMembers())
        {
            builder.Add(type.Name);
        }

        foreach (var childNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            CollectTypeNames(childNamespace, builder);
        }
    }

    private bool HasFrameworkAssembly(IAssemblySymbol assembly)
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

    private bool HasTestMethod(INamedTypeSymbol typeSymbol)
    {
        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is not IMethodSymbol method)
            {
                continue;
            }

            foreach (var attribute in method.GetAttributes())
            {
                if (attribute.AttributeClass?.Name == "TestAttribute")
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void RegisterCompilationStart(CompilationStartAnalysisContext compilationContext)
    {
        var typeNames = BuildTypeNameSet(compilationContext.Compilation);
        compilationContext.RegisterSymbolAction(
            symbolContext => AnalyzeType(symbolContext, typeNames),
            SymbolKind.NamedType);
    }
}
