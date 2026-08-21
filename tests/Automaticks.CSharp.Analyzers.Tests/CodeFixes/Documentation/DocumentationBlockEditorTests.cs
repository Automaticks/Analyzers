using Automaticks.CSharp.CodeFixes.Documentation;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes.Documentation;

/// <summary>
///     Tests for DocumentationBlockEditor.
/// </summary>
public class DocumentationBlockEditorTests
{
    /// <summary>
    ///     Tests that a document without any line break falls back to a newline separator.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InsertLine_DocumentWithoutLineBreaks_FallsBackToNewline(CancellationToken cancellationToken)
    {
        const string source = "public class C { public void M() { } }";
        var tree = CSharpSyntaxTree.ParseText(source, cancellationToken: cancellationToken);
        var compilationUnit = tree.GetRoot(cancellationToken) as CompilationUnitSyntax
            ?? throw new InvalidOperationException("Expected a compilation unit.");
        var classDeclaration = compilationUnit.Members[0] as ClassDeclarationSyntax
            ?? throw new InvalidOperationException("Expected a class declaration.");
        var method = classDeclaration.Members[0] as MethodDeclarationSyntax
            ?? throw new InvalidOperationException("Expected a method declaration.");
        var text = tree.GetText(cancellationToken);

        var result = DocumentationBlockEditor.InsertLine(text, method, "/// <returns></returns>");

        await Assert.That(result.ToString()).Contains("/// <returns></returns>\n");
    }
}
