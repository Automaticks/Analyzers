using Automaticks.Testing.Testability;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Testing.Analyzers.Tests;

/// <summary>
///     Tests for CompositeBitmaskTestAnalyzer.
/// </summary>
public class CompositeBitmaskTestAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_ComparisonOperatorNotEqualityOrInequality_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ComparisonOperatorNotEqualityOrInequality_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool Bar(int flags) { return (flags & 6) > 0; }
                                  }
                              }
                              """;

        var analyzer = new CompositeBitmaskTestAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST006")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_CompositeMaskComparedToZero_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CompositeMaskComparedToZero_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool Bar(int flags) { return (flags & 6) != 0; }
                                  }
                              }
                              """;

        var analyzer = new CompositeBitmaskTestAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(
            diagnostics,
            "ATXTST006",
            "2 bits set")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_CompositeMaskOnLeftSide_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CompositeMaskOnLeftSide_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool Bar(int flags) { return (0xF0 & flags) == 0; }
                                  }
                              }
                              """;

        var analyzer = new CompositeBitmaskTestAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST006")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_CompositeMaskUsedAsValue_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CompositeMaskUsedAsValue_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Bar(int value) { return value & 0xFF; }
                                  }
                              }
                              """;

        var analyzer = new CompositeBitmaskTestAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST006")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_HasFlagArgumentCountMismatch_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_HasFlagArgumentCountMismatch_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Bar(int a, int b) { return System.Math.Max(a, b); }
                                  }
                              }
                              """;

        var analyzer = new CompositeBitmaskTestAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST006")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_HasFlagOnNonEnumType_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_HasFlagOnNonEnumType_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public struct CustomFlags {
                                      public bool HasFlag(CustomFlags other) { return false; }
                                  }
                                  public class Foo {
                                      public bool Bar(CustomFlags flags) { return flags.HasFlag(flags); }
                                  }
                              }
                              """;

        var analyzer = new CompositeBitmaskTestAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST006")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_HasFlagWithCompositeValue_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_HasFlagWithCompositeValue_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  [Flags]
                                  public enum Access { None = 0, Read = 1, Write = 2 }
                                  public class Foo {
                                      public bool Bar(Access access) { return access.HasFlag(Access.Read | Access.Write); }
                                  }
                              }
                              """;

        var analyzer = new CompositeBitmaskTestAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST006")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_HasFlagWithSingleBit_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_HasFlagWithSingleBit_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  [Flags]
                                  public enum Access { None = 0, Read = 1, Write = 2 }
                                  public class Foo {
                                      public bool Bar(Access access) { return access.HasFlag(Access.Read); }
                                  }
                              }
                              """;

        var analyzer = new CompositeBitmaskTestAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST006")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_NeitherSideIsZeroLiteral_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NeitherSideIsZeroLiteral_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool Bar(int flags) { return (flags & 6) != 1; }
                                  }
                              }
                              """;

        var analyzer = new CompositeBitmaskTestAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST006")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_NonConstantMask_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NonConstantMask_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool Bar(int flags, int mask) { return (flags & mask) != 0; }
                                  }
                              }
                              """;

        var analyzer = new CompositeBitmaskTestAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST006")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_OneArgumentMethodNotNamedHasFlag_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_OneArgumentMethodNotNamedHasFlag_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Bar(string text) { return int.Parse(text); }
                                  }
                              }
                              """;

        var analyzer = new CompositeBitmaskTestAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST006")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_SingleBitMaskComparedToZero_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SingleBitMaskComparedToZero_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool Bar(int flags) { return (flags & 4) != 0; }
                                  }
                              }
                              """;

        var analyzer = new CompositeBitmaskTestAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST006")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_UnresolvedInvocationShapedAsHasFlag_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_UnresolvedInvocationShapedAsHasFlag_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool Bar(int flags) { return Undefined(flags); }
                                  }
                              }
                              """;

        var analyzer = new CompositeBitmaskTestAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST006")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ZeroLiteralOnLeftSide_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ZeroLiteralOnLeftSide_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool Bar(int flags) { return 0 != (flags & 6); }
                                  }
                              }
                              """;

        var analyzer = new CompositeBitmaskTestAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(
            diagnostics,
            "ATXTST006",
            "2 bits set")).IsTrue();
    }
}
