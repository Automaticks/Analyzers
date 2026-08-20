using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using static Automaticks.CSharp.Naming.AsyncReturnTypeHelper;

namespace Automaticks.CSharp.Naming;

/// <summary>
///     Flags methods whose name ends with <c>Async</c> but whose return type is not
///     <see cref="System.Threading.Tasks.Task" />, <see cref="System.Threading.Tasks.Task{TResult}" />,
///     <see cref="System.Threading.Tasks.ValueTask" />,
///     <see cref="System.Threading.Tasks.ValueTask{TResult}" />, or
///     <c>IAsyncEnumerable&lt;T&gt;</c>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AsyncSuffixReturnTypeAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a method named with the <c>Async</c> suffix does not
    ///     return a recognised async type.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static AsyncSuffixReturnTypeAnalyzer()
    {
        Rule = new(
            DiagnosticIds.CSharp.AsyncSuffixReturnType,
            "Methods with the 'Async' suffix must return Task, ValueTask, or IAsyncEnumerable<T>",
            "Method '{0}' has the 'Async' suffix but returns '{1}'. Rename the method or change the return type to Task, Task<T>, ValueTask, ValueTask<T>, or IAsyncEnumerable<T>. A code fix is available (dotnet format analyzers --diagnostics ATXCS009).",
            "CSharp",
            DiagnosticSeverity.Error,
            true,
            "A method whose name ends with `Async` must return `Task`, `Task<T>`, `ValueTask`, `ValueTask<T>`, or `IAsyncEnumerable<T>`. Either remove the `Async` suffix from the method name, or change the return type to one of the allowed async types.");
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
        var method = (context.Node as MethodDeclarationSyntax)!;
        if (!method.Identifier.Text.EndsWith("Async", StringComparison.Ordinal))
        {
            return;
        }

        var symbol = context.SemanticModel.GetDeclaredSymbol(method)!;
        if (HasAsyncReturnType(symbol, context.SemanticModel.Compilation))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, method.Identifier.GetLocation(), method.Identifier.Text, symbol.ReturnType.ToDisplayString()));
    }
}
