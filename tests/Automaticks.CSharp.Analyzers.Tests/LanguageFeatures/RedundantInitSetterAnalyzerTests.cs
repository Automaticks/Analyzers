using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.LanguageFeatures;

/// <summary>
///     Tests for <see cref="RedundantInitSetterAnalyzer"/> covering positive cases for the
///     redundant <c>init</c>-setter shape and negative cases for each documented exemption.
/// </summary>
public class RedundantInitSetterAnalyzerTests
{

    /// <summary>
    ///     Verifies that a property whose containing type's sole instance constructor does not
    ///     assign it is not reported, while a sibling property in the same ctor that IS assigned
    ///     from a parameter is reported.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CtorDoesNotAssignProperty_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public sealed class Foo {
                                      public string Name { get; init; }
                                      public int Count { get; init; }
                                      public Foo(int count) {
                                          Count = count;
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantInitSetterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.CountId(diagnostics, "ATXCS065")).IsEqualTo(1);
        var diagnostic = DiagnosticCollectionAssertions.GetSingleById(diagnostics, "ATXCS065");
        await Assert.That(diagnostic.GetMessage(CultureInfo.InvariantCulture).Contains("'Count'")).IsTrue();
    }

    /// <summary>
    ///     Verifies that an init property whose RHS is a collection literal is not reported;
    ///     the init setter provides legitimate caller-override capability via object initializer.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_DefaultSeedingCollectionLiteral_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public sealed class Foo {
                                      public int[] Items { get; init; }
                                      public Foo() {
                                          Items = [];
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantInitSetterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS065")).IsFalse();
    }

    /// <summary>
    ///     Verifies that a property whose RHS in a parameterised constructor does not reference
    ///     any ctor parameter is exempt (default seeding); a sibling property that IS assigned
    ///     from a parameter in the same ctor is still reported.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_DefaultSeedingInCtorWithParam_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public sealed class Foo {
                                      public double Opacity { get; init; }
                                      public string Label { get; init; }
                                      public Foo(string label) {
                                          Opacity = 1.0;
                                          Label = label;
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantInitSetterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.CountId(diagnostics, "ATXCS065")).IsEqualTo(1);
        var diagnostic = DiagnosticCollectionAssertions.GetSingleById(diagnostics, "ATXCS065");
        await Assert.That(diagnostic.GetMessage(CultureInfo.InvariantCulture).Contains("'Label'")).IsTrue();
    }

    /// <summary>
    ///     Verifies that an init property whose RHS is a numeric literal is not reported.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_DefaultSeedingLiteral_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public sealed class Foo {
                                      public int Count { get; init; }
                                      public Foo(string unrelated) {
                                          Count = 42;
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantInitSetterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS065")).IsFalse();
    }

    /// <summary>
    ///     Verifies that init properties whose RHS is a static method call (with no parameter
    ///     references) are not reported.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_DefaultSeedingMethodCall_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public sealed class Foo {
                                      public Guid Id { get; init; }
                                      public DateTime Timestamp { get; init; }
                                      public Foo() {
                                          Id = Guid.NewGuid();
                                          Timestamp = DateTime.UtcNow;
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantInitSetterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS065")).IsFalse();
    }

    /// <summary>
    ///     Verifies that a property with only a getter (no init setter) is not considered.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_GetOnlyProperty_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public sealed class Foo {
                                      public string Name { get; }
                                      public Foo(string name) {
                                          Name = name;
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantInitSetterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS065")).IsFalse();
    }

    /// <summary>
    ///     Verifies that a property with a get/set pair (no init) is not considered.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_GetSetProperty_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public sealed class Foo {
                                      public string Name { get; set; }
                                      public Foo(string name) {
                                          Name = name;
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantInitSetterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS065")).IsFalse();
    }

    /// <summary>
    ///     Verifies that an interface init-only property declaration is not reported (interfaces
    ///     cannot have constructors).
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_InterfaceInitProperty_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface IFoo {
                                      string Name { get; init; }
                                  }
                              }
                              """;

        var analyzer = new RedundantInitSetterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS065")).IsFalse();
    }

    /// <summary>
    ///     Verifies that an internal init accessor is exempt because the setter is not externally
    ///     callable from other assemblies.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_InternalInit_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public sealed class Foo {
                                      public string Name { get; internal init; }
                                      public Foo(string name) {
                                          Name = name;
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantInitSetterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS065")).IsFalse();
    }

    /// <summary>
    ///     Verifies that a type with two or more instance constructors is exempt (multi-shape
    ///     construction often indicates deserialization or copy semantics).
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_MultipleConstructors_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public sealed class Foo {
                                      public string Name { get; init; }
                                      public Foo() {
                                          Name = "";
                                      }
                                      public Foo(string name) {
                                          Name = name;
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantInitSetterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS065")).IsFalse();
    }

    /// <summary>
    ///     Verifies that a type whose constructor is annotated with the Newtonsoft.Json
    ///     <c>JsonConstructor</c> attribute is exempt.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NewtonsoftJsonConstructor_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace Newtonsoft.Json {
                                  [System.AttributeUsage(System.AttributeTargets.Constructor)]
                                  public sealed class JsonConstructorAttribute : System.Attribute { }
                              }
                              namespace MyApp {
                                  public sealed class Foo {
                                      public string Name { get; init; }
                                      [Newtonsoft.Json.JsonConstructor]
                                      public Foo(string name) {
                                          Name = name;
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantInitSetterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS065")).IsFalse();
    }

    /// <summary>
    ///     Verifies that a type with no instance constructors is exempt (the property exists
    ///     purely for object-initializer construction).
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NoConstructor_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public sealed class Foo {
                                      public string Name { get; init; }
                                      public int Count { get; init; }
                                  }
                              }
                              """;

        var analyzer = new RedundantInitSetterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS065")).IsFalse();
    }

    /// <summary>
    ///     Verifies that a null-coalescing throw expression on the RHS (a common
    ///     validate-then-assign pattern) is reported because it still references the parameter.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NullCoalesceThrowParamAssignment_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System;
                              namespace MyApp {
                                  public sealed class Foo {
                                      public string Name { get; init; }
                                      public Foo(string name) {
                                          Name = name ?? throw new ArgumentNullException(nameof(name));
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantInitSetterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS065")).IsTrue();
    }

    /// <summary>
    ///     Verifies that a private init accessor is exempt because the setter is not callable
    ///     from outside the declaring type.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PrivateInit_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public sealed class Foo {
                                      public string Name { get; private init; }
                                      public Foo(string name) {
                                          Name = name;
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantInitSetterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS065")).IsFalse();
    }

    /// <summary>
    ///     Verifies that a protected init accessor is exempt; only derived classes can use the
    ///     <c>with</c>-expression bypass.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ProtectedInit_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Foo {
                                      public string Name { get; protected init; }
                                      public Foo(string name) {
                                          Name = name;
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantInitSetterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS065")).IsFalse();
    }

    /// <summary>
    ///     The core positive case: a public init-only auto-property whose sole instance ctor
    ///     assigns it from a same-named parameter.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_PureParamAssignment_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public sealed class Foo {
                                      public string Name { get; init; }
                                      public Foo(string name) {
                                          Name = name;
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantInitSetterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS065")).IsTrue();
    }

    /// <summary>
    ///     Verifies that a record with explicit init properties and an explicit constructor
    ///     that assigns each property from its same-named parameter reports one diagnostic per
    ///     property.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_RecordWithParamAssignment_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public sealed record Token {
                                      public string AccessToken { get; init; }
                                      public string RefreshToken { get; init; }
                                      public Token(string accessToken, string refreshToken) {
                                          AccessToken = accessToken;
                                          RefreshToken = refreshToken;
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantInitSetterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.CountId(diagnostics, "ATXCS065")).IsEqualTo(2);
    }

    /// <summary>
    ///     Verifies that a <c>required</c> init property is exempt regardless of whether a
    ///     constructor exists.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_RequiredInit_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public sealed class Foo {
                                      public required string Name { get; init; }
                                  }
                              }
                              """;

        var analyzer = new RedundantInitSetterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS065")).IsFalse();
    }

    /// <summary>
    ///     Verifies that a <c>required init</c> property is exempt even when a constructor
    ///     marked with <c>[SetsRequiredMembers]</c> assigns it from a parameter.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_RequiredInitWithSetsRequiredMembersCtor_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Diagnostics.CodeAnalysis;
                              namespace MyApp {
                                  public sealed class Foo {
                                      public required string Name { get; init; }
                                      [SetsRequiredMembers]
                                      public Foo(string name) {
                                          Name = name;
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantInitSetterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS065")).IsFalse();
    }

    /// <summary>
    ///     Verifies that a static init property is exempt; static lifecycle is different and
    ///     static <c>init</c> is rare and intentional.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StaticProperty_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public sealed class Foo {
                                      public static string Name { get; init; }
                                      static Foo() {
                                          Name = "default";
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantInitSetterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS065")).IsFalse();
    }

    /// <summary>
    ///     Verifies that the rule fires on record-struct init properties just like class init
    ///     properties.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StructWithParamAssignment_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public readonly record struct Foo {
                                      public int Value { get; init; }
                                      public Foo(int value) {
                                          Value = value;
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantInitSetterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS065")).IsTrue();
    }

    /// <summary>
    ///     Verifies that a type whose constructor is annotated with the System.Text.Json
    ///     <c>JsonConstructor</c> attribute is exempt.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_SystemTextJsonConstructor_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              using System.Text.Json.Serialization;
                              namespace MyApp {
                                  public sealed class Foo {
                                      public string Name { get; init; }
                                      [JsonConstructor]
                                      public Foo(string name) {
                                          Name = name;
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantInitSetterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS065")).IsFalse();
    }

    /// <summary>
    ///     Verifies that the rule fires on assignments qualified with <c>this.</c>.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ThisQualifiedParamAssignment_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public sealed class Foo {
                                      public string Name { get; init; }
                                      public Foo(string name) {
                                          this.Name = name;
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantInitSetterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS065")).IsTrue();
    }

    /// <summary>
    ///     Verifies that the rule fires when the RHS transforms the parameter (e.g. method
    ///     invocation), because the init setter still bypasses the transformation via
    ///     <c>with</c>-expression construction.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_TransformedParamAssignment_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public sealed class Foo {
                                      public string Name { get; init; }
                                      public Foo(string name) {
                                          Name = name.Trim();
                                      }
                                  }
                              }
                              """;

        var analyzer = new RedundantInitSetterAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS065")).IsTrue();
    }
}
