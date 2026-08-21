using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.Formatting;

/// <summary>
///     Enforces exactly one blank line between the last <c>using</c> directive and the <c>namespace</c> declaration.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SingleBlankLineBetweenUsingsAndNamespaceAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     Fires when the <c>namespace</c> keyword immediately follows the last <c>using</c> with no blank line.
    /// </summary>
    public static readonly DiagnosticDescriptor TooFewBlankLinesRule;

    static SingleBlankLineBetweenUsingsAndNamespaceAnalyzer()
    {
        var tooFewBlankLinesRule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.SingleBlankLineBetweenUsingsAndNamespace,
            "Missing blank line between using directives and namespace declaration",
            "Add a blank line between the last using directive and the namespace declaration. A code fix is available (dotnet format analyzers --diagnostics ATXCS043).",
            "Style",
            DiagnosticSeverity.Error,
            true,
            "Add exactly one blank line between the last `using` directive and the `namespace` declaration. This visually separates the import section from the namespace scope.");
        TooFewBlankLinesRule = tooFewBlankLinesRule;
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeCompilationUnit, SyntaxKind.CompilationUnit);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [TooFewBlankLinesRule];
        }
    }

    private void AnalyzeCompilationUnit(SyntaxNodeAnalysisContext context)
    {
        var compilationUnit = (context.Node as CompilationUnitSyntax)!;
        if (compilationUnit.Usings.Count == 0)
        {
            return;
        }

        var namespaceSyntax = FindFirstNamespace(compilationUnit);
        if (namespaceSyntax is null)
        {
            return;
        }

        var lastUsing = compilationUnit.Usings[compilationUnit.Usings.Count - 1];
        var blankLineCount = CountBlankLines(lastUsing, namespaceSyntax);

        if (blankLineCount == 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(TooFewBlankLinesRule, namespaceSyntax.GetFirstToken().GetLocation()));
        }
    }

    private int CountBlankLines(UsingDirectiveSyntax lastUsing, MemberDeclarationSyntax namespaceSyntax)
    {
        var state = new BlankLineState();
        ProcessTriviaList(lastUsing.GetLastToken().TrailingTrivia, state);
        ProcessTriviaList(namespaceSyntax.GetFirstToken().LeadingTrivia, state);
        return state.Count;
    }

    private MemberDeclarationSyntax? FindFirstNamespace(CompilationUnitSyntax compilationUnit)
    {
        foreach (var member in compilationUnit.Members)
        {
            if (member is NamespaceDeclarationSyntax or FileScopedNamespaceDeclarationSyntax)
            {
                return member;
            }
        }

        return null;
    }

    private bool HasPreprocessorTriviaKind(SyntaxTrivia trivia)
    {
        var isPreprocessorTrivia = trivia.IsKind(SyntaxKind.IfDirectiveTrivia)
            || trivia.IsKind(SyntaxKind.ElifDirectiveTrivia)
            || trivia.IsKind(SyntaxKind.ElseDirectiveTrivia)
            || trivia.IsKind(SyntaxKind.EndIfDirectiveTrivia)
            || trivia.IsKind(SyntaxKind.DisabledTextTrivia)
            || trivia.IsKind(SyntaxKind.DefineDirectiveTrivia)
            || trivia.IsKind(SyntaxKind.UndefDirectiveTrivia);
        return isPreprocessorTrivia;
    }

    private void ProcessTriviaList(SyntaxTriviaList triviaList, BlankLineState state)
    {
        foreach (var trivia in triviaList)
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                if (state.IsNewline)
                {
                    state.Count++;
                }

                state.IsNewline = true;
            }
            else if (HasPreprocessorTriviaKind(trivia))
            {
                state.IsNewline = true;
            }
            else if (!trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                state.IsNewline = false;
            }
        }
    }

    private sealed class BlankLineState
    {
        public int Count;
        public bool IsNewline;
    }
}
