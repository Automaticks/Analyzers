using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Automaticks.CSharp.Formatting;

/// <summary>
///     Computes the canonical <see cref="MemberRank" /> of a type member. Shared so the ordering
///     analyzer and its code fix cannot drift apart.
/// </summary>
public static class MemberRankCalculator
{
    /// <summary>
    ///     Computes the canonical rank of <paramref name="member" />.
    /// </summary>
    /// <param name="member">The member to rank.</param>
    /// <param name="isInterface">Whether the declaring type is an interface.</param>
    /// <param name="semanticModel">The semantic model for the member's tree.</param>
    /// <returns>The canonical rank.</returns>
    public static MemberRank Compute(MemberDeclarationSyntax member, bool isInterface, SemanticModel semanticModel)
    {
        var groupIndex = GetGroupIndex(member, semanticModel);
        var subGroupIndex = groupIndex == 7 ? GetConcreteImplSubGroup(member) : GetFieldSubGroup(member, groupIndex);
        var accessRank = GetAccessRank(member, isInterface);
        var staticRank = HasStaticOrConstModifier(member) ? 0 : 1;
        var name = GetMemberName(member);
        return new MemberRank(groupIndex, subGroupIndex, accessRank, staticRank, name);
    }

    /// <summary>
    ///     Returns the display name used for a member in ordering diagnostics.
    /// </summary>
    /// <param name="member">The member whose name is required.</param>
    /// <returns>The member name.</returns>
    public static string GetMemberName(MemberDeclarationSyntax member)
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

    private static int GetGroupIndex(MemberDeclarationSyntax member, SemanticModel semanticModel)
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

    private static string GetSpecialMemberName(MemberDeclarationSyntax member)
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

    private static bool HasExplicitInterfaceSpecifier(MemberDeclarationSyntax member)
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

    private static bool HasImplicitInterfaceImplementation(MemberDeclarationSyntax member, SemanticModel semanticModel)
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

    private static bool HasInterfaceImplementation(ISymbol symbol, INamedTypeSymbol containingType)
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

    private static bool HasOverrideOrImplementation(MemberDeclarationSyntax member, SemanticModel semanticModel)
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

    private static bool HasStaticOrConstModifier(MemberDeclarationSyntax member)
    {
        if (member.Modifiers.Any(SyntaxKind.StaticKeyword))
        {
            return true;
        }

        return member is FieldDeclarationSyntax field && field.Modifiers.Any(SyntaxKind.ConstKeyword);
    }
}
