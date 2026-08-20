using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.Testing.Testability;

/// <summary>
///     Flags a [Test] method whose body contains no assertion.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MissingAssertionAnalyzer : DiagnosticAnalyzer
{
    private const string AssertIdentifierName = "Assert";
    private static readonly DiagnosticDescriptor Rule;

    static MissingAssertionAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.Testing.MissingAssertion,
            "Test method must contain an assertion",
            "Test method '{0}' contains no assertion, so it passes regardless of what the code under test does. Add an Assert call that verifies the expected outcome.",
            "Testing",
            DiagnosticSeverity.Warning,
            true,
            "A [Test] method with no Assert call always passes, whatever the code under test does, so it gives false confidence and hides regressions. Add an assertion such as `Assert.That(...)` that checks the expected outcome, or use `Assert.Throws`/`ThrowsAsync` to verify an expected exception.");
        Rule = rule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MethodDeclarationSyntax method)
        {
            return;
        }

        SyntaxNode? body = method.Body;
        body ??= method.ExpressionBody;
        if (body is null)
        {
            return;
        }

        if (!HasTestAttribute(context, method))
        {
            return;
        }

        if (HasAssertion(body))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, method.Identifier.GetLocation(), method.Identifier.ValueText));
    }

    private bool HasAssertIdentifier(ExpressionSyntax expression)
    {
        foreach (var node in expression.DescendantNodesAndSelf())
        {
            if (node is IdentifierNameSyntax identifier && identifier.Identifier.ValueText == AssertIdentifierName)
            {
                return true;
            }
        }

        return false;
    }

    private bool HasAssertion(SyntaxNode body)
    {
        foreach (var node in body.DescendantNodes())
        {
            if (node is InvocationExpressionSyntax invocation && HasAssertIdentifier(invocation.Expression))
            {
                return true;
            }
        }

        return false;
    }

    private bool HasTestAttribute(SyntaxNodeAnalysisContext context, MethodDeclarationSyntax method)
    {
        var symbol = context.SemanticModel.GetDeclaredSymbol(method, context.CancellationToken);
        if (symbol is null)
        {
            return false;
        }

        foreach (var attribute in symbol.GetAttributes())
        {
            if (attribute.AttributeClass?.Name == "TestAttribute")
            {
                return true;
            }
        }

        return false;
    }
}
