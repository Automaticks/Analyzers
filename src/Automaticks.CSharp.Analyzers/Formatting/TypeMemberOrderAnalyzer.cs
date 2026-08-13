using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Automaticks.CSharp.Formatting;

/// <summary>
///     Enforces canonical member ordering within every C# type declaration body.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TypeMemberOrderAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic rule reported when a member appears in the wrong section (wrong group).
    /// </summary>
    public static readonly DiagnosticDescriptor Rule;

    /// <summary>
    ///     The diagnostic rule reported when a member is out of order within its own group.
    /// </summary>
    public static readonly DiagnosticDescriptor WithinGroupOrderRule;

    static TypeMemberOrderAnalyzer()
    {
        var rule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.TypeMemberOrder,
            "Type member is declared in the wrong section",
            "'{0}' ({1}) is in the wrong section — {1} members must appear before {2} members. Canonical order: abstract members → events → constants → fields → properties → indexers → constructors → implementations → methods → nested types.",
            "Style",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            "Move the flagged member to its correct section. The required canonical order inside any type is: (1) abstract members, (2) events, (3) constants, (4) read-only fields, (5) mutable fields, (6) properties, (7) indexers, (8) constructors, (9) explicit interface implementations, (10) overrides and implicit implementations, (11) own methods, (12) nested types.");
        Rule = rule;

        var withinGroupOrderRule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.TypeMemberWithinGroupOrder,
            "Type member violates within-group ordering",
            "'{0}' ({1}) is out of order within the {1} group — within the same group the required order is: public before protected before private, static before instance, then alphabetically by name",
            "Style",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            "Move the flagged member earlier within its group. Within every member group the sub-ordering rules are: (1) public before protected before private, (2) static before instance, (3) alphabetical by member name (case-insensitive).");
        WithinGroupOrderRule = withinGroupOrderRule;
    }

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

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule, WithinGroupOrderRule];

    private void AnalyzeTypeDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not TypeDeclarationSyntax typeDeclaration)
        {
            return;
        }

        var isInterface = typeDeclaration is InterfaceDeclarationSyntax;
        var maxRank = MemberRank.Minimum;

        foreach (var member in typeDeclaration.Members)
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

    private MemberRank ComputeRank(MemberDeclarationSyntax member, bool isInterface, SemanticModel semanticModel)
    {
        var groupIndex = GetGroupIndex(member, semanticModel);
        var subGroupIndex = groupIndex == 7 ? GetConcreteImplSubGroup(member) : GetFieldSubGroup(member, groupIndex);
        var accessRank = GetAccessRank(member, isInterface);
        var staticRank = HasStaticOrConstModifier(member) ? 0 : 1;
        var name = GetMemberName(member);
        return new MemberRank(groupIndex, subGroupIndex, accessRank, staticRank, name);
    }

    private int GetAccessRank(MemberDeclarationSyntax member, bool isInterface)
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

    private int GetConcreteImplSubGroup(MemberDeclarationSyntax member)
    {
        return HasExplicitInterfaceSpecifier(member) ? 0 : 1;
    }

    private int GetFieldSubGroup(MemberDeclarationSyntax member, int groupIndex)
    {
        if (groupIndex == 3 && member is FieldDeclarationSyntax field && field.Modifiers.Any(SyntaxKind.ReadOnlyKeyword))
        {
            return 0;
        }

        return groupIndex == 3 ? 1 : 0;
    }

    private int GetGroupIndex(MemberDeclarationSyntax member, SemanticModel semanticModel)
    {
        if (member.Modifiers.Any(SyntaxKind.AbstractKeyword))
        {
            return 0;
        }

        if (HasOverrideOrImplementation(member, semanticModel))
        {
            return 7;
        }

        return member switch
        {
            EventDeclarationSyntax or EventFieldDeclarationSyntax => 1,
            FieldDeclarationSyntax field when field.Modifiers.Any(SyntaxKind.ConstKeyword) => 2,
            FieldDeclarationSyntax => 3,
            PropertyDeclarationSyntax => 4,
            IndexerDeclarationSyntax => 5,
            ConstructorDeclarationSyntax or DestructorDeclarationSyntax => 6,
            MethodDeclarationSyntax or OperatorDeclarationSyntax or ConversionOperatorDeclarationSyntax => 8,
            TypeDeclarationSyntax or EnumDeclarationSyntax or DelegateDeclarationSyntax => 9,
            _ => 9
        };
    }

    private string GetMemberName(MemberDeclarationSyntax member)
    {
        return member switch
        {
            MethodDeclarationSyntax method => method.Identifier.Text,
            PropertyDeclarationSyntax property => property.Identifier.Text,
            EventDeclarationSyntax eventDeclaration => eventDeclaration.Identifier.Text,
            ConstructorDeclarationSyntax constructor => constructor.Identifier.Text,
            TypeDeclarationSyntax typeDeclaration => typeDeclaration.Identifier.Text,
            EnumDeclarationSyntax enumDeclaration => enumDeclaration.Identifier.Text,
            DelegateDeclarationSyntax delegateDeclaration => delegateDeclaration.Identifier.Text,
            _ => GetSpecialMemberName(member)
        };
    }

    private string GetSpecialMemberName(MemberDeclarationSyntax member)
    {
        return member switch
        {
            FieldDeclarationSyntax field when field.Declaration.Variables.Count > 0 => field.Declaration.Variables[0].Identifier.Text,
            EventFieldDeclarationSyntax eventField when eventField.Declaration.Variables.Count > 0 => eventField.Declaration.Variables[0].Identifier.Text,
            IndexerDeclarationSyntax => "this",
            DestructorDeclarationSyntax destructor => $"~{destructor.Identifier.Text}",
            OperatorDeclarationSyntax operatorDeclaration => $"operator {operatorDeclaration.OperatorToken.Text}",
            ConversionOperatorDeclarationSyntax conversionOperator => $"{conversionOperator.ImplicitOrExplicitKeyword.Text} operator",
            _ => member.GetType().Name
        };
    }

    private bool HasExplicitInterfaceSpecifier(MemberDeclarationSyntax member)
    {
        return member switch
        {
            MethodDeclarationSyntax method => method.ExplicitInterfaceSpecifier is not null,
            PropertyDeclarationSyntax property => property.ExplicitInterfaceSpecifier is not null,
            EventDeclarationSyntax eventDeclaration => eventDeclaration.ExplicitInterfaceSpecifier is not null,
            IndexerDeclarationSyntax indexer => indexer.ExplicitInterfaceSpecifier is not null,
            _ => false
        };
    }

    private bool HasImplicitInterfaceImplementation(MemberDeclarationSyntax member, SemanticModel semanticModel)
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

        return HasInterfaceImplementation(symbol, containingType);
    }

    private bool HasInterfaceImplementation(ISymbol symbol, INamedTypeSymbol containingType)
    {
        foreach (var interfaceType in containingType.AllInterfaces)
        {
            foreach (var interfaceMember in interfaceType.GetMembers())
            {
                var implementation = containingType.FindImplementationForInterfaceMember(interfaceMember);
                if (SymbolEqualityComparer.Default.Equals(implementation, symbol))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool HasOverrideOrImplementation(MemberDeclarationSyntax member, SemanticModel semanticModel)
    {
        if (member.Modifiers.Any(SyntaxKind.OverrideKeyword))
        {
            return true;
        }

        if (HasExplicitInterfaceSpecifier(member))
        {
            return true;
        }

        return HasImplicitInterfaceImplementation(member, semanticModel);
    }

    private bool HasStaticOrConstModifier(MemberDeclarationSyntax member)
    {
        if (member.Modifiers.Any(SyntaxKind.StaticKeyword))
        {
            return true;
        }

        return member is FieldDeclarationSyntax field && field.Modifiers.Any(SyntaxKind.ConstKeyword);
    }

    private void ReportViolation(
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
}
