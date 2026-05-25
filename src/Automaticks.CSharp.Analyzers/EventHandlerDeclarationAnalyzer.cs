using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp;

/// <summary>
///     Flags event declarations whose type is <see cref="System.EventHandler" /> or
///     <see cref="System.EventHandler{TEventArgs}" />. Use <c>Action&lt;T&gt;</c> or a custom
///     delegate instead. Avalonia routed-event wrappers (add/remove via
///     <c>AddHandler</c>/<c>RemoveHandler</c>) are exempt.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EventHandlerDeclarationAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when an event, field, or property uses the forbidden
    ///     <see cref="System.EventHandler" /> or <see cref="System.EventHandler{TEventArgs}" /> type.
    /// </summary>
    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.CSharp.EventHandlerDeclaration,
        "EventHandler and EventHandler<T> declarations are not allowed",
        "'{0}' uses EventHandler or EventHandler<T> which is not allowed. Use Action<T> or a custom delegate instead.",
        "CSharp",
        DiagnosticSeverity.Error,
        true,
        "Replace `EventHandler` or `EventHandler<TEventArgs>` with `Action<TEventArgs>` or a named custom delegate. `EventHandler` carries the legacy `object sender` parameter that is rarely needed. Define an explicit delegate type that only includes the parameters your subscribers actually need.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
    }

    private static bool AccessorCallsAddOrRemoveHandler(AccessorDeclarationSyntax accessor)
    {
        if (accessor.ExpressionBody is not null)
        {
            return IsAddOrRemoveHandlerCall(accessor.ExpressionBody.Expression);
        }

        if (accessor.Body is not null)
        {
            foreach (var statement in accessor.Body.Statements)
            {
                if (statement is ExpressionStatementSyntax exprStmt && IsAddOrRemoveHandlerCall(exprStmt.Expression))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static void AnalyzeType(SymbolAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.Symbol;

        foreach (var member in type.GetMembers())
        {
            if (member is IEventSymbol eventSymbol)
            {
                if (IsEventHandlerType(eventSymbol.Type, context.Compilation))
                {
                    if (IsAvaloniaRoutedEventWrapper(eventSymbol))
                    {
                        continue;
                    }

                    Location location;
                    if (eventSymbol.Locations.Length > 0)
                    {
                        location = eventSymbol.Locations[0];
                    }
                    else
                    {
                        location = Location.None;
                    }

                    context.ReportDiagnostic(Diagnostic.Create(Rule, location, eventSymbol.Name));
                }
            }
            else if (member is IFieldSymbol field && IsEventHandlerType(field.Type, context.Compilation))
            {
                Location location;
                if (field.Locations.Length > 0)
                {
                    location = field.Locations[0];
                }
                else
                {
                    location = Location.None;
                }

                context.ReportDiagnostic(Diagnostic.Create(Rule, location, field.Name));
            }
            else if (member is IPropertySymbol property && IsEventHandlerType(property.Type, context.Compilation))
            {
                Location location;
                if (property.Locations.Length > 0)
                {
                    location = property.Locations[0];
                }
                else
                {
                    location = Location.None;
                }

                context.ReportDiagnostic(Diagnostic.Create(Rule, location, property.Name));
            }
        }
    }

    private static bool IsAddOrRemoveHandlerCall(ExpressionSyntax expression)
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

    private static bool IsAvaloniaRoutedEventWrapper(IEventSymbol eventSymbol)
    {
        foreach (var syntaxRef in eventSymbol.DeclaringSyntaxReferences)
        {
            var syntax = syntaxRef.GetSyntax();

            if (syntax is EventDeclarationSyntax { AccessorList: not null } eventDecl)
            {
                foreach (var accessor in eventDecl.AccessorList.Accessors)
                {
                    if (AccessorCallsAddOrRemoveHandler(accessor))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private static bool IsEventHandlerType(ITypeSymbol type, Compilation compilation)
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
