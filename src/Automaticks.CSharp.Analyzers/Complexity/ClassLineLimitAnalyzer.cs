using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Automaticks.CSharp.Complexity;

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
    private const int MaxLines = 500;

    /// <summary>
    ///     Diagnostic rule emitted when a class exceeds <see cref="MaxLines" /> lines of code.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    static ClassLineLimitAnalyzer()
    {
        Rule = new(
            DiagnosticIds.CSharp.ClassLineLimit,
            "Classes must not exceed the maximum lines-of-code limit",
            "Class '{0}' has {1} lines of code, which exceeds the maximum of 500. Split it into smaller, focused classes.",
            "Maintainability",
            DiagnosticSeverity.Error,
            true,
            "The class body exceeds 500 lines of code (blank lines and comment-only lines are excluded). Split the class into smaller, focused types or extract cohesive groups of members into separate collaborator classes. Consider applying the Single Responsibility Principle to identify natural extraction points.",
            customTags: WellKnownDiagnosticTags.CompilationEnd);
    }

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(RegisterCompilationActions);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private List<TextSpan> CollectBlockCommentSpans(SyntaxNode node)
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

    private void CollectClassPart(
        SyntaxNodeAnalysisContext nodeContext,
        ConcurrentDictionary<INamedTypeSymbol, ConcurrentBag<ClassPart>> partsBySymbol)
    {
        var classDecl = (nodeContext.Node as ClassDeclarationSyntax)!;
        var symbol = nodeContext.SemanticModel.GetDeclaredSymbol(classDecl)!;

        var sourceText = classDecl.SyntaxTree.GetText();
        var loc = CountLinesOfCode(classDecl, sourceText);
        var location = classDecl.Identifier.GetLocation();
        var parts = partsBySymbol.GetOrAdd(symbol, _ => []);
        var classPart = new ClassPart(loc, location);
        parts.Add(classPart);
    }

    private int CountLinesOfCode(ClassDeclarationSyntax classDecl, SourceText sourceText)
    {
        var span = classDecl.Span;
        var startLine = sourceText.Lines.GetLineFromPosition(span.Start).LineNumber;
        var endLine = sourceText.Lines.GetLineFromPosition(span.End).LineNumber;
        var blockCommentSpans = CollectBlockCommentSpans(classDecl);
        var count = 0;

        for (var lineIndex = startLine; lineIndex <= endLine; lineIndex++)
        {
            var firstNonWhitespacePosition = FindFirstNonWhitespace(sourceText, sourceText.Lines[lineIndex].Span);
            if (firstNonWhitespacePosition < 0)
            {
                continue;
            }

            if (HasLineCommentStart(sourceText, firstNonWhitespacePosition, sourceText.Lines[lineIndex].Span.End))
            {
                continue;
            }

            if (HasContainingBlockComment(firstNonWhitespacePosition, blockCommentSpans))
            {
                continue;
            }

            count++;
        }

        return count;
    }

    private int FindFirstNonWhitespace(SourceText sourceText, TextSpan lineSpan)
    {
        for (var position = lineSpan.Start; position < lineSpan.End; position++)
        {
            if (!char.IsWhiteSpace(sourceText[position]))
            {
                return position;
            }
        }

        return -1;
    }

    private bool HasContainingBlockComment(int position, List<TextSpan> blockCommentSpans)
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

    private bool HasEarlierLocation(Location candidate, Location current)
    {
        var candidatePath = candidate.SourceTree!.FilePath;
        var currentPath = current.SourceTree!.FilePath;
        var pathComparison = string.CompareOrdinal(candidatePath, currentPath);

        if (pathComparison != 0)
        {
            return pathComparison < 0;
        }

        return candidate.SourceSpan.Start < current.SourceSpan.Start;
    }

    private bool HasLineCommentStart(SourceText sourceText, int position, int lineEnd)
    {
        return sourceText[position] == '/' && position + 1 < lineEnd && sourceText[position + 1] == '/';
    }

    private void RegisterCompilationActions(CompilationStartAnalysisContext compilationContext)
    {
        var partsBySymbol = new ConcurrentDictionary<INamedTypeSymbol, ConcurrentBag<ClassPart>>(
            SymbolEqualityComparer.Default);

        compilationContext.RegisterSyntaxNodeAction(
            nodeContext => CollectClassPart(nodeContext, partsBySymbol),
            SyntaxKind.ClassDeclaration);

        compilationContext.RegisterCompilationEndAction(endContext => ReportViolations(endContext, partsBySymbol));
    }

    private void ReportViolations(
        CompilationAnalysisContext endContext,
        ConcurrentDictionary<INamedTypeSymbol, ConcurrentBag<ClassPart>> partsBySymbol)
    {
        foreach (var pair in partsBySymbol)
        {
            var totalLoc = 0;
            Location? firstLocation = null;

            foreach (var part in pair.Value)
            {
                totalLoc += part.Loc;
                if (firstLocation is null || HasEarlierLocation(part.Location, firstLocation))
                {
                    firstLocation = part.Location;
                }
            }

            if (totalLoc > MaxLines && firstLocation is not null)
            {
                endContext.ReportDiagnostic(Diagnostic.Create(Rule, firstLocation, pair.Key.Name, totalLoc));
            }
        }
    }
    private readonly struct ClassPart
    {
        public int Loc { get; }

        public Location Location { get; }

        public ClassPart(int loc, Location location)
        {
            Loc = loc;
            Location = location;
        }
    }
}