using Automaticks.CSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Automaticks.CSharp.Analyzers.Tests;

public class TypeMemberOrderAnalyzerTests
{
    // ── Group ordering: violations ───────────────────────────────────────────

    [Test]
    public async Task Analyze_FieldBeforeAbstractMethod_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public abstract class MyClass {
                                      public int Field;
                                      public abstract void Method();
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsTrue();
    }

    [Test]
    public async Task Analyze_ConstantBeforeAbstractMethod_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public abstract class MyClass {
                                      public const int Max = 100;
                                      public abstract void Method();
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsTrue();
    }

    [Test]
    public async Task Analyze_EventAfterConstant_ReportsDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsTrue();
    }

    [Test]
    public async Task Analyze_FieldBeforeConstant_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class MyClass {
                                      public int Field;
                                      public const int Max = 100;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsTrue();
    }

    [Test]
    public async Task Analyze_IndexerBeforeProperty_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class MyClass {
                                      public int this[int index] => index;
                                      public int Value { get; set; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsTrue();
    }

    [Test]
    public async Task Analyze_ConstructorBeforeIndexer_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class MyClass {
                                      public MyClass() { }
                                      public int this[int index] => index;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsTrue();
    }

    [Test]
    public async Task Analyze_MethodBeforeConstructor_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class MyClass {
                                      public void Method() { }
                                      public MyClass() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsTrue();
    }

    [Test]
    public async Task Analyze_NestedTypeBeforeOwnMethod_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class MyClass {
                                      public class Nested { }
                                      public void Method() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsTrue();
    }

    [Test]
    public async Task Analyze_OwnMethodBeforeOverride_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public abstract class Base {
                                      public abstract void Template();
                                  }
                                  public class MyClass : Base {
                                      public void OwnMethod() { }
                                      public override void Template() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsTrue();
    }

    // ── Group ordering: valid ────────────────────────────────────────────────

    [Test]
    public async Task Analyze_CanonicalFullOrder_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsFalse();
    }

    [Test]
    public async Task Analyze_EmptyClass_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Empty { }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsFalse();
    }

    [Test]
    public async Task Analyze_SingleMember_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class MyClass {
                                      public void Method() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsFalse();
    }

    [Test]
    public async Task Analyze_AbstractBeforeField_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public abstract class MyClass {
                                      public abstract void Method();
                                      public int Field;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsFalse();
    }

    [Test]
    public async Task Analyze_ConstantBeforeField_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class MyClass {
                                      public const int Max = 100;
                                      public int Value;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsFalse();
    }

    [Test]
    public async Task Analyze_PropertyBeforeIndexer_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class MyClass {
                                      public int Value { get; set; }
                                      public int this[int index] => index;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsFalse();
    }

    [Test]
    public async Task Analyze_ConstructorBeforeOwnMethod_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class MyClass {
                                      public MyClass() { }
                                      public void Method() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsFalse();
    }

    [Test]
    public async Task Analyze_OverrideBeforeOwnMethod_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public abstract class Base {
                                      public abstract void Template();
                                  }
                                  public class MyClass : Base {
                                      public override void Template() { }
                                      public void OwnMethod() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsFalse();
    }

    [Test]
    public async Task Analyze_OwnMethodBeforeNestedType_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class MyClass {
                                      public void Method() { }
                                      public class Nested { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsFalse();
    }

    // ── Sub-ordering: violations ─────────────────────────────────────────────

    [Test]
    public async Task Analyze_PrivateFieldBeforePublicField_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class MyClass {
                                      private int _value;
                                      public int Other;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS064")).IsTrue();
    }

    [Test]
    public async Task Analyze_InstanceFieldBeforeStaticFieldSameAccess_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class MyClass {
                                      public int Instance;
                                      public static int Static;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS064")).IsTrue();
    }

    // ── Sub-ordering: valid ──────────────────────────────────────────────────

    [Test]
    public async Task Analyze_PublicBeforeProtectedBeforePrivateField_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class MyClass {
                                      public int PublicField;
                                      protected int ProtectedField;
                                      private int _privateField;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsFalse();
    }

    [Test]
    public async Task Analyze_StaticFieldBeforeInstanceFieldSameAccess_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class MyClass {
                                      public static int Static;
                                      public int Instance;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsFalse();
    }

    // ── Concrete implementation sub-groups ───────────────────────────────────

    [Test]
    public async Task Analyze_ImplicitImplBeforeExplicitImpl_ReportsDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsTrue();
    }

    [Test]
    public async Task Analyze_ExplicitImplBeforeImplicitImpl_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsFalse();
    }

    [Test]
    public async Task Analyze_ImplicitImplBeforeOwnMethod_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsFalse();
    }

    [Test]
    public async Task Analyze_OwnMethodBeforeImplicitImpl_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public interface ICounter { void Increment(); }
                                  public class Counter : ICounter {
                                      public void Reset() { }
                                      public void Increment() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsTrue();
    }

    // ── Type declaration kinds ───────────────────────────────────────────────

    [Test]
    public async Task Analyze_StructWithFieldBeforeConstant_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public struct MyStruct {
                                      public int Field;
                                      public const int Max = 100;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsTrue();
    }

    [Test]
    public async Task Analyze_RecordWithFieldBeforeConstant_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public record MyRecord {
                                      public int Field;
                                      public const int Max = 100;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsTrue();
    }

    [Test]
    public async Task Analyze_RecordStructWithFieldBeforeConstant_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public record struct MyRecordStruct {
                                      public int Field;
                                      public const int Max = 100;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsTrue();
    }

    [Test]
    public async Task Analyze_InterfaceWithMethodBeforeConstant_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public interface IService {
                                      void Execute();
                                      const int Timeout = 30;
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsTrue();
    }

    [Test]
    public async Task Analyze_InterfaceInCorrectOrder_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public interface IService {
                                      const int Timeout = 30;
                                      void AbstractMethod();
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsFalse();
    }

    // ── Nested type declarations ─────────────────────────────────────────────

    [Test]
    public async Task Analyze_NestedClassMembersOutOfOrder_ReportsDiagnosticOnNestedMembers()
    {
        const string source = """
                              namespace MyApp {
                                  public class Outer {
                                      public void OuterMethod() { }
                                      public class Inner {
                                          public int Field;
                                          public const int Max = 100;
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsTrue();
    }

    [Test]
    public async Task Analyze_OuterOrderedNestedOrdered_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class Outer {
                                      public void OuterMethod() { }
                                      public class Inner {
                                          public const int Max = 100;
                                          public int Field;
                                      }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsFalse();
    }

    // ── Partial types (per-file enforcement) ─────────────────────────────────

    [Test]
    public async Task Analyze_PartialClassEachFileCorrect_ReportsNoDiagnostic()
    {
        var sourceFiles = new (string Source, string FilePath)[]
        {
            ("""
             namespace MyApp {
                 public partial class MyClass {
                     public const int Max = 100;
                 }
             }
             """, "FileA.cs"),
            ("""
             namespace MyApp {
                 public partial class MyClass {
                     public void Method() { }
                 }
             }
             """, "FileB.cs")
        };

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), sourceFiles);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsFalse();
    }

    [Test]
    public async Task Analyze_PartialClassOneFileOutOfOrder_ReportsDiagnosticInThatFile()
    {
        var sourceFiles = new (string Source, string FilePath)[]
        {
            ("""
             namespace MyApp {
                 public partial class MyClass {
                     public int Field;
                     public const int Max = 100;
                 }
             }
             """, "FileA.cs"),
            ("""
             namespace MyApp {
                 public partial class MyClass {
                     public void Method() { }
                 }
             }
             """, "FileB.cs")
        };

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), sourceFiles);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsTrue();
    }

    // ── Diagnostic count ─────────────────────────────────────────────────────

    [Test]
    public async Task Analyze_MultipleViolationsInOneType_ReportsOneDiagnosticPerViolation()
    {
        const string source = """
                              namespace MyApp {
                                  public class MyClass {
                                      public int Field;
                                      public const int Max = 100;
                                      public void Method() { }
                                      public MyClass() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Count(d => d.Id == "ATXCS042")).IsEqualTo(2);
    }

    // ── Abstract members sub-ordering ────────────────────────────────────────

    [Test]
    public async Task Analyze_AbstractProperty_PlacedWithAbstractMembers_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsFalse();
    }

    [Test]
    public async Task Analyze_FieldBeforeAbstractProperty_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public abstract class MyClass {
                                      public int Field;
                                      public abstract int Value { get; }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsTrue();
    }

    // ── Readonly field sub-ordering ──────────────────────────────────────────

    [Test]
    public async Task Analyze_ReadonlyFieldBeforeMutableField_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class MyClass {
                                      private readonly int _readonlyField;
                                      private int _mutableField;
                                      public MyClass() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsFalse();
    }

    [Test]
    public async Task Analyze_MutableFieldBeforeReadonlyField_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class MyClass {
                                      private int _mutableField;
                                      private readonly int _readonlyField;
                                      public MyClass() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsTrue();
    }

    [Test]
    public async Task Analyze_ConstantThenReadonlyThenMutableField_ReportsNoDiagnostic()
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

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsFalse();
    }

    [Test]
    public async Task Analyze_MutableFieldBeforeConstant_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class MyClass {
                                      private int _mutableField;
                                      private const int Limit = 10;
                                      public MyClass() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsTrue();
    }

    [Test]
    public async Task Analyze_ReadonlyFieldsAlphabetical_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class MyClass {
                                      private readonly int _alpha;
                                      private readonly int _beta;
                                      private int _zeta;
                                      public MyClass() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS042")).IsFalse();
    }

    [Test]
    public async Task Analyze_ReadonlyFieldsReverseAlphabetical_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class MyClass {
                                      private readonly int _beta;
                                      private readonly int _alpha;
                                      public MyClass() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS064")).IsTrue();
    }

    [Test]
    public async Task Analyze_UnderscorePrefixedFieldBeforeLetterStartingField_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class MyClass {
                                      private int _center;
                                      private bool isCapturingFrames;
                                      public MyClass() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS064")).IsFalse();
    }

    [Test]
    public async Task Analyze_LetterStartingFieldBeforeUnderscorePrefixedField_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class MyClass {
                                      private bool isCapturingFrames;
                                      private int _center;
                                      public MyClass() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS064")).IsTrue();
    }

    [Test]
    public async Task Analyze_MethodsWithShorterNameBeforeLongerSharedPrefix_ReportsNoDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class MyClass {
                                      public void Offset_North_DecrementsY() { }
                                      public void Offset_NorthEast_IncrementsX() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS064")).IsFalse();
    }

    [Test]
    public async Task Analyze_MethodsWithLongerSharedPrefixBeforeShorterName_ReportsDiagnostic()
    {
        const string source = """
                              namespace MyApp {
                                  public class MyClass {
                                      public void Offset_NorthEast_IncrementsX() { }
                                      public void Offset_North_DecrementsY() { }
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestRunner.AnalyzeAsync(new TypeMemberOrderAnalyzer(), source);

        await Assert.That(diagnostics.Any(d => d.Id == "ATXCS064")).IsTrue();
    }
}
