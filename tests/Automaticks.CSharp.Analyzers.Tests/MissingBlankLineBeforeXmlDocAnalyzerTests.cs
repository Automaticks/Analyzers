using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class MissingBlankLineBeforeXmlDocAnalyzerTests
{
    [Test]
    public async Task Analyze_FieldWithXmlDocNoBlankLineAfterPreviousField_ReportsDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingBlankLineBeforeXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS054")).IsTrue();
    }

    [Test]
    public async Task Analyze_PropertyWithXmlDocNoBlankLineAfterPreviousMember_ReportsDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingBlankLineBeforeXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS054")).IsTrue();
    }

    [Test]
    public async Task Analyze_MethodWithXmlDocNoBlankLineAfterPreviousMember_ReportsDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingBlankLineBeforeXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS054")).IsTrue();
    }

    [Test]
    public async Task Analyze_ConstructorWithXmlDocNoBlankLineAfterPreviousMember_ReportsDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingBlankLineBeforeXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS054")).IsTrue();
    }

    [Test]
    public async Task Analyze_EventWithXmlDocNoBlankLineAfterPreviousMember_ReportsDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingBlankLineBeforeXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS054")).IsTrue();
    }

    [Test]
    public async Task Analyze_NestedClassWithXmlDocNoBlankLineAfterPreviousMember_ReportsDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingBlankLineBeforeXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS054")).IsTrue();
    }

    [Test]
    public async Task Analyze_TypeInNamespaceWithXmlDocNoBlankLineAfterPreviousType_ReportsDiagnostic()
    {
        const string source =
            "namespace MyApp {\n" +
            "    public class A { }\n" +
            "    /// <summary>\n" +
            "    ///     Type B.\n" +
            "    /// </summary>\n" +
            "    public class B { }\n" +
            "}";

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingBlankLineBeforeXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS054")).IsTrue();
    }

    [Test]
    public async Task Analyze_FirstMemberWithXmlDoc_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingBlankLineBeforeXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS054")).IsFalse();
    }

    [Test]
    public async Task Analyze_FieldWithXmlDocPrecededByBlankLine_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingBlankLineBeforeXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS054")).IsFalse();
    }

    [Test]
    public async Task Analyze_MemberWithoutXmlDoc_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Counter {
                                      private int _a;
                                      private int _b;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingBlankLineBeforeXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS054")).IsFalse();
    }

    [Test]
    public async Task Analyze_MethodWithXmlDocPrecededByBlankLine_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingBlankLineBeforeXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS054")).IsFalse();
    }

    [Test]
    public async Task Analyze_SingleMemberClass_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Single {
                                      public void DoWork() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingBlankLineBeforeXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS054")).IsFalse();
    }

    [Test]
    public async Task Analyze_TypeInNamespaceWithXmlDocPrecededByBlankLine_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingBlankLineBeforeXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS054")).IsFalse();
    }

    [Test]
    public async Task Analyze_ThreeMembersMiddleMissingBlankLine_ReportsOneDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingBlankLineBeforeXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Count(d => d.Id == "ATXCS054")).IsEqualTo(1);
    }

    [Test]
    public async Task Analyze_ThreeMembersBothMissingBlankLines_ReportsTwoDiagnostics()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MissingBlankLineBeforeXmlDocAnalyzer(), source);

        await Assert.That(diagnostics.Count(d => d.Id == "ATXCS054")).IsEqualTo(2);
    }
}
