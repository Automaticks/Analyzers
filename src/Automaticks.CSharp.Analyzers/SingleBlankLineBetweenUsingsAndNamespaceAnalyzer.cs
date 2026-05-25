using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp;

/// <summary>
///     Enforces exactly one blank line between the last top-level <c>using</c> directive and the
///     <c>namespace</c> declaration (both block-scoped and file-scoped forms).
///     Zero blank lines (cramped) and two or more blank lines (over-spaced) are both violations.
///     Files that contain no <c>using</c> directives or no <c>namespace</c> declaration are exempt.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SingleBlankLineBetweenUsingsAndNamespaceAnalyzer : DiagnosticAnalyzer
{
    /// <summary>Fires when the <c>namespace</c> keyword immediately follows the last <c>using</c> with no blank line.</summary>
    public static readonly DiagnosticDescriptor TooFewBlankLinesRule = new(
        DiagnosticIds.CSharp.SingleBlankLineBetweenUsingsAndNamespace,
        "Missing blank line between using directives and namespace declaration",
        "Add a blank line between the last using directive and the namespace declaration",
        "Style",
        DiagnosticSeverity.Error,
        true,
        "Add exactly one blank line between the last `using` directive and the `namespace` declaration. This visually separates the import section from the namespace scope.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [TooFewBlankLinesRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeCompilationUnit, SyntaxKind.CompilationUnit);
    }

    private static void AnalyzeCompilationUnit(SyntaxNodeAnalysisContext context)
    {
        var compilationUnit = (CompilationUnitSyntax)context.Node;

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

    private static MemberDeclarationSyntax? FindFirstNamespace(CompilationUnitSyntax compilationUnit)
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

    private static int CountBlankLines(UsingDirectiveSyntax lastUsing, MemberDeclarationSyntax namespaceSyntax)
    {
        var state = new BlankLineState();
        ProcessTriviaList(lastUsing.GetLastToken().TrailingTrivia, state);
        ProcessTriviaList(namespaceSyntax.GetFirstToken().LeadingTrivia, state);
        return state.Count;
    }

    private static void ProcessTriviaList(SyntaxTriviaList triviaList, BlankLineState state)
    {
        foreach (var trivia in triviaList)
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                if (state.SawNewline)
                {
                    state.Count++;
                }

                state.SawNewline = true;
            }
            else if (IsPreprocessorTrivia(trivia))
            {
                state.SawNewline = true;
            }
            else if (!trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                state.SawNewline = false;
            }
        }
    }

    private static bool IsPreprocessorTrivia(SyntaxTrivia trivia)
    {
        return trivia.IsKind(SyntaxKind.IfDirectiveTrivia)
            || trivia.IsKind(SyntaxKind.ElifDirectiveTrivia)
            || trivia.IsKind(SyntaxKind.ElseDirectiveTrivia)
            || trivia.IsKind(SyntaxKind.EndIfDirectiveTrivia)
            || trivia.IsKind(SyntaxKind.DisabledTextTrivia)
            || trivia.IsKind(SyntaxKind.DefineDirectiveTrivia)
            || trivia.IsKind(SyntaxKind.UndefDirectiveTrivia);
    }

    private sealed class BlankLineState
    {
        public bool SawNewline;
        public int Count;
    }
}
