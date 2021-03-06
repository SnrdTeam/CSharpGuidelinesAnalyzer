using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AvoidMemberWithManyStatementsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1500";

        private const int MaxStatementCount = 7;
        private const string MaxStatementCountText = "7";

        private const string Title = "Member or local function contains more than " + MaxStatementCountText + " statements";

        private const string MessageFormat = "{0} '{1}' contains {2} statements, which exceeds the maximum of " +
            MaxStatementCountText + " statements.";

        private const string Description = "Methods should not exceed " + MaxStatementCountText + " statements.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category.DisplayName, DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        [NotNull]
        private static readonly Action<CodeBlockAnalysisContext> AnalyzeCodeBlockAction = AnalyzeCodeBlock;

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCodeBlockAction(AnalyzeCodeBlockAction);
        }

        private static void AnalyzeCodeBlock(CodeBlockAnalysisContext context)
        {
            if (context.OwningSymbol is INamedTypeSymbol || context.OwningSymbol.IsSynthesized())
            {
                return;
            }

            var statementWalker = new StatementWalker(context.CancellationToken);
            statementWalker.Visit(context.CodeBlock);

            if (statementWalker.StatementCount > MaxStatementCount)
            {
                ReportAtContainingSymbol(statementWalker.StatementCount, context);
            }
        }

        private static void ReportAtContainingSymbol(int statementCount, CodeBlockAnalysisContext context)
        {
            string kind = GetMemberKind(context.OwningSymbol, context.CancellationToken);
            string memberName = context.OwningSymbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
            Location location = GetMemberLocation(context.OwningSymbol, context.SemanticModel, context.CancellationToken);

            context.ReportDiagnostic(Diagnostic.Create(Rule, location, kind, memberName, statementCount));
        }

        [NotNull]
        private static string GetMemberKind([NotNull] ISymbol member, CancellationToken cancellationToken)
        {
            Guard.NotNull(member, nameof(member));

            foreach (SyntaxNode syntax in member.DeclaringSyntaxReferences.Select(reference =>
                reference.GetSyntax(cancellationToken)))
            {
                if (syntax is VariableDeclaratorSyntax || syntax is PropertyDeclarationSyntax)
                {
                    return "Initializer for";
                }
            }

            return member.GetKind();
        }

        [NotNull]
        private static Location GetMemberLocation([NotNull] ISymbol member, [NotNull] SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            foreach (ArrowExpressionClauseSyntax arrowExpressionClause in member.DeclaringSyntaxReferences
                .Select(reference => reference.GetSyntax(cancellationToken)).OfType<ArrowExpressionClauseSyntax>())
            {
                ISymbol parentSymbol = semanticModel.GetDeclaredSymbol(arrowExpressionClause.Parent);
                if (parentSymbol != null && parentSymbol.Locations.Any())
                {
                    return parentSymbol.Locations[0];
                }
            }

            return member.Locations[0];
        }

        private sealed class StatementWalker : CSharpSyntaxWalker
        {
            private CancellationToken cancellationToken;

            public int StatementCount { get; private set; }

            public StatementWalker(CancellationToken cancellationToken)
            {
                this.cancellationToken = cancellationToken;
            }

            public override void Visit([NotNull] SyntaxNode node)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (IsStatement(node))
                {
                    StatementCount++;
                }

                base.Visit(node);
            }

            private bool IsStatement([NotNull] SyntaxNode node)
            {
                return !node.IsMissing && node is StatementSyntax && !IsExcludedStatement(node);
            }

            private bool IsExcludedStatement([NotNull] SyntaxNode node)
            {
                return node is BlockSyntax || node is LabeledStatementSyntax || node is LocalFunctionStatementSyntax;
            }
        }
    }
}
