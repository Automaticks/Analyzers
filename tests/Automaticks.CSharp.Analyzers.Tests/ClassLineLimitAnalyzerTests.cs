
using Automaticks.CSharp;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class ClassLineLimitAnalyzerTests
{
    [Test]
    public async Task Analyze_BlankLinesExcluded_StaysUnderLimit()
    {
        var source = BuildClassWithCodeAndBlankLines(250, 1000);

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ClassLineLimitAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.ClassLineLimit)).IsFalse();
    }

    [Test]
    public async Task Analyze_ClassAtExactLimit_ReportsNoDiagnostic()
    {
        var source = BuildClassWithCodeLines(497);

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ClassLineLimitAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.ClassLineLimit)).IsFalse();
    }

    [Test]
    public async Task Analyze_ClassBelowLimit_ReportsNoDiagnostic()
    {
        var source = BuildClassWithCodeLines(496);

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ClassLineLimitAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.ClassLineLimit)).IsFalse();
    }

    [Test]
    public async Task Analyze_ClassExceedingLimit_ReportsDiagnostic()
    {
        var source = BuildClassWithCodeLines(498);

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ClassLineLimitAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.ClassLineLimit)).IsTrue();
    }

    [Test]
    public async Task Analyze_DiagnosticMessage_ContainsLineCount()
    {
        var source = BuildClassWithCodeLines(498);

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ClassLineLimitAnalyzer(), source);

        var diagnostic = diagnostics.FirstOrDefault(d => d.Id == DiagnosticIds.CSharp.ClassLineLimit);
        await Assert.That(diagnostic).IsNotNull();
        await Assert.That(diagnostic!.GetMessage(CultureInfo.InvariantCulture)).Contains("501");
    }

    [Test]
    public async Task Analyze_GeneratedCodeFile_ReportsNoDiagnostic()
    {
        var source = BuildGeneratedClassWithCodeLines(498);

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ClassLineLimitAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.ClassLineLimit)).IsFalse();
    }

    [Test]
    public async Task Analyze_MultiLineCommentsExcluded_StaysUnderLimit()
    {
        var source = BuildClassWithCodeAndBlockComment(250, 1000);

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ClassLineLimitAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.ClassLineLimit)).IsFalse();
    }

    [Test]
    public async Task Analyze_NestedTypeIncluded_InCount()
    {
        var source = BuildOuterClassWithNestedClass(495);

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ClassLineLimitAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.ClassLineLimit)).IsTrue();
    }

    [Test]
    public async Task Analyze_PartialClassBelowLimitAggregated_ReportsNoDiagnostic()
    {
        IReadOnlyList<(string Source, string FilePath)> sourceFiles =
        [
            (BuildClassWithCodeLines(245, true), "FooA.cs"),
            (BuildClassWithCodeLines(245, true), "FooB.cs")
        ];

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ClassLineLimitAnalyzer(), sourceFiles);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.ClassLineLimit)).IsFalse();
    }

    [Test]
    public async Task Analyze_PartialClassExceedingLimitAggregated_ReportsSingleDiagnostic()
    {
        IReadOnlyList<(string Source, string FilePath)> sourceFiles =
        [
            (BuildClassWithCodeLines(249, true), "FooA.cs"),
            (BuildClassWithCodeLines(249, true), "FooB.cs")
        ];

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ClassLineLimitAnalyzer(), sourceFiles);

        await Assert.That(diagnostics.Count(d => d.Id == DiagnosticIds.CSharp.ClassLineLimit)).IsEqualTo(1);
    }

    [Test]
    public async Task Analyze_RegionDirectivesCounted_ExceedsLimit()
    {
        var source = BuildClassWithRegions(248, 125);

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ClassLineLimitAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.ClassLineLimit)).IsTrue();
    }

    [Test]
    public async Task Analyze_SingleLineCommentsExcluded_StaysUnderLimit()
    {
        var source = BuildClassWithCodeAndSingleLineComments(250, 1000);

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ClassLineLimitAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.ClassLineLimit)).IsFalse();
    }

    [Test]
    public async Task Analyze_XmlDocCommentsExcluded_StaysUnderLimit()
    {
        var source = BuildClassWithCodeAndXmlDocComments(250, 1000);

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new ClassLineLimitAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.ClassLineLimit)).IsFalse();
    }

    private static string BuildClassWithCodeAndBlankLines(int codeLines, int blankLines)
    {
        var sb = new StringBuilder();
        sb.AppendLine("namespace MyApp");
        sb.AppendLine("{");
        sb.AppendLine("    public class Foo");
        sb.AppendLine("    {");
        for (var i = 0; i < codeLines; i++)
        {
            sb.Append("        var x").Append(i).AppendLine(" = 0;");
        }

        for (var i = 0; i < blankLines; i++)
        {
            sb.AppendLine();
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string BuildClassWithCodeAndBlockComment(int codeLines, int blockCommentLines)
    {
        var sb = new StringBuilder();
        sb.AppendLine("namespace MyApp");
        sb.AppendLine("{");
        sb.AppendLine("    public class Foo");
        sb.AppendLine("    {");
        for (var i = 0; i < codeLines; i++)
        {
            sb.Append("        var x").Append(i).AppendLine(" = 0;");
        }

        sb.AppendLine("        /*");
        for (var i = 0; i < blockCommentLines; i++)
        {
            sb.Append("         * block comment line ").Append(i).AppendLine();
        }

        sb.AppendLine("         */");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string BuildClassWithCodeAndSingleLineComments(int codeLines, int commentLines)
    {
        var sb = new StringBuilder();
        sb.AppendLine("namespace MyApp");
        sb.AppendLine("{");
        sb.AppendLine("    public class Foo");
        sb.AppendLine("    {");
        for (var i = 0; i < codeLines; i++)
        {
            sb.Append("        var x").Append(i).AppendLine(" = 0;");
        }

        for (var i = 0; i < commentLines; i++)
        {
            sb.Append("        // comment line ").Append(i).AppendLine();
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string BuildClassWithCodeAndXmlDocComments(int codeLines, int commentLines)
    {
        var sb = new StringBuilder();
        sb.AppendLine("namespace MyApp");
        sb.AppendLine("{");
        sb.AppendLine("    public class Foo");
        sb.AppendLine("    {");
        for (var i = 0; i < codeLines; i++)
        {
            sb.Append("        var x").Append(i).AppendLine(" = 0;");
        }

        for (var i = 0; i < commentLines; i++)
        {
            sb.Append("        /// xml doc comment line ").Append(i).AppendLine();
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string BuildClassWithCodeLines(int bodyLineCount, bool isPartial = false)
    {
        var partial = isPartial ? "partial " : string.Empty;
        var sb = new StringBuilder();
        sb.AppendLine("namespace MyApp");
        sb.AppendLine("{");
        sb.Append("    public ").Append(partial).AppendLine("class Foo");
        sb.AppendLine("    {");
        for (var i = 0; i < bodyLineCount; i++)
        {
            sb.Append("        var x").Append(i).AppendLine(" = 0;");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string BuildClassWithRegions(int codeLines, int regionCount)
    {
        var sb = new StringBuilder();
        sb.AppendLine("namespace MyApp");
        sb.AppendLine("{");
        sb.AppendLine("    public class Foo");
        sb.AppendLine("    {");
        for (var i = 0; i < regionCount; i++)
        {
            sb.Append("        #region Region").Append(i).AppendLine();
        }

        for (var i = 0; i < codeLines; i++)
        {
            sb.Append("        var x").Append(i).AppendLine(" = 0;");
        }

        for (var i = 0; i < regionCount; i++)
        {
            sb.AppendLine("        #endregion");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string BuildGeneratedClassWithCodeLines(int bodyLineCount)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated>");
        sb.AppendLine("namespace MyApp");
        sb.AppendLine("{");
        sb.AppendLine("    public class Foo");
        sb.AppendLine("    {");
        for (var i = 0; i < bodyLineCount; i++)
        {
            sb.Append("        var x").Append(i).AppendLine(" = 0;");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string BuildOuterClassWithNestedClass(int innerBodyLines)
    {
        var sb = new StringBuilder();
        sb.AppendLine("namespace MyApp");
        sb.AppendLine("{");
        sb.AppendLine("    public class Outer");
        sb.AppendLine("    {");
        sb.AppendLine("        public class Inner");
        sb.AppendLine("        {");
        for (var i = 0; i < innerBodyLines; i++)
        {
            sb.Append("            var x").Append(i).AppendLine(" = 0;");
        }

        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }
}
