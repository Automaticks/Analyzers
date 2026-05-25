using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Automaticks.CSharp;

/// <summary>
///     Enforces a maximum lines-of-code (LOC) limit per class. A class whose LOC count exceeds
///     <see cref="MaxLines" /> is flagged. LOC is measured as the number of non-blank,
///     non-comment lines within the class declaration span (from the class keyword through the
///     closing brace, including nested types). Leading/trailing trivia outside the class
///     declaration (e.g. XML doc comments, attributes on prior lines) are excluded.
///     <para>
///         <b>What is excluded from the count:</b>
///         blank/whitespace-only lines; single-line comment lines (<c>//</c> and <c>///</c>);
///         multi-line block comment lines (<c>/* … */</c>).
///     </para>
///     <para>
///         Partial classes have their LOC aggregated across all parts in the compilation.
///         A single diagnostic is reported per class symbol. Generated files (those containing
///         an <c>&lt;auto-generated&gt;</c> header) are skipped automatically.
///     </para>
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ClassLineLimitAnalyzer : DiagnosticAnalyzer
{
    /// <summary>Diagnostic rule emitted when a class exceeds <see cref="MaxLines" /> lines of code.</summary>
    public static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.CSharp.ClassLineLimit,
        "Classes must not exceed the maximum lines-of-code limit",
        "Class '{0}' has {1} lines of code, which exceeds the maximum of 500. Split it into smaller, focused classes.",
        "Maintainability",
        DiagnosticSeverity.Error,
        true,
        "The class body exceeds 500 lines of code (blank lines and comment-only lines are excluded). Split the class into smaller, focused types or extract cohesive groups of members into separate collaborator classes. Consider applying the Single Responsibility Principle to identify natural extraction points.",
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    private const int MaxLines = 500;

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(RegisterCompilationActions);
    }

    private static List<TextSpan> CollectBlockCommentSpans(SyntaxNode node)
    {
        var spans = new List<TextSpan>();
        foreach (var trivia in node.DescendantTrivia())
        {
            if (trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
            {
                spans.Add(trivia.Span);
            }
        }

        return spans;
    }

    private static void CollectClassPart(
        SyntaxNodeAnalysisContext nodeContext,
        ConcurrentDictionary<INamedTypeSymbol, ConcurrentBag<(int Loc, Location Location)>> partsBySymbol)
    {
        var classDecl = (ClassDeclarationSyntax)nodeContext.Node;
        if (nodeContext.SemanticModel.GetDeclaredSymbol(classDecl) is not { } symbol)
        {
            return;
        }

        var sourceText = classDecl.SyntaxTree.GetText();
        var loc = CountLinesOfCode(classDecl, sourceText);
        var location = classDecl.Identifier.GetLocation();
        var parts = partsBySymbol.GetOrAdd(symbol, _ => []);
        parts.Add((loc, location));
    }

    private static int CountLinesOfCode(ClassDeclarationSyntax classDecl, SourceText sourceText)
    {
        var span = classDecl.Span;
        var startLine = sourceText.Lines.GetLineFromPosition(span.Start).LineNumber;
        var endLine = sourceText.Lines.GetLineFromPosition(span.End).LineNumber;
        var blockCommentSpans = CollectBlockCommentSpans(classDecl);
        var count = 0;

        for (var lineIndex = startLine; lineIndex <= endLine; lineIndex++)
        {
            var lineText = sourceText.Lines[lineIndex].ToString();
            if (string.IsNullOrWhiteSpace(lineText))
            {
                continue;
            }

            var trimmed = lineText.TrimStart();
            if (trimmed.StartsWith("//", StringComparison.Ordinal))
            {
                continue;
            }

            var firstNonWhitespacePosition = sourceText.Lines[lineIndex].Start + (lineText.Length - trimmed.Length);
            if (IsInsideBlockComment(firstNonWhitespacePosition, blockCommentSpans))
            {
                continue;
            }

            count++;
        }

        return count;
    }

    private static bool IsEarlierLocation(Location candidate, Location current)
    {
        var candidatePath = candidate.SourceTree?.FilePath ?? string.Empty;
        var currentPath = current.SourceTree?.FilePath ?? string.Empty;
        var pathComparison = string.CompareOrdinal(candidatePath, currentPath);

        if (pathComparison != 0)
        {
            return pathComparison < 0;
        }

        return candidate.SourceSpan.Start < current.SourceSpan.Start;
    }

    private static bool IsInsideBlockComment(int position, List<TextSpan> blockCommentSpans)
    {
        foreach (var span in blockCommentSpans)
        {
            if (span.Contains(position))
            {
                return true;
            }
        }

        return false;
    }

    private static void RegisterCompilationActions(CompilationStartAnalysisContext compilationContext)
    {
        var partsBySymbol = new ConcurrentDictionary<INamedTypeSymbol, ConcurrentBag<(int Loc, Location Location)>>(
            SymbolEqualityComparer.Default);

        compilationContext.RegisterSyntaxNodeAction(
            nodeContext => CollectClassPart(nodeContext, partsBySymbol),
            SyntaxKind.ClassDeclaration);

        compilationContext.RegisterCompilationEndAction(endContext => ReportViolations(endContext, partsBySymbol));
    }

    private static void ReportViolations(
        CompilationAnalysisContext endContext,
        ConcurrentDictionary<INamedTypeSymbol, ConcurrentBag<(int Loc, Location Location)>> partsBySymbol)
    {
        foreach (var pair in partsBySymbol)
        {
            var totalLoc = 0;
            Location? firstLocation = null;

            foreach (var (loc, location) in pair.Value)
            {
                totalLoc += loc;
                if (firstLocation is null || IsEarlierLocation(location, firstLocation))
                {
                    firstLocation = location;
                }
            }

            if (totalLoc > MaxLines && firstLocation is not null)
            {
                endContext.ReportDiagnostic(Diagnostic.Create(Rule, firstLocation, pair.Key.Name, totalLoc));
            }
        }
    }
}
