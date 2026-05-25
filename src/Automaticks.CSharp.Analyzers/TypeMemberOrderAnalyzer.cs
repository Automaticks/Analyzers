using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace Automaticks.CSharp;

/// <summary>
///     Enforces canonical member ordering within every C# type declaration body (class, struct,
///     record, record struct, interface — including nested declarations). The required order is:
///     abstract members, events, constants, fields, properties, indexers, constructors and
///     finalizers, explicit interface implementations, implicit implementations and overrides,
///     own methods (operators and conversions included), nested types.
///     Within each group members are ordered public before protected before private, and static
///     before instance. One diagnostic is reported per out-of-order member.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TypeMemberOrderAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The diagnostic rule reported when a member appears in the wrong section (wrong group).</summary>
    public static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.CSharp.TypeMemberOrder,
        "Type member is declared in the wrong section",
        "'{0}' ({1}) is in the wrong section — {1} members must appear before {2} members. Canonical order: abstract members → events → constants → fields → properties → indexers → constructors → implementations → methods → nested types.",
        "Style",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Move the flagged member to its correct section. The required canonical order inside any type is: (1) abstract members, (2) events, (3) constants, (4) read-only fields, (5) mutable fields, (6) properties, (7) indexers, (8) constructors, (9) explicit interface implementations, (10) overrides and implicit implementations, (11) own methods, (12) nested types.");

    /// <summary>
    ///     The diagnostic rule reported when a member is out of order <em>within</em> its own group
    ///     (access level, static vs instance, or alphabetical name).
    /// </summary>
    public static readonly DiagnosticDescriptor WithinGroupOrderRule = new(
        DiagnosticIds.CSharp.TypeMemberWithinGroupOrder,
        "Type member violates within-group ordering",
        "'{0}' ({1}) is out of order within the {1} group — within the same group the required order is: public before protected before private, static before instance, then alphabetically by name",
        "Style",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Move the flagged member earlier within its group. Within every member group the sub-ordering rules are: (1) public before protected before private, (2) static before instance, (3) alphabetical by member name (case-insensitive).");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule, WithinGroupOrderRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(
            AnalyzeTypeDeclaration,
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKind.RecordDeclaration,
            SyntaxKind.RecordStructDeclaration,
            SyntaxKind.InterfaceDeclaration);
    }

    private static void AnalyzeTypeDeclaration(SyntaxNodeAnalysisContext context)
    {
        var typeDecl = (TypeDeclarationSyntax)context.Node;
        var isInterface = typeDecl is InterfaceDeclarationSyntax;
        var maxRank = MemberRank.Minimum;

        foreach (var member in typeDecl.Members)
        {
            var rank = ComputeRank(member, isInterface, context.SemanticModel);

            if (rank < maxRank)
            {
                ReportViolation(context, member, rank, maxRank);
            }
            else if (rank > maxRank)
            {
                maxRank = rank;
            }
        }
    }

    private static void ReportViolation(
        SyntaxNodeAnalysisContext context,
        MemberDeclarationSyntax member,
        MemberRank rank,
        MemberRank maxRank)
    {
        if (rank.GroupName == maxRank.GroupName)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                WithinGroupOrderRule,
                member.GetFirstToken().GetLocation(),
                GetMemberName(member),
                rank.GroupName));
        }
        else
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Rule,
                member.GetFirstToken().GetLocation(),
                GetMemberName(member),
                rank.GroupName,
                maxRank.GroupName));
        }
    }

    private static MemberRank ComputeRank(MemberDeclarationSyntax member, bool isInterface, SemanticModel semanticModel)
    {
        var groupIndex = GetGroupIndex(member, semanticModel);
        var subGroupIndex = groupIndex == 7 ? GetConcreteImplSubGroup(member) : GetFieldSubGroup(member, groupIndex);
        var accessRank = GetAccessRank(member, isInterface);
        var staticRank = IsStaticMember(member) ? 0 : 1;
        var name = GetMemberName(member);
        return new MemberRank(groupIndex, subGroupIndex, accessRank, staticRank, name);
    }

    private static int GetGroupIndex(MemberDeclarationSyntax member, SemanticModel semanticModel)
    {
        if (member.Modifiers.Any(SyntaxKind.AbstractKeyword))
        {
            return 0;
        }

        if (IsConcreteImplementation(member, semanticModel))
        {
            return 7;
        }

        return member switch
        {
            EventDeclarationSyntax or EventFieldDeclarationSyntax => 1,
            FieldDeclarationSyntax f when f.Modifiers.Any(SyntaxKind.ConstKeyword) => 2,
            FieldDeclarationSyntax => 3,
            PropertyDeclarationSyntax => 4,
            IndexerDeclarationSyntax => 5,
            ConstructorDeclarationSyntax or DestructorDeclarationSyntax => 6,
            MethodDeclarationSyntax or OperatorDeclarationSyntax or ConversionOperatorDeclarationSyntax => 8,
            TypeDeclarationSyntax or EnumDeclarationSyntax or DelegateDeclarationSyntax => 9,
            _ => 9
        };
    }

    private static bool IsConcreteImplementation(MemberDeclarationSyntax member, SemanticModel semanticModel)
    {
        if (member.Modifiers.Any(SyntaxKind.OverrideKeyword))
        {
            return true;
        }

        if (HasExplicitInterfaceSpecifier(member))
        {
            return true;
        }

        return IsImplicitInterfaceImplementation(member, semanticModel);
    }

    private static bool HasExplicitInterfaceSpecifier(MemberDeclarationSyntax member)
    {
        return member switch
        {
            MethodDeclarationSyntax m => m.ExplicitInterfaceSpecifier is not null,
            PropertyDeclarationSyntax p => p.ExplicitInterfaceSpecifier is not null,
            EventDeclarationSyntax e => e.ExplicitInterfaceSpecifier is not null,
            IndexerDeclarationSyntax i => i.ExplicitInterfaceSpecifier is not null,
            _ => false
        };
    }

    private static bool IsImplicitInterfaceImplementation(MemberDeclarationSyntax member, SemanticModel semanticModel)
    {
        if (!member.Modifiers.Any(SyntaxKind.PublicKeyword))
        {
            return false;
        }

        if (member is not (MethodDeclarationSyntax or PropertyDeclarationSyntax or EventDeclarationSyntax or IndexerDeclarationSyntax))
        {
            return false;
        }

        var symbol = semanticModel.GetDeclaredSymbol(member);
        if (symbol?.ContainingType is not { } containingType)
        {
            return false;
        }

        return ContainsInterfaceImplementation(symbol, containingType);
    }

    private static bool ContainsInterfaceImplementation(ISymbol symbol, INamedTypeSymbol containingType)
    {
        foreach (var iface in containingType.AllInterfaces)
        {
            foreach (var ifaceMember in iface.GetMembers())
            {
                var implementation = containingType.FindImplementationForInterfaceMember(ifaceMember);
                if (SymbolEqualityComparer.Default.Equals(implementation, symbol))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static int GetConcreteImplSubGroup(MemberDeclarationSyntax member)
    {
        return HasExplicitInterfaceSpecifier(member) ? 0 : 1;
    }

    private static int GetFieldSubGroup(MemberDeclarationSyntax member, int groupIndex)
    {
        if (groupIndex == 3 && member is FieldDeclarationSyntax field && field.Modifiers.Any(SyntaxKind.ReadOnlyKeyword))
        {
            return 0;
        }

        return groupIndex == 3 ? 1 : 0;
    }

    private static int GetAccessRank(MemberDeclarationSyntax member, bool isInterface)
    {
        var modifiers = member.Modifiers;

        if (member is ConstructorDeclarationSyntax && modifiers.Any(SyntaxKind.StaticKeyword))
        {
            return 0;
        }

        if (modifiers.Any(SyntaxKind.PublicKeyword))
        {
            return 0;
        }

        if (modifiers.Any(SyntaxKind.ProtectedKeyword))
        {
            return 1;
        }

        if (isInterface && !modifiers.Any(SyntaxKind.PrivateKeyword))
        {
            return 0;
        }

        return 2;
    }

    private static bool IsStaticMember(MemberDeclarationSyntax member)
    {
        if (member.Modifiers.Any(SyntaxKind.StaticKeyword))
        {
            return true;
        }

        return member is FieldDeclarationSyntax f && f.Modifiers.Any(SyntaxKind.ConstKeyword);
    }

    private static string GetMemberName(MemberDeclarationSyntax member)
    {
        return member switch
        {
            MethodDeclarationSyntax m => m.Identifier.Text,
            PropertyDeclarationSyntax p => p.Identifier.Text,
            FieldDeclarationSyntax f when f.Declaration.Variables.Count > 0 => f.Declaration.Variables[0].Identifier.Text,
            EventDeclarationSyntax e => e.Identifier.Text,
            EventFieldDeclarationSyntax ef when ef.Declaration.Variables.Count > 0 => ef.Declaration.Variables[0].Identifier.Text,
            IndexerDeclarationSyntax => "this",
            ConstructorDeclarationSyntax c => c.Identifier.Text,
            DestructorDeclarationSyntax d => $"~{d.Identifier.Text}",
            OperatorDeclarationSyntax o => $"operator {o.OperatorToken.Text}",
            ConversionOperatorDeclarationSyntax c => $"{c.ImplicitOrExplicitKeyword.Text} operator",
            TypeDeclarationSyntax t => t.Identifier.Text,
            EnumDeclarationSyntax e => e.Identifier.Text,
            DelegateDeclarationSyntax d => d.Identifier.Text,
            _ => member.GetType().Name
        };
    }
}

/// <summary>
///     A composite rank that captures the canonical position of a type member.
///     Used for lexicographic comparison when enforcing member ordering.
/// </summary>
internal readonly struct MemberRank : IComparable<MemberRank>
{
    private static readonly string[] GroupNames =
    [
        "abstract member",
        "event",
        "constant",
        "field",
        "property",
        "indexer",
        "constructor",
        "concrete implementation",
        "method",
        "nested type"
    ];

    private readonly int _groupIndex;
    private readonly int _subGroupIndex;
    private readonly int _accessRank;
    private readonly int _staticRank;
    private readonly string _name;

    internal MemberRank(int groupIndex, int subGroupIndex, int accessRank, int staticRank, string name)
    {
        _groupIndex = groupIndex;
        _subGroupIndex = subGroupIndex;
        _accessRank = accessRank;
        _staticRank = staticRank;
        _name = name;
    }

    internal static readonly MemberRank Minimum = new(0, 0, 0, 0, string.Empty);

    /// <summary>Human-readable name of this member's canonical group, used in diagnostic messages.</summary>
    internal string GroupName
    {
        get
        {
            if (_groupIndex == 3)
            {
                return _subGroupIndex == 0 ? "read-only field" : "field";
            }

            if (_groupIndex != 7)
            {
                return GroupNames[_groupIndex];
            }

            return _subGroupIndex == 0 ? "explicit interface implementation" : "implicit implementation or override";
        }
    }

    /// <inheritdoc />
    public int CompareTo(MemberRank other)
    {
        var groupComparison = _groupIndex.CompareTo(other._groupIndex);
        if (groupComparison != 0)
        {
            return groupComparison;
        }

        var subGroupComparison = _subGroupIndex.CompareTo(other._subGroupIndex);
        if (subGroupComparison != 0)
        {
            return subGroupComparison;
        }

        var accessComparison = _accessRank.CompareTo(other._accessRank);
        if (accessComparison != 0)
        {
            return accessComparison;
        }

        var staticComparison = _staticRank.CompareTo(other._staticRank);
        if (staticComparison != 0)
        {
            return staticComparison;
        }

        var nameComparison = string.Compare(
            NormalizeNameForComparison(_name),
            NormalizeNameForComparison(other._name),
            StringComparison.OrdinalIgnoreCase);
        return nameComparison;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is MemberRank other && CompareTo(other) == 0;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = 17;
        hash = (hash * 31) + _groupIndex;
        hash = (hash * 31) + _subGroupIndex;
        hash = (hash * 31) + _accessRank;
        hash = (hash * 31) + _staticRank;
        hash = (hash * 31) + StringComparer.OrdinalIgnoreCase.GetHashCode(_name);
        return hash;
    }

    private static string NormalizeNameForComparison(string name) => name.Replace('_', '\x01');

    /// <summary>Returns <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> have different rank.</summary>
    public static bool operator !=(MemberRank left, MemberRank right)
    {
        return left.CompareTo(right) != 0;
    }

    /// <summary>Returns <see langword="true" /> if <paramref name="left" /> ranks strictly before <paramref name="right" />.</summary>
    public static bool operator <(MemberRank left, MemberRank right)
    {
        return left.CompareTo(right) < 0;
    }

    /// <summary>Returns <see langword="true" /> if <paramref name="left" /> ranks before or equal to <paramref name="right" />.</summary>
    public static bool operator <=(MemberRank left, MemberRank right)
    {
        return left.CompareTo(right) <= 0;
    }

    /// <summary>Returns <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> have equal rank.</summary>
    public static bool operator ==(MemberRank left, MemberRank right)
    {
        return left.CompareTo(right) == 0;
    }

    /// <summary>Returns <see langword="true" /> if <paramref name="left" /> ranks strictly after <paramref name="right" />.</summary>
    public static bool operator >(MemberRank left, MemberRank right)
    {
        return left.CompareTo(right) > 0;
    }

    /// <summary>Returns <see langword="true" /> if <paramref name="left" /> ranks after or equal to <paramref name="right" />.</summary>
    public static bool operator >=(MemberRank left, MemberRank right)
    {
        return left.CompareTo(right) >= 0;
    }
}
