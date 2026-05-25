
using Automaticks.CSharp;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class MethodLineLimitAnalyzerTests
{
    [Test]
    public async Task Analyze_AbstractMethod_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp
                              {
                                  public abstract class Foo
                                  {
                                      public abstract void Bar();
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MethodLineLimitAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.MethodLineLimit)).IsFalse();
    }

    [Test]
    public async Task Analyze_ExpressionBodiedMethod_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp
                              {
                                  public class Foo
                                  {
                                      public int Bar() => 42;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MethodLineLimitAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.MethodLineLimit)).IsFalse();
    }

    [Test]
    public async Task Analyze_LocalFunctionExceedingLimit_ReportsDiagnostic()
    {
        var source = BuildClassWithLocalFunction(48);

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MethodLineLimitAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.MethodLineLimit && d.GetMessage(CultureInfo.InvariantCulture).Contains("'Inner'"))).IsTrue();
    }

    [Test]
    public async Task Analyze_MethodAtExactLimit_ReportsNoDiagnostic()
    {
        var source = BuildClassWithMethod(47);

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MethodLineLimitAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.MethodLineLimit)).IsFalse();
    }

    [Test]
    public async Task Analyze_MethodExceedingLimit_ReportsDiagnostic()
    {
        var source = BuildClassWithMethod(48);

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MethodLineLimitAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.MethodLineLimit)).IsTrue();
    }

    [Test]
    public async Task Analyze_OperatorExceedingLimit_ReportsDiagnostic()
    {
        var source = BuildClassWithOperator(48);

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MethodLineLimitAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.MethodLineLimit)).IsTrue();
    }

    [Test]
    public async Task Analyze_PropertyGetAccessorExceedingLimit_ReportsDiagnostic()
    {
        var source = BuildClassWithPropertyGetter(48);

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new MethodLineLimitAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == DiagnosticIds.CSharp.MethodLineLimit)).IsTrue();
    }

    private static string BuildClassWithLocalFunction(int bodyLineCount)
    {
        var sb = new StringBuilder();
        sb.AppendLine("namespace MyApp");
        sb.AppendLine("{");
        sb.AppendLine("    public class Foo");
        sb.AppendLine("    {");
        sb.AppendLine("        public void Outer()");
        sb.AppendLine("        {");
        sb.AppendLine("            void Inner()");
        sb.AppendLine("            {");
        for (var i = 0; i < bodyLineCount; i++)
        {
            sb.Append("                // line ").Append(i + 1).AppendLine();
        }

        sb.AppendLine("            }");
        sb.AppendLine("            Inner();");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string BuildClassWithMethod(int bodyLineCount)
    {
        var sb = new StringBuilder();
        sb.AppendLine("namespace MyApp");
        sb.AppendLine("{");
        sb.AppendLine("    public class Foo");
        sb.AppendLine("    {");
        sb.AppendLine("        public void Bar()");
        sb.AppendLine("        {");
        for (var i = 0; i < bodyLineCount; i++)
        {
            sb.Append("            // line ").Append(i + 1).AppendLine();
        }

        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string BuildClassWithOperator(int bodyLineCount)
    {
        var sb = new StringBuilder();
        sb.AppendLine("namespace MyApp");
        sb.AppendLine("{");
        sb.AppendLine("    public class Foo");
        sb.AppendLine("    {");
        sb.AppendLine("        public static Foo operator +(Foo left, Foo right)");
        sb.AppendLine("        {");
        for (var i = 0; i < bodyLineCount - 1; i++)
        {
            sb.Append("            // line ").Append(i + 1).AppendLine();
        }

        sb.AppendLine("            return left;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string BuildClassWithPropertyGetter(int bodyLineCount)
    {
        var sb = new StringBuilder();
        sb.AppendLine("namespace MyApp");
        sb.AppendLine("{");
        sb.AppendLine("    public class Foo");
        sb.AppendLine("    {");
        sb.AppendLine("        public int Value");
        sb.AppendLine("        {");
        sb.AppendLine("            get");
        sb.AppendLine("            {");
        for (var i = 0; i < bodyLineCount - 1; i++)
        {
            sb.Append("                // line ").Append(i + 1).AppendLine();
        }

        sb.AppendLine("                return 0;");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }
}
