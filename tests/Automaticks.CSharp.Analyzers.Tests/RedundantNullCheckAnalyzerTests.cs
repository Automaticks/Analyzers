using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class RedundantNullCheckAnalyzerTests
{
    [Test]
    public async Task Analyze_CoalesceThrowOnNonNullableParam_ReportsDiagnostic()
    {
        const string source = """
                              #nullable enable
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      private readonly string _x;
                                      public Foo(string x) {
                                          _x = x ?? throw new ArgumentNullException(nameof(x));
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new RedundantNullCheckAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS014")).IsTrue();
    }

    [Test]
    public async Task Analyze_CoalesceThrowOnNullableParam_ReportsNoDiagnostic()
    {
        const string source = """
                              #nullable enable
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      private readonly string _x;
                                      public Foo(string? x) {
                                          _x = x ?? throw new ArgumentNullException(nameof(x));
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new RedundantNullCheckAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS014")).IsFalse();
    }

    [Test]
    public async Task Analyze_CoalesceThrowOnValueTypeParam_ReportsNoDiagnostic()
    {
        const string source = """
                              #nullable enable
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      private readonly int _x;
                                      public Foo(int x) {
                                          _x = x ?? throw new ArgumentNullException(nameof(x));
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new RedundantNullCheckAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS014")).IsFalse();
    }

    [Test]
    public async Task Analyze_CoalesceWithDifferentException_ReportsNoDiagnostic()
    {
        const string source = """
                              #nullable enable
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      private readonly string _x;
                                      public Foo(string x) {
                                          _x = x ?? throw new InvalidOperationException();
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new RedundantNullCheckAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS014")).IsFalse();
    }

    [Test]
    public async Task Analyze_IfEqualsNullThrowOnNonNullableParam_ReportsDiagnostic()
    {
        const string source = """
                              #nullable enable
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public Foo(string x) {
                                          if (x == null) throw new ArgumentNullException(nameof(x));
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new RedundantNullCheckAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS014")).IsTrue();
    }

    [Test]
    public async Task Analyze_IfIsNullThrowOnNonNullableParam_ReportsDiagnostic()
    {
        const string source = """
                              #nullable enable
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public Foo(string x) {
                                          if (x is null) throw new ArgumentNullException(nameof(x));
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new RedundantNullCheckAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS014")).IsTrue();
    }

    [Test]
    public async Task Analyze_ThrowIfNullOnNonNullableParam_ReportsDiagnostic()
    {
        const string source = """
                              #nullable enable
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public Foo(string x) {
                                          ArgumentNullException.ThrowIfNull(x);
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new RedundantNullCheckAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS014")).IsTrue();
    }

    [Test]
    public async Task Analyze_ThrowIfNullOnNullableParam_ReportsNoDiagnostic()
    {
        const string source = """
                              #nullable enable
                              using System;
                              namespace MyApp {
                                  public class Foo {
                                      public Foo(string? x) {
                                          ArgumentNullException.ThrowIfNull(x);
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new RedundantNullCheckAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS014")).IsFalse();
    }
}
