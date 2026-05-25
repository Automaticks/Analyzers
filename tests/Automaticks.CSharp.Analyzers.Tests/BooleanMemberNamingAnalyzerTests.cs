using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class BooleanMemberNamingAnalyzerTests
{
    // ── Fields ────────────────────────────────────────────────────────────────

    [Test]
    public async Task Analyze_BoolFieldWithoutPrefix_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private bool enabled;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMemberNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS062")).IsTrue();
    }

    [Test]
    public async Task Analyze_BoolFieldWithIsPrefix_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private bool isEnabled;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMemberNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS062")).IsFalse();
    }

    [Test]
    public async Task Analyze_BoolFieldWithIsPrefixUpperCase_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private bool IsEnabled;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMemberNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS062")).IsFalse();
    }

    [Test]
    public async Task Analyze_BoolFieldWithAllowPrefix_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private bool allowRetry;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMemberNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS062")).IsFalse();
    }

    [Test]
    public async Task Analyze_BoolFieldWithAllowPrefixUpperCase_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private bool AllowRetry;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMemberNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS062")).IsFalse();
    }

    [Test]
    public async Task Analyze_NullableBoolFieldWithoutPrefix_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private bool? enabled;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMemberNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS062")).IsTrue();
    }

    [Test]
    public async Task Analyze_NullableBoolFieldWithIsPrefix_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private bool? isEnabled;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMemberNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS062")).IsFalse();
    }

    [Test]
    public async Task Analyze_ConstBoolFieldWithoutPrefix_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public const bool DefaultEnabled = true;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMemberNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS062")).IsTrue();
    }

    [Test]
    public async Task Analyze_ConstBoolFieldWithIsPrefix_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public const bool IsDefaultEnabled = true;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMemberNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS062")).IsFalse();
    }

    [Test]
    public async Task Analyze_StaticReadonlyBoolFieldWithoutPrefix_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public static readonly bool DefaultEnabled = true;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMemberNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS062")).IsTrue();
    }

    [Test]
    public async Task Analyze_StaticReadonlyBoolFieldWithAllowPrefix_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public static readonly bool AllowOverride = false;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMemberNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS062")).IsFalse();
    }

    [Test]
    public async Task Analyze_NonBoolField_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private int count;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMemberNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS062")).IsFalse();
    }

    // ── Properties ────────────────────────────────────────────────────────────

    [Test]
    public async Task Analyze_BoolAutoPropertyWithoutPrefix_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool Enabled { get; set; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMemberNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS062")).IsTrue();
    }

    [Test]
    public async Task Analyze_BoolAutoPropertyWithIsPrefix_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool IsEnabled { get; set; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMemberNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS062")).IsFalse();
    }

    [Test]
    public async Task Analyze_BoolFullPropertyWithoutPrefix_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private bool _enabled;
                                      public bool Enabled { get => _enabled; set => _enabled = value; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMemberNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS062")).IsTrue();
    }

    [Test]
    public async Task Analyze_NullableBoolPropertyWithoutPrefix_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool? Enabled { get; set; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMemberNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS062")).IsTrue();
    }

    [Test]
    public async Task Analyze_NonBoolProperty_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public string Name { get; set; } = string.Empty;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMemberNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS062")).IsFalse();
    }

    // ── Overrides and interface implementations ───────────────────────────────

    [Test]
    public async Task Analyze_ExternalOverrideProperty_ReportsNoDiagnostic()
    {
        const string externalSource = """
                                      public abstract class Base {
                                          public abstract bool Enabled { get; }
                                      }
                                      """;

        const string source = """
                              namespace MyApp {
                                  public class Derived : Base {
                                      public override bool Enabled { get; } = true;
                                  }
                              }
                              """;

        var externalRef = AnalyzerTestRunner.CompileToReference(externalSource);
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMemberNamingAnalyzer(), source, [externalRef]);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS062")).IsFalse();
    }

    [Test]
    public async Task Analyze_ExplicitExternalInterfacePropertyImplementation_ReportsNoDiagnostic()
    {
        const string externalSource = """
                                      public interface IFoo {
                                          bool Enabled { get; }
                                      }
                                      """;

        const string source = """
                              namespace MyApp {
                                  public class Foo : IFoo {
                                      bool IFoo.Enabled { get; } = true;
                                  }
                              }
                              """;

        var externalRef = AnalyzerTestRunner.CompileToReference(externalSource);
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMemberNamingAnalyzer(), source, [externalRef]);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS062")).IsFalse();
    }

    [Test]
    public async Task Analyze_ImplicitExternalInterfacePropertyImplementation_ReportsNoDiagnostic()
    {
        const string externalSource = """
                                      public interface IFoo {
                                          bool Enabled { get; }
                                      }
                                      """;

        const string source = """
                              namespace MyApp {
                                  public class Foo : IFoo {
                                      public bool Enabled { get; } = true;
                                  }
                              }
                              """;

        var externalRef = AnalyzerTestRunner.CompileToReference(externalSource);
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMemberNamingAnalyzer(), source, [externalRef]);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS062")).IsFalse();
    }

    [Test]
    public async Task Analyze_BoolFieldWithUnderscoreIsPrefix_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private bool _isEnabled;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMemberNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS062")).IsFalse();
    }

    [Test]
    public async Task Analyze_BoolFieldWithUnderscoreAllowPrefix_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private bool _allowRetry;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMemberNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS062")).IsFalse();
    }

    [Test]
    public async Task Analyze_BoolFieldWithUnderscoreOnlyPrefix_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private bool _enabled;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMemberNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS062")).IsTrue();
    }

    [Test]
    public async Task Analyze_LocalInterfacePropertyImplementation_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo {
                                      bool Enabled { get; }
                                  }
                                  public class Foo : IFoo {
                                      public bool Enabled { get; } = true;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMemberNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS062")).IsTrue();
    }
}
