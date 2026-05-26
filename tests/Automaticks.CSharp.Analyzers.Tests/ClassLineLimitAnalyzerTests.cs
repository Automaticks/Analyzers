
using Automaticks.CSharp;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

/// <summary>
///     Tests for ClassLineLimitAnalyzer.
/// </summary>
public class ClassLineLimitAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_BlankLinesExcluded_StaysUnderLimit.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_BlankLinesExcluded_StaysUnderLimit(CancellationToken cancellationToken)
    {
        var source = BuildClassWithCodeAndBlankLines(250, 1000);

        var analyzer = new ClassLineLimitAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.ClassLineLimit)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ClassAtExactLimit_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ClassAtExactLimit_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        var source = BuildClassWithCodeLines(497);

        var analyzer = new ClassLineLimitAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.ClassLineLimit)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ClassBelowLimit_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ClassBelowLimit_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        var source = BuildClassWithCodeLines(496);

        var analyzer = new ClassLineLimitAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.ClassLineLimit)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ClassExceedingLimit_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ClassExceedingLimit_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        var source = BuildClassWithCodeLines(498);

        var analyzer = new ClassLineLimitAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.ClassLineLimit)).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_DiagnosticMessage_ContainsLineCount.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_DiagnosticMessage_ContainsLineCount(CancellationToken cancellationToken)
    {
        var source = BuildClassWithCodeLines(498);

        var analyzer = new ClassLineLimitAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        var diagnostic = DiagnosticCollectionAssertions.FindById(diagnostics, DiagnosticIds.CSharp.ClassLineLimit);
        await Assert.That(diagnostic).IsNotNull();
        await Assert.That(diagnostic!.GetMessage(CultureInfo.InvariantCulture)).Contains("501");
    }

    /// <summary>
    ///     Tests that Analyze_ExtensibleMarkupLanguageDocCommentsExcluded_StaysUnderLimit.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExtensibleMarkupLanguageDocCommentsExcluded_StaysUnderLimit(CancellationToken cancellationToken)
    {
        var source = BuildClassWithCodeAndExtensibleMarkupLanguageDocComments(250, 1000);

        var analyzer = new ClassLineLimitAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.ClassLineLimit)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_GeneratedCodeFile_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_GeneratedCodeFile_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        var source = BuildGeneratedClassWithCodeLines(498);

        var analyzer = new ClassLineLimitAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.ClassLineLimit)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_MultiLineCommentsExcluded_StaysUnderLimit.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MultiLineCommentsExcluded_StaysUnderLimit(CancellationToken cancellationToken)
    {
        var source = BuildClassWithCodeAndBlockComment(250, 1000);

        var analyzer = new ClassLineLimitAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.ClassLineLimit)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_NestedTypeIncluded_InCount.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NestedTypeIncluded_InCount(CancellationToken cancellationToken)
    {
        var source = BuildOuterClassWithNestedClass(495);

        var analyzer = new ClassLineLimitAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.ClassLineLimit)).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_PartialClassBelowLimitAggregated_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PartialClassBelowLimitAggregated_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        var firstSourceFile = new SourceFile
        {
            Source = BuildClassWithCodeLines(245, true),
            FilePath = "FooA.cs"
        };
        var secondSourceFile = new SourceFile
        {
            Source = BuildClassWithCodeLines(245, true),
            FilePath = "FooB.cs"
        };
        IReadOnlyList<SourceFile> sourceFiles = [firstSourceFile, secondSourceFile];

        var analyzer = new ClassLineLimitAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, sourceFiles, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.ClassLineLimit)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_PartialClassExceedingLimitAggregated_ReportsSingleDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PartialClassExceedingLimitAggregated_ReportsSingleDiagnostic(CancellationToken cancellationToken)
    {
        var firstSourceFile = new SourceFile
        {
            Source = BuildClassWithCodeLines(249, true),
            FilePath = "FooA.cs"
        };
        var secondSourceFile = new SourceFile
        {
            Source = BuildClassWithCodeLines(249, true),
            FilePath = "FooB.cs"
        };
        IReadOnlyList<SourceFile> sourceFiles = [firstSourceFile, secondSourceFile];

        var analyzer = new ClassLineLimitAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, sourceFiles, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.CountId(diagnostics, DiagnosticIds.CSharp.ClassLineLimit)).IsEqualTo(1);
    }

    /// <summary>
    ///     Tests that Analyze_RegionDirectivesCounted_ExceedsLimit.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_RegionDirectivesCounted_ExceedsLimit(CancellationToken cancellationToken)
    {
        var source = BuildClassWithRegions(248, 125);

        var analyzer = new ClassLineLimitAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.ClassLineLimit)).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_SingleLineCommentsExcluded_StaysUnderLimit.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SingleLineCommentsExcluded_StaysUnderLimit(CancellationToken cancellationToken)
    {
        var source = BuildClassWithCodeAndSingleLineComments(250, 1000);

        var analyzer = new ClassLineLimitAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.ClassLineLimit)).IsFalse();
    }

    private string BuildClassWithCodeAndBlankLines(int codeLines, int blankLines)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("namespace MyApp");
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("    public class Foo");
        stringBuilder.AppendLine("    {");
        for (var lineIndex = 0; lineIndex < codeLines; lineIndex++)
        {
            stringBuilder.Append("        var x").Append(lineIndex).AppendLine(" = 0;");
        }

        for (var lineIndex = 0; lineIndex < blankLines; lineIndex++)
        {
            stringBuilder.AppendLine();
        }

        stringBuilder.AppendLine("    }");
        stringBuilder.AppendLine("}");
        return stringBuilder.ToString();
    }

    private string BuildClassWithCodeAndBlockComment(int codeLines, int blockCommentLines)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("namespace MyApp");
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("    public class Foo");
        stringBuilder.AppendLine("    {");
        for (var lineIndex = 0; lineIndex < codeLines; lineIndex++)
        {
            stringBuilder.Append("        var x").Append(lineIndex).AppendLine(" = 0;");
        }

        stringBuilder.AppendLine("        ");
        stringBuilder.AppendLine("    }");
        stringBuilder.AppendLine("}");
        return stringBuilder.ToString();
    }

    private string BuildClassWithCodeAndExtensibleMarkupLanguageDocComments(int codeLines, int commentLines)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("namespace MyApp");
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("    public class Foo");
        stringBuilder.AppendLine("    {");
        for (var lineIndex = 0; lineIndex < codeLines; lineIndex++)
        {
            stringBuilder.Append("        var x").Append(lineIndex).AppendLine(" = 0;");
        }

        for (var lineIndex = 0; lineIndex < commentLines; lineIndex++)
        {
            stringBuilder.Append("        /// xml doc comment line ").Append(lineIndex).AppendLine();
        }

        stringBuilder.AppendLine("    }");
        stringBuilder.AppendLine("}");
        return stringBuilder.ToString();
    }

    private string BuildClassWithCodeAndSingleLineComments(int codeLines, int commentLines)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("namespace MyApp");
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("    public class Foo");
        stringBuilder.AppendLine("    {");
        for (var lineIndex = 0; lineIndex < codeLines; lineIndex++)
        {
            stringBuilder.Append("        var x").Append(lineIndex).AppendLine(" = 0;");
        }

        for (var lineIndex = 0; lineIndex < commentLines; lineIndex++)
        {
            stringBuilder.Append("        // comment line ").Append(lineIndex).AppendLine();
        }

        stringBuilder.AppendLine("    }");
        stringBuilder.AppendLine("}");
        return stringBuilder.ToString();
    }

    private string BuildClassWithCodeLines(int bodyLineCount)
    {
        return BuildClassWithCodeLines(bodyLineCount, false);
    }

    private string BuildClassWithCodeLines(int bodyLineCount, bool isPartial)
    {
        var partial = isPartial ? "partial " : string.Empty;
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("namespace MyApp");
        stringBuilder.AppendLine("{");
        stringBuilder.Append("    public ").Append(partial).AppendLine("class Foo");
        stringBuilder.AppendLine("    {");
        for (var lineIndex = 0; lineIndex < bodyLineCount; lineIndex++)
        {
            stringBuilder.Append("        var x").Append(lineIndex).AppendLine(" = 0;");
        }

        stringBuilder.AppendLine("    }");
        stringBuilder.AppendLine("}");
        return stringBuilder.ToString();
    }

    private string BuildClassWithRegions(int codeLines, int regionCount)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("namespace MyApp");
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("    public class Foo");
        stringBuilder.AppendLine("    {");
        for (var lineIndex = 0; lineIndex < regionCount; lineIndex++)
        {
            stringBuilder.Append("        #region Region").Append(lineIndex).AppendLine();
        }

        for (var lineIndex = 0; lineIndex < codeLines; lineIndex++)
        {
            stringBuilder.Append("        var x").Append(lineIndex).AppendLine(" = 0;");
        }

        for (var lineIndex = 0; lineIndex < regionCount; lineIndex++)
        {
            stringBuilder.AppendLine("        #endregion");
        }

        stringBuilder.AppendLine("    }");
        stringBuilder.AppendLine("}");
        return stringBuilder.ToString();
    }

    private string BuildGeneratedClassWithCodeLines(int bodyLineCount)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("// <auto-generated>");
        stringBuilder.AppendLine("namespace MyApp");
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("    public class Foo");
        stringBuilder.AppendLine("    {");
        for (var lineIndex = 0; lineIndex < bodyLineCount; lineIndex++)
        {
            stringBuilder.Append("        var x").Append(lineIndex).AppendLine(" = 0;");
        }

        stringBuilder.AppendLine("    }");
        stringBuilder.AppendLine("}");
        return stringBuilder.ToString();
    }

    private string BuildOuterClassWithNestedClass(int innerBodyLines)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("namespace MyApp");
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("    public class Outer");
        stringBuilder.AppendLine("    {");
        stringBuilder.AppendLine("        public class Inner");
        stringBuilder.AppendLine("        {");
        for (var lineIndex = 0; lineIndex < innerBodyLines; lineIndex++)
        {
            stringBuilder.Append("            var x").Append(lineIndex).AppendLine(" = 0;");
        }

        stringBuilder.AppendLine("        }");
        stringBuilder.AppendLine("    }");
        stringBuilder.AppendLine("}");
        return stringBuilder.ToString();
    }
}
