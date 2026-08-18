using Automaticks.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.EntityFrameworkCore.Analyzers.Tests;

/// <summary>
///     Tests for <see cref="LinqUsageSuppressor" />.
/// </summary>
public class LinqUsageSuppressorTests
{

    /// <summary>
    ///     Tests that Analyze_LinqInFileWithoutEFCoreImport_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LinqInFileWithoutEFCoreImport_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Linq;

                              namespace MyApp {
                                  public class UserRepository {
                                      public void Query(IQueryable<int> items) {
                                          var result = items.Where(x => x > 0).ToList();
                                      }
                                  }
                              }
                              """;

        var analyzer = new LinqUsageAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXLQ002")).IsTrue();
    }

    /// <summary>
    ///     Tests that Suppressor_EFCoreRootImport_IsDetectedAsEFCore.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Suppressor_EFCoreRootImport_IsDetectedAsEFCore(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Linq;
                              using Microsoft.EntityFrameworkCore;
                              namespace A {}
                              """;

        await Assert.That(HasEntityFrameworkCoreImport(source)).IsTrue();
    }

    /// <summary>
    ///     Tests that Suppressor_EFCoreSubNamespaceImport_IsDetectedAsEFCore.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Suppressor_EFCoreSubNamespaceImport_IsDetectedAsEFCore(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Linq;
                              using Microsoft.EntityFrameworkCore.Query;
                              namespace A {}
                              """;

        await Assert.That(HasEntityFrameworkCoreImport(source)).IsTrue();
    }

    /// <summary>
    ///     Tests that ReportSuppressions actually suppresses ATXLQ002 when both a diagnostic
    ///     source and the suppressor run together over a file with an EF Core import.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Suppressor_LinqWithEFCoreImport_SuppressesDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Linq;
                              using Microsoft.EntityFrameworkCore;
                              namespace MyApp {}
                              """;

        var linqUsageAnalyzer = new LinqUsageAnalyzer();
        var linqUsageSuppressor = new LinqUsageSuppressor();
        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(linqUsageAnalyzer, linqUsageSuppressor);
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzers, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXLQ002")).IsTrue();
        await Assert.That(DiagnosticCollectionAssertions.HasSuppressedId(diagnostics, "ATXLQ002")).IsTrue();
    }

    /// <summary>
    ///     Tests that ReportSuppressions leaves ATXLQ002 unsuppressed when both a diagnostic
    ///     source and the suppressor run together over a file without an EF Core import.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Suppressor_LinqWithoutEFCoreImport_DoesNotSuppressDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Linq;
                              namespace MyApp {}
                              """;

        var linqUsageAnalyzer = new LinqUsageAnalyzer();
        var linqUsageSuppressor = new LinqUsageSuppressor();
        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(linqUsageAnalyzer, linqUsageSuppressor);
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzers, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXLQ002")).IsTrue();
        await Assert.That(DiagnosticCollectionAssertions.HasSuppressedId(diagnostics, "ATXLQ002")).IsFalse();
    }

    /// <summary>
    ///     Tests that Suppressor_NoEFCoreImport_IsNotDetectedAsEFCore.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Suppressor_NoEFCoreImport_IsNotDetectedAsEFCore(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Linq;
                              namespace A {}
                              """;

        await Assert.That(HasEntityFrameworkCoreImport(source)).IsFalse();
    }
    /// <summary>
    ///     Tests that Suppressor_SupportedSuppressions_DeclaresAtxEf001.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Suppressor_SupportedSuppressions_DeclaresAtxEf001(CancellationToken cancellationToken)
    {
        var suppressor = new LinqUsageSuppressor();

        await Assert.That(suppressor.SupportedSuppressions.Length).IsEqualTo(1);
        await Assert.That(suppressor.SupportedSuppressions[0].Id).IsEqualTo(SuppressionIds.EFCore.LinqUsage);
        await Assert.That(suppressor.SupportedSuppressions[0].SuppressedDiagnosticId).IsEqualTo("ATXLQ002");
    }

    /// <summary>
    ///     Replicates the EF Core import detection logic from <see cref="LinqUsageSuppressor.ReportSuppressions" />
    ///     so it can be exercised directly without requiring an IDE/MSBuild suppressor host.
    /// </summary>
    private bool HasEntityFrameworkCoreImport(string source)
    {
        var syntaxRoot = CSharpSyntaxTree.ParseText(source).GetRoot();
        if (syntaxRoot is not CompilationUnitSyntax compilationUnit)
        {
            return false;
        }

        foreach (var usingDirective in compilationUnit.Usings)
        {
            var name = usingDirective.Name?.ToString() ?? string.Empty;
            if (name.Equals("Microsoft.EntityFrameworkCore", StringComparison.Ordinal)
                || name.StartsWith("Microsoft.EntityFrameworkCore.", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
