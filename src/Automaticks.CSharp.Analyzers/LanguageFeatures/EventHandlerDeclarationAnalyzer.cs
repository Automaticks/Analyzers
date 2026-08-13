using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.LanguageFeatures;

/// <summary>
///     Flags event, field, and property declarations whose type is <see cref="System.EventHandler" /> or <see cref="System.EventHandler{TEventArgs}" />.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EventHandlerDeclarationAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when an event, field, or property uses the forbidden <see cref="System.EventHandler" /> or <see cref="System.EventHandler{TEventArgs}" /> type.
    /// </summary>
    private static readonly DiagnosticDescriptor Rule;

    static EventHandlerDeclarationAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.EventHandlerDeclaration,
            "EventHandler and EventHandler<T> declarations are not allowed",
            "'{0}' uses EventHandler or EventHandler<T> which is not allowed. Use Action<T> or a custom delegate instead.",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "Replace `EventHandler` or `EventHandler<TEventArgs>` with `Action<TEventArgs>` or a named custom delegate. `EventHandler` carries the legacy `object sender` parameter that is rarely needed. Define an explicit delegate type that only includes the parameters your subscribers actually need.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeMember(SymbolAnalysisContext context, ISymbol member)
    {
        if (member is IEventSymbol eventSymbol)
        {
            if (HasEventHandlerType(eventSymbol.Type, context.Compilation) && !HasAvaloniaRoutedEventWrapperAccessors(eventSymbol))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, GetReportLocation(eventSymbol.Locations), eventSymbol.Name));
            }

            return;
        }

        if (member is IFieldSymbol field && HasEventHandlerType(field.Type, context.Compilation))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, GetReportLocation(field.Locations), field.Name));
            return;
        }

        if (member is IPropertySymbol property && HasEventHandlerType(property.Type, context.Compilation))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, GetReportLocation(property.Locations), property.Name));
        }
    }

    private void AnalyzeType(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol type)
        {
            return;
        }

        foreach (var member in type.GetMembers())
        {
            AnalyzeMember(context, member);
        }
    }

    private Location GetReportLocation(ImmutableArray<Location> locations)
    {
        if (locations.Length > 0)
        {
            return locations[0];
        }

        return Location.None;
    }

    private bool HasAddOrRemoveHandlerCall(ExpressionSyntax expression)
    {
        if (expression is not InvocationExpressionSyntax invocation)
        {
            return false;
        }

        var name = invocation.Expression switch
        {
            IdentifierNameSyntax id => id.Identifier.Text,
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.Text,
            _ => null
        };

        return name is "AddHandler" or "RemoveHandler";
    }

    private bool HasAddOrRemoveHandlerStatement(AccessorDeclarationSyntax accessor)
    {
        if (accessor.ExpressionBody is not null)
        {
            return HasAddOrRemoveHandlerCall(accessor.ExpressionBody.Expression);
        }

        if (accessor.Body is not null)
        {
            foreach (var statement in accessor.Body.Statements)
            {
                if (statement is ExpressionStatementSyntax expressionStatement && HasAddOrRemoveHandlerCall(expressionStatement.Expression))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool HasAvaloniaRoutedEventWrapperAccessors(IEventSymbol eventSymbol)
    {
        foreach (var syntaxRef in eventSymbol.DeclaringSyntaxReferences)
        {
            var syntax = syntaxRef.GetSyntax();

            if (syntax is EventDeclarationSyntax { AccessorList: not null } eventDecl)
            {
                foreach (var accessor in eventDecl.AccessorList.Accessors)
                {
                    if (HasAddOrRemoveHandlerStatement(accessor))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private bool HasEventHandlerType(ITypeSymbol type, Compilation compilation)
    {
        var eventHandlerType = compilation.GetTypeByMetadataName("System.EventHandler");
        var eventHandlerOfTType = compilation.GetTypeByMetadataName("System.EventHandler`1");

        return (eventHandlerType is not null && SymbolEqualityComparer.Default.Equals(type, eventHandlerType))
               || (type is INamedTypeSymbol { IsGenericType: true } namedType && eventHandlerOfTType is not null
                                                                              && SymbolEqualityComparer.Default.Equals(
                                                                                  namedType.ConstructUnboundGenericType(),
                                                                                  eventHandlerOfTType.ConstructUnboundGenericType()));
    }
}
