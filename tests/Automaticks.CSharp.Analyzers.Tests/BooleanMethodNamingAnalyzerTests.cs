using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class BooleanMethodNamingAnalyzerTests
{
    // ── Basic pass/fail ───────────────────────────────────────────────────────

    [Test]
    public async Task Analyze_BoolMethodWithoutPrefix_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool Validate() => true;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMethodNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS063")).IsTrue();
    }

    [Test]
    public async Task Analyze_BoolMethodWithCanPrefix_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool CanExecute() => true;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMethodNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS063")).IsFalse();
    }

    [Test]
    public async Task Analyze_BoolMethodWithHasPrefix_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool HasPermission() => true;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMethodNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS063")).IsFalse();
    }

    [Test]
    public async Task Analyze_BoolMethodWithLowercaseCanPrefix_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool canExecute() => true;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMethodNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS063")).IsFalse();
    }

    [Test]
    public async Task Analyze_BoolMethodWithLowercaseHasPrefix_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool hasAccess() => true;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMethodNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS063")).IsFalse();
    }

    [Test]
    public async Task Analyze_NullableBoolMethodWithoutPrefix_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool? Check() => null;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMethodNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS063")).IsTrue();
    }

    [Test]
    public async Task Analyze_NullableBoolMethodWithCanPrefix_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public bool? CanCheck() => null;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMethodNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS063")).IsFalse();
    }

    [Test]
    public async Task Analyze_NonBoolMethod_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public string GetName() => "foo";
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMethodNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS063")).IsFalse();
    }

    // ── Access modifiers ──────────────────────────────────────────────────────

    [Test]
    public async Task Analyze_PrivateBoolMethodWithoutPrefix_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      private bool Validate() => true;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMethodNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS063")).IsTrue();
    }

    [Test]
    public async Task Analyze_ProtectedBoolMethodWithoutPrefix_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      protected bool Validate() => true;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMethodNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS063")).IsTrue();
    }

    // ── Local functions ───────────────────────────────────────────────────────

    [Test]
    public async Task Analyze_BoolLocalFunctionWithoutPrefix_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Execute() {
                                          bool Validate() => true;
                                          _ = Validate();
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMethodNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS063")).IsTrue();
    }

    [Test]
    public async Task Analyze_BoolLocalFunctionWithCanPrefix_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Execute() {
                                          bool CanValidate() => true;
                                          _ = CanValidate();
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMethodNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS063")).IsFalse();
    }

    [Test]
    public async Task Analyze_NullableBoolLocalFunctionWithoutPrefix_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public void Execute() {
                                          bool? Check() => null;
                                          _ = Check();
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMethodNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS063")).IsTrue();
    }

    // ── Overrides and interface implementations ───────────────────────────────

    [Test]
    public async Task Analyze_ExternalOverrideMethod_ReportsNoDiagnostic()
    {
        const string externalSource = """
                                      public abstract class Base {
                                          public abstract bool Validate();
                                      }
                                      """;

        const string source = """
                              namespace MyApp {
                                  public class Derived : Base {
                                      public override bool Validate() => true;
                                  }
                              }
                              """;

        var externalRef = AnalyzerTestRunner.CompileToReference(externalSource);
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMethodNamingAnalyzer(), source, [externalRef]);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS063")).IsFalse();
    }

    [Test]
    public async Task Analyze_ExplicitExternalInterfaceMethodImplementation_ReportsNoDiagnostic()
    {
        const string externalSource = """
                                      public interface IValidator {
                                          bool Validate();
                                      }
                                      """;

        const string source = """
                              namespace MyApp {
                                  public class Validator : IValidator {
                                      bool IValidator.Validate() => true;
                                  }
                              }
                              """;

        var externalRef = AnalyzerTestRunner.CompileToReference(externalSource);
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMethodNamingAnalyzer(), source, [externalRef]);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS063")).IsFalse();
    }

    [Test]
    public async Task Analyze_ImplicitExternalInterfaceMethodImplementation_ReportsNoDiagnostic()
    {
        const string externalSource = """
                                      public interface IValidator {
                                          bool Validate();
                                      }
                                      """;

        const string source = """
                              namespace MyApp {
                                  public class Validator : IValidator {
                                      public bool Validate() => true;
                                  }
                              }
                              """;

        var externalRef = AnalyzerTestRunner.CompileToReference(externalSource);
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMethodNamingAnalyzer(), source, [externalRef]);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS063")).IsFalse();
    }

    [Test]
    public async Task Analyze_LocalInterfaceMethodImplementation_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public interface IValidator {
                                      bool Validate();
                                  }
                                  public class Validator : IValidator {
                                      public bool Validate() => true;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new BooleanMethodNamingAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS063")).IsTrue();
    }
}
