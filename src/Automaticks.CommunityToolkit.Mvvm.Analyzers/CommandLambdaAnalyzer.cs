using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace Automaticks.CommunityToolkit.Mvvm;

/// <summary>
///     Flags RelayCommand and AsyncRelayCommand constructor arguments that use lambda expressions or anonymous methods.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CommandLambdaAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<string> CommandTypeMetadataNames;
    private static readonly DiagnosticDescriptor Rule;

    static CommandLambdaAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.ModelViewViewModel.CommandLambda,
            "Command constructors must use method groups, not lambdas",
            "Argument to '{0}' constructor is a lambda expression. Use a named method group instead. A code fix is available (dotnet format analyzers --diagnostics ATXMV001).",
            "CommunityToolkit.Mvvm",
            DiagnosticSeverity.Error,
            true,
            "Replace the lambda expression with a named method group. Example: change `new RelayCommand(() => Execute())` to `new RelayCommand(Execute)` where `Execute` is a named method on the same class. This applies to all `RelayCommand` and `AsyncRelayCommand` constructor arguments.");
        Rule = rule;
        CommandTypeMetadataNames =
        [
            "CommunityToolkit.Mvvm.Input.RelayCommand",
            "CommunityToolkit.Mvvm.Input.RelayCommand`1",
            "CommunityToolkit.Mvvm.Input.AsyncRelayCommand",
            "CommunityToolkit.Mvvm.Input.AsyncRelayCommand`1"
        ];
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
    {
        var creation = (context.Node as ObjectCreationExpressionSyntax)!;
        var typeInfo = context.SemanticModel.GetTypeInfo(creation);

        if (typeInfo.Type is not INamedTypeSymbol createdType)
        {
            return;
        }

        if (!HasCommandType(createdType))
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

    private bool HasCommandType(INamedTypeSymbol type)
    {
        var namespaceName = type.ContainingNamespace.ToDisplayString();
        string metadataName;
        if (type.IsGenericType)
        {
            metadataName = $"{namespaceName}.{type.MetadataName}";
        }
        else
        {
            metadataName = $"{namespaceName}.{type.Name}";
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
