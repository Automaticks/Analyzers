using Automaticks.CSharp.Naming;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Naming;

/// <summary>
///     Tests for StaticMemberInNonStaticClassAnalyzer.
/// </summary>
public class StaticMemberInNonStaticClassAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_ConstFieldInNonStaticClass_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConstFieldInNonStaticClass_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private const int Limit = 5;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzeAsync(source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS070")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_EmptyConfiguredList_UsesDefaults.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_EmptyConfiguredList_UsesDefaults(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class DiagnosticDescriptor { }
                                  public class Foo {
                                      private static readonly DiagnosticDescriptor Rule;
                                  }
                              }
                              """;
        var options = new Dictionary<string, string>
        {
            ["automaticks.static_member_excluded_types"] = string.Empty,
        };

        var diagnostics = await AnalyzeAsync(source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS070")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExcludedTypeByDefault_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExcludedTypeByDefault_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class DiagnosticDescriptor { }
                                  public class Foo {
                                      private static readonly DiagnosticDescriptor Rule;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzeAsync(source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS070")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_GenericFieldTypeMatchesConfiguredName_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_GenericFieldTypeMatchesConfiguredName_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Foo {
                                      private static readonly List<string> Names = null!;
                                  }
                              }
                              """;
        var options = new Dictionary<string, string>
        {
            ["automaticks.static_member_excluded_types"] = "List",
        };

        var diagnostics = await AnalyzeAsync(source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS070")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_MultipleDeclaratorsOnOneField_ReportsOnePerDeclarator.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MultipleDeclaratorsOnOneField_ReportsOnePerDeclarator(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private static int first, second;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzeAsync(source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.CountId(diagnostics, "ATXCS070")).IsEqualTo(2);
    }

    /// <summary>
    ///     Tests that Analyze_NullableArrayFieldOfExcludedType_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NullableArrayFieldOfExcludedType_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Marker { }
                                  public class Foo {
                                      private static readonly Marker[]? Markers;
                                  }
                              }
                              """;
        var options = new Dictionary<string, string>
        {
            ["automaticks.static_member_excluded_types"] = " Marker , ",
        };

        var diagnostics = await AnalyzeAsync(source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS070")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_QualifiedFieldTypeMatchesConfiguredName_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_QualifiedFieldTypeMatchesConfiguredName_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private static readonly System.Text.StringBuilder Builder = null!;
                                  }
                              }
                              """;
        var options = new Dictionary<string, string>
        {
            ["automaticks.static_member_excluded_types"] = "StringBuilder",
        };

        var diagnostics = await AnalyzeAsync(source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS070")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_StaticFieldInNonStaticClass_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StaticFieldInNonStaticClass_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private static readonly int Limit = 5;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzeAsync(source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS070")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_StaticFieldInStaticClass_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StaticFieldInStaticClass_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public static class Foo {
                                      private static readonly int Limit = 5;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzeAsync(source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS070")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_StaticFieldInStruct_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StaticFieldInStruct_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public struct Foo {
                                      private static readonly int Limit = 5;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzeAsync(source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS070")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_StaticPropertyInNonStaticClass_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StaticPropertyInNonStaticClass_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public static int Limit => 5;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzeAsync(source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS070")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_StaticPropertyInStaticClass_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StaticPropertyInStaticClass_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public static class Foo {
                                      public static int Limit => 5;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzeAsync(source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS070")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_StaticPropertyInStruct_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StaticPropertyInStruct_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public struct Foo {
                                      public static int Limit => 5;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzeAsync(source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS070")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_StaticPropertyOfExcludedType_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StaticPropertyOfExcludedType_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class DiagnosticDescriptor { }
                                  public class Foo {
                                      public static DiagnosticDescriptor Rule => null!;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzeAsync(source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS070")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_TupleFieldType_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TupleFieldType_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private static (int, int) Pair;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzeAsync(source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS070")).IsTrue();
    }

    private async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(
        string source,
        CancellationToken cancellationToken)
    {
        var analyzer = new StaticMemberInNonStaticClassAnalyzer();
        return await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);
    }

    private async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(
        string source,
        IReadOnlyDictionary<string, string> configOptions,
        CancellationToken cancellationToken)
    {
        var analyzer = new StaticMemberInNonStaticClassAnalyzer();
        var options = new AnalysisOptions
        {
            ConfigOptions = configOptions,
        };
        return await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, options, cancellationToken);
    }
}
