using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Stubs;

/// <summary>
///     Feeds a fixed diagnostic set to a Fix All operation during tests.
/// </summary>
public sealed class TestFixAllDiagnosticProvider : FixAllContext.DiagnosticProvider
{
    private readonly IReadOnlyList<Diagnostic> diagnostics;

    /// <summary>
    ///     Initializes the provider with the diagnostics to serve.
    /// </summary>
    /// <param name="diagnostics">The diagnostics returned for every requested scope.</param>
    public TestFixAllDiagnosticProvider(IReadOnlyList<Diagnostic> diagnostics)
    {
        this.diagnostics = diagnostics;
    }

    /// <inheritdoc />
    public override Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(Project project, CancellationToken cancellationToken)
    {
        return Task.FromResult<IEnumerable<Diagnostic>>(diagnostics);
    }

    /// <inheritdoc />
    public override Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync(Document document, CancellationToken cancellationToken)
    {
        return Task.FromResult<IEnumerable<Diagnostic>>(diagnostics);
    }

    /// <inheritdoc />
    public override Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(Project project, CancellationToken cancellationToken)
    {
        return Task.FromResult<IEnumerable<Diagnostic>>(diagnostics);
    }
}
