using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.CodeFixes.LanguageFeatures;

/// <summary>
///     Simplifies the namespace-qualified reference reported by ATXCS072, adding a <c>using</c> directive when needed.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(QualifiedTypeReferenceCodeFixProvider))]
[Shared]
public sealed class QualifiedTypeReferenceCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds
    {
        get
        {
            return [DiagnosticIds.CSharp.NamespaceQualifiedTypeReference];
        }
    }

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var semanticModel = (await context.Document.GetSemanticModelAsync(context.CancellationToken))!;
        var root = (await context.Document.GetSyntaxRootAsync(context.CancellationToken))!;
        foreach (var diagnostic in context.Diagnostics)
        {
            var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
            if (!HasClassification(node, semanticModel, out var classification))
            {
                continue;
            }

            var namespaceNameToAdd = classification.NamespaceNameToAdd;
            var simpleName = classification.SimpleName;
            var title = namespaceNameToAdd is null
                ? $"Simplify to '{simpleName}'"
                : $"Simplify to '{simpleName}' and add 'using {namespaceNameToAdd};'";
            var nodeSpan = node.Span;
            var action = CodeAction.Create(
                title,
                cancellationToken => SimplifyAsync(context.Document, nodeSpan, namespaceNameToAdd, cancellationToken),
                title);
            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private UsingDirectiveSyntax BuildUsingDirective(string namespaceName, SyntaxTriviaList trailingTrivia)
    {
        var name = SyntaxFactory.ParseName(namespaceName);
        return SyntaxFactory.UsingDirective(name).WithTrailingTrivia(trailingTrivia);
    }

    private int FindInsertionIndex(SyntaxList<UsingDirectiveSyntax> usings, string namespaceName)
    {
        var lastRegularIndex = -1;
        for (var index = 0; index < usings.Count; index++)
        {
            if (!IsRegularUsingDirective(usings[index]))
            {
                continue;
            }

            lastRegularIndex = index;
            var existingName = usings[index].Name!.ToString();
            if (string.Compare(namespaceName, existingName, StringComparison.OrdinalIgnoreCase) < 0)
            {
                return index;
            }
        }

        return lastRegularIndex + 1;
    }

    /// <summary>
    ///     Splits a reference into its left part and simple name for either syntax shape.
    /// </summary>
    private QualificationParts GetQualificationParts(SyntaxNode node)
    {
        if (node is QualifiedNameSyntax qualifiedName)
        {
            return new QualificationParts(qualifiedName.Left, qualifiedName.Right);
        }

        var memberAccess = (node as MemberAccessExpressionSyntax)!;
        return new QualificationParts(memberAccess.Expression, memberAccess.Name);
    }

    private bool HasClassification(SyntaxNode node, SemanticModel semanticModel, out ClassificationResult classification)
    {
        classification = new ClassificationResult(null, string.Empty);

        var parts = GetQualificationParts(node);
        var targetType = (semanticModel.GetSymbolInfo(node).Symbol as INamedTypeSymbol)!;
        var namespaceSymbol = (semanticModel.GetSymbolInfo(parts.Left).Symbol as INamespaceSymbol)!;
        var simpleName = parts.SimpleName.Identifier.ValueText;
        var visibleSymbols = semanticModel.LookupNamespacesAndTypes(node.SpanStart, null, simpleName);

        if (HasMatchingType(visibleSymbols, targetType))
        {
            classification = new ClassificationResult(null, simpleName);
            return true;
        }

        if (visibleSymbols.Length == 0)
        {
            classification = new ClassificationResult(namespaceSymbol.ToDisplayString(), simpleName);
            return true;
        }

        return false;
    }

    private bool HasMatchingType(ImmutableArray<ISymbol> visibleSymbols, INamedTypeSymbol targetType)
    {
        foreach (var visibleSymbol in visibleSymbols)
        {
            if (visibleSymbol is INamedTypeSymbol namedType
                && SymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, targetType.OriginalDefinition))
            {
                return true;
            }
        }

        return false;
    }

    private CompilationUnitSyntax InsertUsingDirective(CompilationUnitSyntax compilationUnit, string namespaceName)
    {
        var usings = compilationUnit.Usings;
        var trailingTrivia = usings.Count > 0
            ? usings[0].GetTrailingTrivia()
            : SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine("\n"));
        var newUsingDirective = BuildUsingDirective(namespaceName, trailingTrivia);
        var insertionIndex = FindInsertionIndex(usings, namespaceName);
        return compilationUnit.WithUsings(usings.Insert(insertionIndex, newUsingDirective));
    }

    private bool IsRegularUsingDirective(UsingDirectiveSyntax usingDirective)
    {
        return !usingDirective.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword)
            && !usingDirective.StaticKeyword.IsKind(SyntaxKind.StaticKeyword)
            && usingDirective.Alias is null;
    }

    private async Task<Document> SimplifyAsync(
        Document document,
        TextSpan nodeSpan,
        string? namespaceNameToAdd,
        CancellationToken cancellationToken)
    {
        var compilationUnit = (await document.GetSyntaxRootAsync(cancellationToken) as CompilationUnitSyntax)!;
        var flaggedNode = compilationUnit.FindNode(nodeSpan, getInnermostNodeForTie: true);
        var parts = GetQualificationParts(flaggedNode);
        var replaced = compilationUnit.ReplaceNode(flaggedNode, parts.SimpleName.WithTriviaFrom(flaggedNode));
        if (namespaceNameToAdd is null)
        {
            return document.WithSyntaxRoot(replaced);
        }

        var withUsing = InsertUsingDirective(replaced, namespaceNameToAdd);
        return document.WithSyntaxRoot(withUsing);
    }

    /// <summary>
    ///     The classification outcome for a flagged reference: its simple name and namespace to import, if any.
    /// </summary>
    private readonly struct ClassificationResult
    {
        /// <summary>
        ///     Gets the namespace to add as a <c>using</c> directive, or <see langword="null" /> when none is needed.
        /// </summary>
        public string? NamespaceNameToAdd { get; }

        /// <summary>
        ///     Gets the simple type name the qualified reference simplifies to.
        /// </summary>
        public string SimpleName { get; }

        public ClassificationResult(string? namespaceNameToAdd, string simpleName)
        {
            NamespaceNameToAdd = namespaceNameToAdd;
            SimpleName = simpleName;
        }
    }

    /// <summary>
    ///     The left (namespace) part and simple name part of a qualified reference.
    /// </summary>
    private readonly struct QualificationParts
    {
        /// <summary>
        ///     Gets the left-hand side of the qualification, expected to resolve to a namespace.
        /// </summary>
        public SyntaxNode Left { get; }

        /// <summary>
        ///     Gets the simple type name on the right-hand side of the qualification.
        /// </summary>
        public SimpleNameSyntax SimpleName { get; }

        public QualificationParts(SyntaxNode left, SimpleNameSyntax simpleName)
        {
            Left = left;
            SimpleName = simpleName;
        }
    }
}
