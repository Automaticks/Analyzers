using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.CodeFixes.Documentation;

/// <summary>
///     Adds an empty <c>&lt;param&gt;</c> element for the undocumented parameter reported by
///     ATXCS052.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MissingParamXmlDocCodeFixProvider))]
[Shared]
public sealed class MissingParamXmlDocCodeFixProvider : CodeFixProvider
{
    private const string Title = "Add an empty <param> element";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticIds.CSharp.MissingParamXmlDoc];

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = (await context.Document.GetSyntaxRootAsync(context.CancellationToken))!;

        foreach (var diagnostic in context.Diagnostics)
        {
            var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
            var parameter = node.FirstAncestorOrSelf<ParameterSyntax>()!;
            var member = parameter.FirstAncestorOrSelf<MemberDeclarationSyntax>()!;

            var parameterName = parameter.Identifier.ValueText;
            var action = CodeAction.Create(
                Title,
                cancellationToken => AddParamAsync(context.Document, member, parameterName, cancellationToken),
                $"{Title}:{parameterName}");
            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private async Task<Document> AddParamAsync(
        Document document,
        MemberDeclarationSyntax member,
        string parameterName,
        CancellationToken cancellationToken)
    {
        var text = await document.GetTextAsync(cancellationToken);
        var element = $"/// <param name=\"{parameterName}\"></param>";
        var newText = DocumentationBlockEditor.InsertLine(text, member, element);
        return document.WithText(newText);
    }
}
