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
    ///     Tests that Analyze_ConfiguredMutableType_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConfiguredMutableType_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Cache { }
                                  public class Foo {
                                      private static readonly Cache Store = null!;
                                  }
                              }
                              """;
        var options = new Dictionary<string, string>
        {
            ["automaticks.static_member_mutable_types"] = "Cache",
        };

        var diagnostics = await AnalyzeAsync(source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS070")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ConstField_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConstField_ReportsNoDiagnostic(CancellationToken cancellationToken)
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
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Foo {
                                      private static readonly List<string> Names = null!;
                                  }
                              }
                              """;
        var options = new Dictionary<string, string>
        {
            ["automaticks.static_member_mutable_types"] = string.Empty,
        };

        var diagnostics = await AnalyzeAsync(source, options, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS070")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ExpressionBodiedStaticProperty_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExpressionBodiedStaticProperty_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public static int Limit => 5;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzeAsync(source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS070")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_GetOnlyStaticPropertyOfImmutableType_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_GetOnlyStaticPropertyOfImmutableType_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public static int Limit { get; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzeAsync(source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS070")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_GetOnlyStaticPropertyOfMutableType_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_GetOnlyStaticPropertyOfMutableType_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Foo {
                                      public static List<string> Names { get; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzeAsync(source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS070")).IsTrue();
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
    ///     Tests that Analyze_MutableStaticFieldInStaticClass_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MutableStaticFieldInStaticClass_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public static class Foo {
                                      private static int Counter;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzeAsync(source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS070")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_MutableStaticFieldInStruct_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MutableStaticFieldInStruct_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public struct Foo {
                                      private static int Counter;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzeAsync(source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS070")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_MutableStaticPropertyInStaticClass_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MutableStaticPropertyInStaticClass_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public static class Foo {
                                      public static int Limit { get; set; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzeAsync(source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS070")).IsFalse();
    }
    /// <summary>
    ///     Tests that Analyze_NonReadOnlyStaticField_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NonReadOnlyStaticField_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private static int Counter;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzeAsync(source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS070")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_NullableMutableStaticField_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NullableMutableStaticField_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Text;
                              namespace MyApp {
                                  public class Foo {
                                      private static readonly StringBuilder? Builder;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzeAsync(source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS070")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_QualifiedMutableStaticField_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_QualifiedMutableStaticField_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private static readonly System.Collections.Generic.Dictionary<string, int> Map = null!;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzeAsync(source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS070")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ReadOnlyStaticArrayField_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ReadOnlyStaticArrayField_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private static readonly string[] Names = null!;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzeAsync(source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS070")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ReadOnlyStaticFieldOfImmutableType_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ReadOnlyStaticFieldOfImmutableType_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections.Immutable;
                              namespace MyApp {
                                  public class Foo {
                                      private static readonly ImmutableArray<string> Names;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzeAsync(source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS070")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ReadOnlyStaticFieldOfUnknownType_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ReadOnlyStaticFieldOfUnknownType_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Marker { }
                                  public class Foo {
                                      private static readonly Marker Instance = null!;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzeAsync(source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS070")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_StaticPropertyWithGetterBody_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StaticPropertyWithGetterBody_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Collections.Generic;
                              namespace MyApp {
                                  public class Foo {
                                      public static List<string> Names { get { return null!; } }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzeAsync(source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS070")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_StaticPropertyWithInitAccessor_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StaticPropertyWithInitAccessor_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public static int Limit { get; init; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzeAsync(source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS070")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_StaticPropertyWithSetter_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StaticPropertyWithSetter_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public static int Limit { get; set; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzeAsync(source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS070")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_TupleTypedStaticField_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TupleTypedStaticField_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private static readonly (int, int) Pair;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzeAsync(source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS070")).IsFalse();
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
