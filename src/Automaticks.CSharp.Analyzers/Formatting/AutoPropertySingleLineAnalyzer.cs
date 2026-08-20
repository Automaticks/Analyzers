using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.Formatting;

/// <summary>
///     Enforces that auto-implemented properties are declared on a single line.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AutoPropertySingleLineAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when an auto-implemented property spans more than one line.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static AutoPropertySingleLineAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.AutoPropertySingleLine,
            "Auto-implemented property must be declared on a single line",
            "Property '{0}' has only auto-implemented accessors and must be declared on a single line. A code fix is available (dotnet format analyzers --diagnostics ATXCS045).",
            "Style",
            DiagnosticSeverity.Error,
            true,
            "Collapse the auto-implemented property to a single line. Example: replace the multi-line `public string Foo { get; }` block with `public string Foo { get; }` on one line. This rule only applies to properties where every accessor has no body (no `=>` or `{ ... }` logic).");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeProperty, SyntaxKind.PropertyDeclaration);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeProperty(SyntaxNodeAnalysisContext context)
    {
        var property = (context.Node as PropertyDeclarationSyntax)!;
        if (property.AccessorList is not { } accessorList)
        {
            return;
        }

        if (!HasOnlyAutoImplementedAccessors(accessorList))
        {
            return;
        }

        var startToken = property.Modifiers.Count > 0
            ? property.Modifiers[0]
            : property.Type.GetFirstToken();

        var startLine = startToken.GetLocation().GetLineSpan().StartLinePosition.Line;
        var endLine = accessorList.CloseBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line;

        if (startLine != endLine)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, accessorList.OpenBraceToken.GetLocation(), property.Identifier.Text));
        }
    }

    private bool HasOnlyAutoImplementedAccessors(AccessorListSyntax accessorList)
    {
        foreach (var accessor in accessorList.Accessors)
        {
            if (accessor.Body is not null || accessor.ExpressionBody is not null)
            {
                return false;
            }
        }

        return true;
    }
}
