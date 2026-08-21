using Automaticks.Testing.Testability;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.Testing.Analyzers.Tests;

/// <summary>
///     Tests for CompositeBitmaskTestAnalyzer covering each enum underlying type.
/// </summary>
public class CompositeBitmaskTestAnalyzerUnderlyingTypeTests
{
    /// <summary>
    ///     Tests that Analyze_HasFlagWithByteUnderlyingType_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_HasFlagWithByteUnderlyingType_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  [Flags]
                                  public enum Access : byte { None = 0, Read = 1, Write = 2 }
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
    ///     Tests that Analyze_HasFlagWithInt16UnderlyingType_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_HasFlagWithInt16UnderlyingType_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  [Flags]
                                  public enum Access : short { None = 0, Read = 1, Write = 2 }
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
    ///     Tests that Analyze_HasFlagWithInt64UnderlyingType_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_HasFlagWithInt64UnderlyingType_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  [Flags]
                                  public enum Access : long { None = 0, Read = 1, Write = 2 }
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
    ///     Tests that Analyze_HasFlagWithSignedByteUnderlyingType_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_HasFlagWithSignedByteUnderlyingType_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  [Flags]
                                  public enum Access : sbyte { None = 0, Read = 1, Write = 2 }
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
    ///     Tests that Analyze_HasFlagWithUnsignedInt16UnderlyingType_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_HasFlagWithUnsignedInt16UnderlyingType_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  [Flags]
                                  public enum Access : ushort { None = 0, Read = 1, Write = 2 }
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
    ///     Tests that Analyze_HasFlagWithUnsignedInt32UnderlyingType_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_HasFlagWithUnsignedInt32UnderlyingType_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  [Flags]
                                  public enum Access : uint { None = 0, Read = 1, Write = 2 }
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
    ///     Tests that Analyze_HasFlagWithUnsignedInt64UnderlyingType_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_HasFlagWithUnsignedInt64UnderlyingType_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  [Flags]
                                  public enum Access : ulong { None = 0, Read = 1, Write = 2 }
                                  public class Foo {
                                      public bool Bar(Access access) { return access.HasFlag(Access.Read | Access.Write); }
                                  }
                              }
                              """;

        var analyzer = new CompositeBitmaskTestAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXTST006")).IsTrue();
    }
}
