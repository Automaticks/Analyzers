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
            "'{0}' ({1}) is in the wrong section — {1} members must appear before {2} members. Canonical order: abstract members → events → constants → fields → properties → indexers → constructors → implementations → methods → nested types. A code fix is available (dotnet format analyzers --diagnostics ATXCS042).",
            "Style",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            "Move the flagged member to its correct section. The required canonical order inside any type is: (1) abstract members, (2) events, (3) constants, (4) read-only fields, (5) mutable fields, (6) properties, (7) indexers, (8) constructors, (9) explicit interface implementations, (10) overrides and implicit implementations, (11) own methods, (12) nested types.");
        Rule = rule;

        var withinGroupOrderRule = new DiagnosticDescriptor(
            DiagnosticIds.CSharp.TypeMemberWithinGroupOrder,
            "Type member violates within-group ordering",
            "'{0}' ({1}) is out of order within the {1} group — within the same group the required order is: public before protected before private, static before instance, then alphabetically by name. A code fix is available (dotnet format analyzers --diagnostics ATXCS064).",
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
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [Rule, WithinGroupOrderRule];
        }
    }

    private void AnalyzeTypeDeclaration(SyntaxNodeAnalysisContext context)
    {
        var typeDeclaration = (context.Node as TypeDeclarationSyntax)!;
        var isInterface = typeDeclaration is InterfaceDeclarationSyntax;
        var maxRank = MemberRank.Minimum;

        foreach (var member in typeDeclaration.Members)
        {
            var rank = MemberRankCalculator.Compute(member, isInterface, context.SemanticModel);

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
                MemberRankCalculator.GetMemberName(member),
                rank.GroupName));
        }
        else
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Rule,
                member.GetFirstToken().GetLocation(),
                MemberRankCalculator.GetMemberName(member),
                rank.GroupName,
                maxRank.GroupName));
        }
    }
}
