using Automaticks.CSharp.CodeFixes.Documentation;
using Automaticks.CSharp.CodeFixes.Formatting;
using Automaticks.CSharp.CodeFixes.Naming;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.CodeFixes;

/// <summary>
///     Verifies every shipped code fix supports the document, project, and solution Fix All
///     scopes, and that a document-scoped Fix All clears all occurrences in one pass.
/// </summary>
public class CodeFixTestRunnerFixAllScopeTests
{
    private const string ManyInternalTypesSource = "namespace MyApp {\n    internal class First { }\n    internal class Second { }\n    internal class Third { }\n}\n";
    private const string ManyUnusedUsingsSource = "using System.Text;\nusing System.IO;\nusing System.Net;\n\nnamespace MyApp {\n    public class Foo { }\n}\n";

    /// <summary>
    ///     Tests that a document-scoped Fix All clears every internal modifier at once.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFixAll_DocumentScopeOnInternalTypes_FixesEveryOccurrence(CancellationToken cancellationToken)
    {
        var analyzer = new InternalModifierAnalyzer();
        var provider = new InternalModifierCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = ManyInternalTypesSource
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAllAsync(request, FixAllScope.Document, cancellationToken);

        await Assert.That(fixedSource).DoesNotContain("internal");
        await Assert.That(fixedSource).Contains("public class First");
        await Assert.That(fixedSource).Contains("public class Second");
        await Assert.That(fixedSource).Contains("public class Third");
    }

    /// <summary>
    ///     Tests that a document-scoped Fix All removes every unused using at once.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFixAll_DocumentScopeOnUnusedUsings_FixesEveryOccurrence(CancellationToken cancellationToken)
    {
        var analyzer = new UnusedUsingDirectiveAnalyzer();
        var provider = new UnusedUsingDirectiveCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = ManyUnusedUsingsSource
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAllAsync(request, FixAllScope.Document, cancellationToken);

        await Assert.That(fixedSource).DoesNotContain("using System.Text;");
        await Assert.That(fixedSource).DoesNotContain("using System.IO;");
        await Assert.That(fixedSource).DoesNotContain("using System.Net;");
    }

    /// <summary>
    ///     Tests that a project-scoped Fix All is accepted and applies the fixes.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFixAll_ProjectScope_FixesEveryOccurrence(CancellationToken cancellationToken)
    {
        var analyzer = new InternalModifierAnalyzer();
        var provider = new InternalModifierCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = ManyInternalTypesSource
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAllAsync(request, FixAllScope.Project, cancellationToken);

        await Assert.That(fixedSource).DoesNotContain("internal");
    }

    /// <summary>
    ///     Tests that a solution-scoped Fix All is accepted and applies the fixes.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyFixAll_SolutionScope_FixesEveryOccurrence(CancellationToken cancellationToken)
    {
        var analyzer = new InternalModifierAnalyzer();
        var provider = new InternalModifierCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = ManyInternalTypesSource
        };
        var fixedSource = await CodeFixTestRunner.ApplyFixAllAsync(request, FixAllScope.Solution, cancellationToken);

        await Assert.That(fixedSource).DoesNotContain("internal");
    }

    /// <summary>
    ///     Tests that a syntax based fix supports all three standard Fix All scopes.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetSupportedFixAllScopes_SyntaxBasedFix_IncludesDocumentProjectAndSolution(CancellationToken cancellationToken)
    {
        var analyzer = new RemarksXmlDocAnalyzer();
        var provider = new RemarksXmlDocCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = ManyInternalTypesSource
        };
        var scopes = CodeFixTestRunner.GetSupportedFixAllScopes(request);

        await Assert.That(scopes.Contains(FixAllScope.Document)).IsTrue();
        await Assert.That(scopes.Contains(FixAllScope.Project)).IsTrue();
        await Assert.That(scopes.Contains(FixAllScope.Solution)).IsTrue();
    }

    /// <summary>
    ///     Tests that a text based fix also supports all three standard Fix All scopes.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetSupportedFixAllScopes_TextBasedFix_IncludesDocumentProjectAndSolution(CancellationToken cancellationToken)
    {
        var analyzer = new ConsecutiveBlankLinesAnalyzer();
        var provider = new ConsecutiveBlankLinesCodeFixProvider();
        var request = new CodeFixRequest
        {
            Analyzer = analyzer,
            Provider = provider,
            Source = ManyUnusedUsingsSource
        };
        var scopes = CodeFixTestRunner.GetSupportedFixAllScopes(request);

        await Assert.That(scopes.Contains(FixAllScope.Document)).IsTrue();
        await Assert.That(scopes.Contains(FixAllScope.Project)).IsTrue();
        await Assert.That(scopes.Contains(FixAllScope.Solution)).IsTrue();
    }
}
