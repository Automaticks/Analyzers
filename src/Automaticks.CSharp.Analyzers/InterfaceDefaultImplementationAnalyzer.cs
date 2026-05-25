using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Automaticks.CSharp;

/// <summary>
///     Flags any interface member that carries a default implementation body or is a static member.
///     Interfaces must remain pure contracts; all implementation must live in concrete types.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InterfaceDefaultImplementationAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when an interface member has a default implementation or is static.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.CSharp.InterfaceDefaultImplementation,
        "Interface default implementations are forbidden",
        "Member '{0}' in interface '{1}' must not have an implementation body. Remove the body and define it in implementing types.",
        "CSharp",
        DiagnosticSeverity.Error,
        true,
        "Default interface implementations (method bodies, property accessor bodies, static members) couple the contract to an implementation detail and undermine the purpose of an interface. Remove the implementation body entirely and move the logic to the types that implement the interface.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInterface, SyntaxKind.InterfaceDeclaration);
    }

    private static void AnalyzeInterface(SyntaxNodeAnalysisContext context)
    {
        var interfaceDecl = (InterfaceDeclarationSyntax)context.Node;
        var interfaceName = interfaceDecl.Identifier.Text;

        foreach (var member in interfaceDecl.Members)
        {
            switch (member)
            {
                case MethodDeclarationSyntax method when HasImplementation(method):
                    context.ReportDiagnostic(Diagnostic.Create(Rule, method.Identifier.GetLocation(), method.Identifier.Text, interfaceName));
                    break;

                case PropertyDeclarationSyntax property when HasImplementation(property):
                    context.ReportDiagnostic(Diagnostic.Create(Rule, property.Identifier.GetLocation(), property.Identifier.Text, interfaceName));
                    break;

                case EventDeclarationSyntax eventDecl when HasImplementation(eventDecl):
                    context.ReportDiagnostic(Diagnostic.Create(Rule, eventDecl.Identifier.GetLocation(), eventDecl.Identifier.Text, interfaceName));
                    break;

                case OperatorDeclarationSyntax operatorDecl when operatorDecl.Body != null || operatorDecl.ExpressionBody != null:
                    context.ReportDiagnostic(Diagnostic.Create(Rule, operatorDecl.OperatorToken.GetLocation(), operatorDecl.OperatorToken.Text, interfaceName));
                    break;

                case ConversionOperatorDeclarationSyntax convDecl when convDecl.Body != null || convDecl.ExpressionBody != null:
                    context.ReportDiagnostic(Diagnostic.Create(Rule, convDecl.Type.GetLocation(), convDecl.Type.ToString(), interfaceName));
                    break;

                case FieldDeclarationSyntax field:
                    foreach (var variable in field.Declaration.Variables)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Rule, variable.Identifier.GetLocation(), variable.Identifier.Text, interfaceName));
                    }
                    break;

                case EventFieldDeclarationSyntax staticEvent
                    when staticEvent.Modifiers.Any(SyntaxKind.StaticKeyword):
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

    private static bool HasImplementation(MethodDeclarationSyntax method)
    {
        return method.Body != null
            || method.ExpressionBody != null
            || (method.Modifiers.Any(SyntaxKind.StaticKeyword)
                && !method.Modifiers.Any(SyntaxKind.AbstractKeyword));
    }

    private static bool HasImplementation(PropertyDeclarationSyntax property)
    {
        if (property.ExpressionBody != null
            || (property.Modifiers.Any(SyntaxKind.StaticKeyword)
                && !property.Modifiers.Any(SyntaxKind.AbstractKeyword)))
        {
            return true;
        }

        if (property.AccessorList == null)
        {
            return false;
        }

        return property.AccessorList.Accessors.Any(a => a.Body != null || a.ExpressionBody != null);
    }

    private static bool HasImplementation(EventDeclarationSyntax eventDecl)
    {
        if (eventDecl.AccessorList == null)
        {
            return false;
        }

        return eventDecl.AccessorList.Accessors.Any(a => a.Body != null || a.ExpressionBody != null);
    }

    private static bool HasImplementation(IndexerDeclarationSyntax indexer)
    {
        if (indexer.ExpressionBody != null)
        {
            return true;
        }

        if (indexer.AccessorList == null)
        {
            return false;
        }

        return indexer.AccessorList.Accessors.Any(a => a.Body != null || a.ExpressionBody != null);
    }
}
