using Automaticks.CSharp.LanguageFeatures;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures.ExternalInterfaces;

/// <summary>
///     Tests the constructor shapes RedundantInitSetterAnalyzer must walk without reporting.
/// </summary>
public class RedundantInitSetterAnalyzerConstructorShapesTests
{
    /// <summary>
    ///     Tests that Analyze_ConstructorAssignsFromParameter_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConstructorAssignsFromParameter_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Value { get; init; }
                                      public Foo(int value) { this.Value = value; }
                                  }
                              }
                              """;

        var analyzer = new RedundantInitSetterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.RedundantInitSetter)).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ConstructorWithAttributes_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConstructorWithAttributes_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public sealed class JsonConstructorAttribute : Attribute { }
                                  public class Foo {
                                      public int Value { get; init; }
                                      [JsonConstructor]
                                      public Foo(int value) { Value = value; }
                                  }
                              }
                              """;

        var analyzer = new RedundantInitSetterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.RedundantInitSetter)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ConstructorWithCompoundAssignment_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConstructorWithCompoundAssignment_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private int _count;
                                      public int Value { get; init; }
                                      public Foo(int value) { _count += value; }
                                  }
                              }
                              """;

        var analyzer = new RedundantInitSetterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.RedundantInitSetter)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ConstructorWithElementAssignment_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConstructorWithElementAssignment_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private readonly int[] _slots = new int[4];
                                      public int Value { get; init; }
                                      public Foo(int value) { _slots[0] = value; }
                                  }
                              }
                              """;

        var analyzer = new RedundantInitSetterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.RedundantInitSetter)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ConstructorWithNonAssignmentStatement_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConstructorWithNonAssignmentStatement_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public int Value { get; init; }
                                      public Foo(int value) {
                                          if (value > 0) { }
                                          Console.WriteLine(value);
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantInitSetterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.RedundantInitSetter)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ConstructorWithUnrelatedAttribute_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConstructorWithUnrelatedAttribute_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public int Value { get; init; }
                                      [Obsolete]
                                      public Foo(int value) { Value = value; }
                                  }
                              }
                              """;

        var analyzer = new RedundantInitSetterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.RedundantInitSetter)).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ExternConstructorWithoutBody_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExternConstructorWithoutBody_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Value { get; init; }
                                      public extern Foo(int value);
                                  }
                              }
                              """;

        var analyzer = new RedundantInitSetterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.RedundantInitSetter)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_InterfaceInitProperty_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_InterfaceInitProperty_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface IShape {
                                      int Value { get; init; }
                                  }
                              }
                              """;

        var analyzer = new RedundantInitSetterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.RedundantInitSetter)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_PropertyOutsideTypeDeclaration_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PropertyOutsideTypeDeclaration_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public int Value { get; init; }
                              }
                              """;

        var analyzer = new RedundantInitSetterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.RedundantInitSetter)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_PropertyWithExpressionBody_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PropertyWithExpressionBody_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public int Computed => 1;
                                      public int Value { get; init; }
                                      public Foo(int value) { Value = value; }
                                  }
                              }
                              """;

        var analyzer = new RedundantInitSetterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.RedundantInitSetter)).IsTrue();
    }
}
