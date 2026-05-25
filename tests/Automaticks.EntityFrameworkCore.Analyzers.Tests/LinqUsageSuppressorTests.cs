using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Threading.Tasks;
using Automaticks.Linq;
using Automaticks.EntityFrameworkCore;

namespace Automaticks.EntityFrameworkCore.Analyzers.Tests;

/// <summary>
///     Tests for <see cref="LinqUsageSuppressor" />.
///     Note: <see cref="Microsoft.CodeAnalysis.DiagnosticSuppressor" /> is only invoked by the
///     IDE/MSBuild host — not by <c>CompilationWithAnalyzers.GetAnalyzerDiagnosticsAsync()</c>.
///     Tests here therefore verify (a) the suppressor's declared metadata, (b) the EF Core
///     detection syntax logic, and (c) that <see cref="LinqUsageAnalyzer" /> continues to
///     report <c>ATXLQ002</c> in files without an EF Core import (proving no false suppression).
/// </summary>
public class LinqUsageSuppressorTests
{
    [Test]
    public async Task Suppressor_SupportedSuppressions_DeclaresAtxEf001()
    {
        var suppressor = new LinqUsageSuppressor();

        await Assert.That(suppressor.SupportedSuppressions.Length).IsEqualTo(1);
        await Assert.That(suppressor.SupportedSuppressions[0].Id).IsEqualTo(SuppressionIds.EFCore.LinqUsage);
        await Assert.That(suppressor.SupportedSuppressions[0].SuppressedDiagnosticId).IsEqualTo("ATXLQ002");
    }

    [Test]
    public async Task Suppressor_EFCoreRootImport_IsDetectedAsEFCore()
    {
        const string source = """
                              using System.Linq;
                              using Microsoft.EntityFrameworkCore;
                              namespace A {}
                              """;

        await Assert.That(HasEFCoreImport(source)).IsTrue();
    }

    [Test]
    public async Task Suppressor_EFCoreSubNamespaceImport_IsDetectedAsEFCore()
    {
        const string source = """
                              using System.Linq;
                              using Microsoft.EntityFrameworkCore.Query;
                              namespace A {}
                              """;

        await Assert.That(HasEFCoreImport(source)).IsTrue();
    }

    [Test]
    public async Task Suppressor_NoEFCoreImport_IsNotDetectedAsEFCore()
    {
        const string source = """
                              using System.Linq;
                              namespace A {}
                              """;

        await Assert.That(HasEFCoreImport(source)).IsFalse();
    }

    [Test]
    public async Task Analyze_LinqInFileWithoutEFCoreImport_ReportsDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new LinqUsageAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXLQ002")).IsTrue();
    }

    /// <summary>
    ///     Replicates the EF Core import detection logic from <see cref="LinqUsageSuppressor.ReportSuppressions" />
    ///     so it can be exercised directly without requiring an IDE/MSBuild suppressor host.
    /// </summary>
    private static bool HasEFCoreImport(string source)
    {
        var root = (CompilationUnitSyntax)CSharpSyntaxTree.ParseText(source).GetRoot();
        return root.Usings.Any(u =>
        {
            var name = u.Name?.ToString() ?? string.Empty;
            return name.Equals("Microsoft.EntityFrameworkCore", StringComparison.Ordinal) ||
                   name.StartsWith("Microsoft.EntityFrameworkCore.", StringComparison.Ordinal);
        });
    }
}
