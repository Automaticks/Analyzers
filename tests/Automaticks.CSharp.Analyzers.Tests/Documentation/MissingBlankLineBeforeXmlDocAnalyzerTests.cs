using Automaticks.CSharp;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Documentation;

/// <summary>
///     Tests for MissingBlankLineBeforeXmlDocAnalyzer.
/// </summary>
public class MissingBlankLineBeforeXmlDocAnalyzerTests
{

    /// <summary>
    ///     Tests that Analyze_ConstructorWithExtensibleMarkupLanguageDocNoBlankLineAfterPreviousMember_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConstructorWithExtensibleMarkupLanguageDocNoBlankLineAfterPreviousMember_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Widget {
                                      private readonly int _value;
                                      /// <summary>
                                      ///     Initializes a new instance of Widget.
                                      /// </summary>
                                      /// <param name="value">The value.</param>
                                      public Widget(int value) { _value = value; }
                                  }
                              }
                              """;

        var analyzer = new MissingBlankLineBeforeXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS054")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_EventWithExtensibleMarkupLanguageDocNoBlankLineAfterPreviousMember_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_EventWithExtensibleMarkupLanguageDocNoBlankLineAfterPreviousMember_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public class Notifier {
                                      private int _count;
                                      /// <summary>
                                      ///     Raised when something happens.
                                      /// </summary>
                                      public event EventHandler? Changed;
                                  }
                              }
                              """;

        var analyzer = new MissingBlankLineBeforeXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS054")).IsTrue();
    }
    /// <summary>
    ///     Tests that Analyze_FieldWithExtensibleMarkupLanguageDocNoBlankLineAfterPreviousField_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FieldWithExtensibleMarkupLanguageDocNoBlankLineAfterPreviousField_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Counter {
                                      private int _a;
                                      /// <summary>
                                      ///     Field b.
                                      /// </summary>
                                      private int _b;
                                  }
                              }
                              """;

        var analyzer = new MissingBlankLineBeforeXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS054")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_FieldWithExtensibleMarkupLanguageDocPrecededByBlankLine_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FieldWithExtensibleMarkupLanguageDocPrecededByBlankLine_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Counter {
                                      private int _a;

                                      /// <summary>
                                      ///     Field b.
                                      /// </summary>
                                      private int _b;
                                  }
                              }
                              """;

        var analyzer = new MissingBlankLineBeforeXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS054")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_FirstMemberWithExtensibleMarkupLanguageDoc_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FirstMemberWithExtensibleMarkupLanguageDoc_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Counter {
                                      /// <summary>
                                      ///     The count.
                                      /// </summary>
                                      private int _count;
                                  }
                              }
                              """;

        var analyzer = new MissingBlankLineBeforeXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS054")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_MemberWithoutExtensibleMarkupLanguageDoc_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MemberWithoutExtensibleMarkupLanguageDoc_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Counter {
                                      private int _a;
                                      private int _b;
                                  }
                              }
                              """;

        var analyzer = new MissingBlankLineBeforeXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS054")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_MethodWithExtensibleMarkupLanguageDocNoBlankLineAfterPreviousMember_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodWithExtensibleMarkupLanguageDocNoBlankLineAfterPreviousMember_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Service {
                                      private int _a;
                                      /// <summary>
                                      ///     Does something.
                                      /// </summary>
                                      public void Execute() { }
                                  }
                              }
                              """;

        var analyzer = new MissingBlankLineBeforeXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS054")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_MethodWithExtensibleMarkupLanguageDocPrecededByBlankLine_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodWithExtensibleMarkupLanguageDocPrecededByBlankLine_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Service {
                                      private int _a;

                                      /// <summary>
                                      ///     Does something.
                                      /// </summary>
                                      public void Execute() { }
                                  }
                              }
                              """;

        var analyzer = new MissingBlankLineBeforeXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS054")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_NestedClassWithExtensibleMarkupLanguageDocNoBlankLineAfterPreviousMember_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NestedClassWithExtensibleMarkupLanguageDocNoBlankLineAfterPreviousMember_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Outer {
                                      private int _count;
                                      /// <summary>
                                      ///     A nested class.
                                      /// </summary>
                                      public class Inner { }
                                  }
                              }
                              """;

        var analyzer = new MissingBlankLineBeforeXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS054")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_PropertyWithExtensibleMarkupLanguageDocNoBlankLineAfterPreviousMember_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PropertyWithExtensibleMarkupLanguageDocNoBlankLineAfterPreviousMember_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Config {
                                      private int _a;
                                      /// <summary>
                                      ///     Gets or sets a value.
                                      /// </summary>
                                      public int Value { get; set; }
                                  }
                              }
                              """;

        var analyzer = new MissingBlankLineBeforeXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS054")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_SingleMemberClass_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SingleMemberClass_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Single {
                                      public void DoWork() { }
                                  }
                              }
                              """;

        var analyzer = new MissingBlankLineBeforeXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS054")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ThreeMembersBothMissingBlankLines_ReportsTwoDiagnostics.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ThreeMembersBothMissingBlankLines_ReportsTwoDiagnostics(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Counter {
                                      private int _a;
                                      /// <summary>
                                      ///     Field b.
                                      /// </summary>
                                      private int _b;
                                      /// <summary>
                                      ///     Field c.
                                      /// </summary>
                                      private int _c;
                                  }
                              }
                              """;

        var analyzer = new MissingBlankLineBeforeXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.CountId(diagnostics, "ATXCS054")).IsEqualTo(2);
    }

    /// <summary>
    ///     Tests that Analyze_ThreeMembersMiddleMissingBlankLine_ReportsOneDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ThreeMembersMiddleMissingBlankLine_ReportsOneDiagnostic(CancellationToken cancellationToken)
    {
        const string source =
            "namespace MyApp {\n" +
            "    public class Counter {\n" +
            "        private int _a;\n" +
            "        /// <summary>\n" +
            "        ///     Field b.\n" +
            "        /// </summary>\n" +
            "        private int _b;\n" +
            "\n" +
            "        /// <summary>\n" +
            "        ///     Field c.\n" +
            "        /// </summary>\n" +
            "        private int _c;\n" +
            "    }\n" +
            "}";

        var analyzer = new MissingBlankLineBeforeXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.CountId(diagnostics, "ATXCS054")).IsEqualTo(1);
    }

    /// <summary>
    ///     Tests that Analyze_TypeInNamespaceWithExtensibleMarkupLanguageDocNoBlankLineAfterPreviousType_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TypeInNamespaceWithExtensibleMarkupLanguageDocNoBlankLineAfterPreviousType_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source =
            "namespace MyApp {\n" +
            "    public class A { }\n" +
            "    /// <summary>\n" +
            "    ///     Type B.\n" +
            "    /// </summary>\n" +
            "    public class B { }\n" +
            "}";

        var analyzer = new MissingBlankLineBeforeXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS054")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_TypeInNamespaceWithExtensibleMarkupLanguageDocPrecededByBlankLine_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TypeInNamespaceWithExtensibleMarkupLanguageDocPrecededByBlankLine_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source =
            "namespace MyApp {\n" +
            "    public class A { }\n" +
            "\n" +
            "    /// <summary>\n" +
            "    ///     Type B.\n" +
            "    /// </summary>\n" +
            "    public class B { }\n" +
            "}";

        var analyzer = new MissingBlankLineBeforeXmlDocAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS054")).IsFalse();
    }
}
