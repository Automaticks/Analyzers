
using Automaticks.CSharp;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

/// <summary>
///     Tests for MethodLineLimitAnalyzer.
/// </summary>
public class MethodLineLimitAnalyzerTests
{
    /// <summary>
    ///     Tests that Analyze_AbstractMethod_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AbstractMethod_ReportsNoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new MethodLineLimitAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.MethodLineLimit)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExpressionBodiedMethod_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExpressionBodiedMethod_ReportsNoDiagnostic(CancellationToken cancellationToken)
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

        var analyzer = new MethodLineLimitAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.MethodLineLimit)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_LocalFunctionExceedingLimit_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_LocalFunctionExceedingLimit_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        var source = BuildClassWithLocalFunction(48);

        var analyzer = new MethodLineLimitAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasIdWithMessageSubstring(diagnostics, DiagnosticIds.CSharp.MethodLineLimit, "'Inner'")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_MethodAtExactLimit_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodAtExactLimit_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        var source = BuildClassWithMethod(47);

        var analyzer = new MethodLineLimitAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.MethodLineLimit)).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_MethodExceedingLimit_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MethodExceedingLimit_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        var source = BuildClassWithMethod(48);

        var analyzer = new MethodLineLimitAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.MethodLineLimit)).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_OperatorExceedingLimit_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_OperatorExceedingLimit_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        var source = BuildClassWithOperator(48);

        var analyzer = new MethodLineLimitAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.MethodLineLimit)).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_PropertyGetAccessorExceedingLimit_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PropertyGetAccessorExceedingLimit_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        var source = BuildClassWithPropertyGetter(48);

        var analyzer = new MethodLineLimitAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, DiagnosticIds.CSharp.MethodLineLimit)).IsTrue();
    }

    private string BuildClassWithLocalFunction(int bodyLineCount)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("namespace MyApp");
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("    public class Foo");
        stringBuilder.AppendLine("    {");
        stringBuilder.AppendLine("        public void Outer()");
        stringBuilder.AppendLine("        {");
        stringBuilder.AppendLine("            void Inner()");
        stringBuilder.AppendLine("            {");
        for (var lineIndex = 0; lineIndex < bodyLineCount; lineIndex++)
        {
            stringBuilder.Append("                // line ").Append(lineIndex + 1).AppendLine();
        }

        stringBuilder.AppendLine("            }");
        stringBuilder.AppendLine("            Inner();");
        stringBuilder.AppendLine("        }");
        stringBuilder.AppendLine("    }");
        stringBuilder.AppendLine("}");
        return stringBuilder.ToString();
    }

    private string BuildClassWithMethod(int bodyLineCount)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("namespace MyApp");
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("    public class Foo");
        stringBuilder.AppendLine("    {");
        stringBuilder.AppendLine("        public void Bar()");
        stringBuilder.AppendLine("        {");
        for (var lineIndex = 0; lineIndex < bodyLineCount; lineIndex++)
        {
            stringBuilder.Append("            // line ").Append(lineIndex + 1).AppendLine();
        }

        stringBuilder.AppendLine("        }");
        stringBuilder.AppendLine("    }");
        stringBuilder.AppendLine("}");
        return stringBuilder.ToString();
    }

    private string BuildClassWithOperator(int bodyLineCount)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("namespace MyApp");
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("    public class Foo");
        stringBuilder.AppendLine("    {");
        stringBuilder.AppendLine("        public static Foo operator +(Foo left, Foo right)");
        stringBuilder.AppendLine("        {");
        for (var lineIndex = 0; lineIndex < bodyLineCount - 1; lineIndex++)
        {
            stringBuilder.Append("            // line ").Append(lineIndex + 1).AppendLine();
        }

        stringBuilder.AppendLine("            return left;");
        stringBuilder.AppendLine("        }");
        stringBuilder.AppendLine("    }");
        stringBuilder.AppendLine("}");
        return stringBuilder.ToString();
    }

    private string BuildClassWithPropertyGetter(int bodyLineCount)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("namespace MyApp");
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("    public class Foo");
        stringBuilder.AppendLine("    {");
        stringBuilder.AppendLine("        public int Value");
        stringBuilder.AppendLine("        {");
        stringBuilder.AppendLine("            get");
        stringBuilder.AppendLine("            {");
        for (var lineIndex = 0; lineIndex < bodyLineCount - 1; lineIndex++)
        {
            stringBuilder.Append("                // line ").Append(lineIndex + 1).AppendLine();
        }

        stringBuilder.AppendLine("                return 0;");
        stringBuilder.AppendLine("            }");
        stringBuilder.AppendLine("        }");
        stringBuilder.AppendLine("    }");
        stringBuilder.AppendLine("}");
        return stringBuilder.ToString();
    }
}
