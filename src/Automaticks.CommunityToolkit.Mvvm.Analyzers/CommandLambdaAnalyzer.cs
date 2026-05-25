using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace Automaticks.CommunityToolkit.Mvvm;

/// <summary>
///     Flags <c>RelayCommand</c> and <c>AsyncRelayCommand</c> constructor arguments that use
///     lambda expressions or anonymous methods. Method groups must be used instead so that the
///     command handler is a named, discoverable method.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CommandLambdaAnalyzer : DiagnosticAnalyzer
{
    private static readonly string[] CommandTypeMetadataNames =
    [
        "CommunityToolkit.Mvvm.Input.RelayCommand",
        "CommunityToolkit.Mvvm.Input.RelayCommand`1",
        "CommunityToolkit.Mvvm.Input.AsyncRelayCommand",
        "CommunityToolkit.Mvvm.Input.AsyncRelayCommand`1"
    ];

    /// <summary>
    ///     The diagnostic rule reported when a command constructor receives a lambda or
    ///     anonymous method as an argument.
    /// </summary>
    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.Mvvm.CommandLambda,
        "Command constructors must use method groups, not lambdas",
        "Argument to '{0}' constructor is a lambda expression. Use a named method group instead.",
        "CommunityToolkit.Mvvm",
        DiagnosticSeverity.Error,
        true,
        "Replace the lambda expression with a named method group. Example: change `new RelayCommand(() => Execute())` to `new RelayCommand(Execute)` where `Execute` is a named method on the same class. This applies to all `RelayCommand` and `AsyncRelayCommand` constructor arguments.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression);
    }

    private static void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
    {
        var creation = (ObjectCreationExpressionSyntax)context.Node;
        var typeInfo = context.SemanticModel.GetTypeInfo(creation);

        if (typeInfo.Type is not INamedTypeSymbol createdType)
        {
            return;
        }

        if (!IsCommandType(createdType))
        {
            return;
        }

        if (creation.ArgumentList is null)
        {
            return;
        }

        foreach (var argument in creation.ArgumentList.Arguments)
        {
            if (argument.Expression is LambdaExpressionSyntax or AnonymousMethodExpressionSyntax)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, argument.GetLocation(), createdType.Name));
            }
        }
    }

    private static bool IsCommandType(INamedTypeSymbol type)
    {
        var ns = type.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        string metadataName;
        if (type.IsGenericType)
        {
            metadataName = $"{ns}.{type.MetadataName}";
        }
        else
        {
            metadataName = $"{ns}.{type.Name}";
        }

        foreach (var name in CommandTypeMetadataNames)
        {
            if (string.Equals(metadataName, name, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
