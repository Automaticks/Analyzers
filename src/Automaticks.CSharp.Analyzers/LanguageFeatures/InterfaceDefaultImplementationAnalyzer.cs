using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.LanguageFeatures;

/// <summary>
///     Flags interface members that carry a default implementation body or are static.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InterfaceDefaultImplementationAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule;

    static InterfaceDefaultImplementationAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.InterfaceDefaultImplementation,
            "Interface default implementations are forbidden",
            "Member '{0}' in interface '{1}' must not have an implementation body. Remove the body and define it in implementing types. A code fix is available (dotnet format analyzers --diagnostics ATXCS061).",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "Default interface implementations (method bodies, property accessor bodies, static members) couple the contract to an implementation detail and undermine the purpose of an interface. Remove the implementation body entirely and move the logic to the types that implement the interface.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInterface, SyntaxKind.InterfaceDeclaration);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeInterface(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InterfaceDeclarationSyntax interfaceDeclaration)
        {
            return;
        }

        var interfaceName = interfaceDeclaration.Identifier.Text;

        foreach (var member in interfaceDeclaration.Members)
        {
            switch (member)
            {
                case MethodDeclarationSyntax method when HasImplementation(method):
                    context.ReportDiagnostic(Diagnostic.Create(Rule, method.Identifier.GetLocation(), method.Identifier.Text, interfaceName));
                    break;

                case PropertyDeclarationSyntax property when HasImplementation(property):
                    context.ReportDiagnostic(Diagnostic.Create(Rule, property.Identifier.GetLocation(), property.Identifier.Text, interfaceName));
                    break;

                case EventDeclarationSyntax eventDeclaration when HasImplementation(eventDeclaration):
                    context.ReportDiagnostic(Diagnostic.Create(Rule, eventDeclaration.Identifier.GetLocation(), eventDeclaration.Identifier.Text, interfaceName));
                    break;

                case OperatorDeclarationSyntax operatorDeclaration when operatorDeclaration.Body != null || operatorDeclaration.ExpressionBody != null:
                    context.ReportDiagnostic(Diagnostic.Create(Rule, operatorDeclaration.OperatorToken.GetLocation(), operatorDeclaration.OperatorToken.Text, interfaceName));
                    break;

                case ConversionOperatorDeclarationSyntax conversionOperatorDeclaration when conversionOperatorDeclaration.Body != null || conversionOperatorDeclaration.ExpressionBody != null:
                    context.ReportDiagnostic(Diagnostic.Create(Rule, conversionOperatorDeclaration.Type.GetLocation(), conversionOperatorDeclaration.Type.ToString(), interfaceName));
                    break;

                case FieldDeclarationSyntax field:
                    foreach (var variable in field.Declaration.Variables)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Rule, variable.Identifier.GetLocation(), variable.Identifier.Text, interfaceName));
                    }

                    break;

                case EventFieldDeclarationSyntax staticEvent when HasStaticModifier(staticEvent.Modifiers):
                    foreach (var variable in staticEvent.Declaration.Variables)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Rule, variable.Identifier.GetLocation(), variable.Identifier.Text, interfaceName));
                    }

                    break;

                case IndexerDeclarationSyntax indexer when HasImplementation(indexer):
                    context.ReportDiagnostic(Diagnostic.Create(Rule, indexer.ThisKeyword.GetLocation(), "this[]", interfaceName));
                    break;
            }
        }
    }

    private bool HasAbstractModifier(SyntaxTokenList modifiers)
    {
        foreach (var modifier in modifiers)
        {
            if (modifier.IsKind(SyntaxKind.AbstractKeyword))
            {
                return true;
            }
        }

        return false;
    }

    private bool HasImplementation(EventDeclarationSyntax eventDeclaration)
    {
        if (eventDeclaration.AccessorList == null)
        {
            return false;
        }

        foreach (var accessor in eventDeclaration.AccessorList.Accessors)
        {
            if (accessor.Body != null || accessor.ExpressionBody != null)
            {
                return true;
            }
        }

        return false;
    }

    private bool HasImplementation(IndexerDeclarationSyntax indexer)
    {
        if (indexer.ExpressionBody != null)
        {
            return true;
        }

        if (indexer.AccessorList == null)
        {
            return false;
        }

        foreach (var accessor in indexer.AccessorList.Accessors)
        {
            if (accessor.Body != null || accessor.ExpressionBody != null)
            {
                return true;
            }
        }

        return false;
    }

    private bool HasImplementation(MethodDeclarationSyntax method)
    {
        return method.Body != null
               || method.ExpressionBody != null
               || (HasStaticModifier(method.Modifiers) && !HasAbstractModifier(method.Modifiers));
    }

    private bool HasImplementation(PropertyDeclarationSyntax property)
    {
        if (property.ExpressionBody != null
            || (HasStaticModifier(property.Modifiers) && !HasAbstractModifier(property.Modifiers)))
        {
            return true;
        }

        if (property.AccessorList == null)
        {
            return false;
        }

        foreach (var accessor in property.AccessorList.Accessors)
        {
            if (accessor.Body != null || accessor.ExpressionBody != null)
            {
                return true;
            }
        }

        return false;
    }

    private bool HasStaticModifier(SyntaxTokenList modifiers)
    {
        foreach (var modifier in modifiers)
        {
            if (modifier.IsKind(SyntaxKind.StaticKeyword))
            {
                return true;
            }
        }

        return false;
    }
}
