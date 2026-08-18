using System.Threading;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests.Formatting;

/// <summary>
///     Tests for TypeMemberOrderAnalyzer.
/// </summary>
public partial class TypeMemberOrderAnalyzerTests
{

    /// <summary>
    ///     Tests that Analyze_AbstractBeforeField_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AbstractBeforeField_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public abstract class MyClass {
                                      public abstract void Method();
                                      public int Field;
                                  }
                              }
                              """;

        var analyzer = new TypeMemberOrderAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS042")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_AbstractProperty_PlacedWithAbstractMembers_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_AbstractProperty_PlacedWithAbstractMembers_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public abstract class MyClass {
                                      public abstract void Method();
                                      public abstract int Value { get; }
                                      public int Field;
                                  }
                              }
                              """;

        var analyzer = new TypeMemberOrderAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS042")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_CanonicalFullOrder_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_CanonicalFullOrder_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public delegate void MyDelegate();
                                  public interface ICounter { void Increment(); }
                                  public abstract class MyClass : ICounter {
                                      public abstract void AbstractMethod();
                                      public event MyDelegate MyEvent;
                                      public const int MaxValue = 100;
                                      public int Field;
                                      public int Property { get; set; }
                                      public int this[int index] => index;
                                      public MyClass() { }
                                      void ICounter.Increment() { }
                                      public override string ToString() { return string.Empty; }
                                      public void OwnMethod() { }
                                      public class NestedClass { }
                                  }
                              }
                              """;

        var analyzer = new TypeMemberOrderAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS042")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ConstantBeforeAbstractMethod_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConstantBeforeAbstractMethod_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public abstract class MyClass {
                                      public const int Max = 100;
                                      public abstract void Method();
                                  }
                              }
                              """;

        var analyzer = new TypeMemberOrderAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS042")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ConstantBeforeField_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConstantBeforeField_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class MyClass {
                                      public const int Max = 100;
                                      public int Value;
                                  }
                              }
                              """;

        var analyzer = new TypeMemberOrderAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS042")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ConstantThenReadonlyThenMutableField_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConstantThenReadonlyThenMutableField_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class MyClass {
                                      private const int Limit = 10;
                                      private readonly int _readonlyField;
                                      private int _mutableField;
                                      public MyClass() { }
                                  }
                              }
                              """;

        var analyzer = new TypeMemberOrderAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS042")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ConstructorBeforeIndexer_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConstructorBeforeIndexer_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class MyClass {
                                      public MyClass() { }
                                      public int this[int index] => index;
                                  }
                              }
                              """;

        var analyzer = new TypeMemberOrderAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS042")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ConstructorBeforeOwnMethod_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConstructorBeforeOwnMethod_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class MyClass {
                                      public MyClass() { }
                                      public void Method() { }
                                  }
                              }
                              """;

        var analyzer = new TypeMemberOrderAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS042")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ConversionOperatorBeforeOperator_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ConversionOperatorBeforeOperator_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp;
                              public class Foo
                              {
                                  public static explicit operator int(Foo value) { return 0; }
                                  public static Foo operator +(Foo left, Foo right) { return left; }
                              }
                              """;

        var analyzer = new TypeMemberOrderAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS042")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_DestructorAfterConstructor_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_DestructorAfterConstructor_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp;
                              public class Foo
                              {
                                  public Foo() { }
                                  ~Foo() { }
                                  public void Bar() { }
                              }
                              """;

        var analyzer = new TypeMemberOrderAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS042")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_EmptyClass_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_EmptyClass_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class Empty { }
                              }
                              """;

        var analyzer = new TypeMemberOrderAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS042")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_EventAfterConstant_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_EventAfterConstant_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public delegate void MyDelegate();
                                  public class MyClass {
                                      public const int Max = 100;
                                      public event MyDelegate MyEvent;
                                  }
                              }
                              """;

        var analyzer = new TypeMemberOrderAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS042")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_EventDeclarationBeforeMethod_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_EventDeclarationBeforeMethod_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp;
                              public delegate void Notifier();
                              public class Foo
                              {
                                  public event Notifier Changed { add { } remove { } }
                                  public void Bar() { }
                              }
                              """;

        var analyzer = new TypeMemberOrderAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS042")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_EventFieldBeforeConstant_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_EventFieldBeforeConstant_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp;
                              public delegate void Notifier();
                              public class Foo
                              {
                                  public event Notifier? Changed;
                                  public const int Limit = 1;
                                  public void Bar() { }
                              }
                              """;

        var analyzer = new TypeMemberOrderAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS042")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExplicitImplBeforeImplicitImpl_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExplicitImplBeforeImplicitImpl_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface IService { void Execute(); }
                                  public class MyService : IService {
                                      void IService.Execute() { }
                                      public void Execute() { }
                                  }
                              }
                              """;

        var analyzer = new TypeMemberOrderAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS042")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ExplicitInterfaceEventBeforeMethod_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ExplicitInterfaceEventBeforeMethod_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp;
                              public delegate void Notifier();
                              public interface IBell { event Notifier Rang; }
                              public class Foo : IBell
                              {
                                  event Notifier IBell.Rang { add { } remove { } }
                                  public void Bar() { }
                              }
                              """;

        var analyzer = new TypeMemberOrderAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS042")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_FieldBeforeAbstractMethod_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FieldBeforeAbstractMethod_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public abstract class MyClass {
                                      public int Field;
                                      public abstract void Method();
                                  }
                              }
                              """;

        var analyzer = new TypeMemberOrderAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS042")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_FieldBeforeAbstractProperty_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FieldBeforeAbstractProperty_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public abstract class MyClass {
                                      public int Field;
                                      public abstract int Value { get; }
                                  }
                              }
                              """;

        var analyzer = new TypeMemberOrderAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS042")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_FieldBeforeConstant_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_FieldBeforeConstant_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class MyClass {
                                      public int Field;
                                      public const int Max = 100;
                                  }
                              }
                              """;

        var analyzer = new TypeMemberOrderAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS042")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ImplicitImplBeforeExplicitImpl_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ImplicitImplBeforeExplicitImpl_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface IService { void Execute(); }
                                  public class MyService : IService {
                                      public void Execute() { }
                                      void IService.Execute() { }
                                  }
                              }
                              """;

        var analyzer = new TypeMemberOrderAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS042")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_ImplicitImplBeforeOwnMethod_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ImplicitImplBeforeOwnMethod_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface ICounter { void Increment(); }
                                  public class Counter : ICounter {
                                      public void Increment() { }
                                      public void Reset() { }
                                  }
                              }
                              """;

        var analyzer = new TypeMemberOrderAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS042")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_IndexerBeforeProperty_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_IndexerBeforeProperty_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class MyClass {
                                      public int this[int index] => index;
                                      public int Value { get; set; }
                                  }
                              }
                              """;

        var analyzer = new TypeMemberOrderAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS042")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_InstanceFieldBeforeStaticFieldSameAccess_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_InstanceFieldBeforeStaticFieldSameAccess_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public class MyClass {
                                      public int Instance;
                                      public static int Static;
                                  }
                              }
                              """;

        var analyzer = new TypeMemberOrderAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS064")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_InterfaceInCorrectOrder_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_InterfaceInCorrectOrder_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface IService {
                                      const int Timeout = 30;
                                      void AbstractMethod();
                                  }
                              }
                              """;

        var analyzer = new TypeMemberOrderAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS042")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_InterfaceWithMethodBeforeConstant_ReportsDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_InterfaceWithMethodBeforeConstant_ReportsDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp {
                                  public interface IService {
                                      void Execute();
                                      const int Timeout = 30;
                                  }
                              }
                              """;

        var analyzer = new TypeMemberOrderAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS042")).IsTrue();
    }

    /// <summary>
    ///     Tests that Analyze_NestedEnumAndDelegateLast_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_NestedEnumAndDelegateLast_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp;
                              public class Foo
                              {
                                  public void Bar() { }
                                  public enum Kind { One }
                                  public delegate void Notifier();
                              }
                              """;

        var analyzer = new TypeMemberOrderAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS042")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_ProtectedMethodBetweenPublicAndPrivate_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_ProtectedMethodBetweenPublicAndPrivate_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp;
                              public class Foo
                              {
                                  public void Alpha() { }
                                  protected void Beta() { }
                                  private void Gamma() { }
                              }
                              """;

        var analyzer = new TypeMemberOrderAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS042")).IsFalse();
    }

    /// <summary>
    ///     Tests that Analyze_StaticConstructorBeforeInstanceConstructor_ReportsNoDiagnostic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Analyze_StaticConstructorBeforeInstanceConstructor_ReportsNoDiagnostic(CancellationToken cancellationToken)
    {
        const string source = """
                              namespace MyApp;
                              public class Foo
                              {
                                  private static readonly int Value;
                                  static Foo() { Value = 1; }
                                  public Foo() { }
                                  public void Bar() { }
                              }
                              """;

        var analyzer = new TypeMemberOrderAnalyzer();
        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(analyzer, source, cancellationToken);

        await Assert.That(DiagnosticCollectionAssertions.HasId(diagnostics, "ATXCS042")).IsFalse();
    }
}
