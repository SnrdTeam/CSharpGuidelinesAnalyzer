using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.ClassDesign
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class TypeShouldHaveASinglePurposeAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1000";

        private const string Title = "Type name contains the word 'and', which suggests in has multiple purposes";
        private const string MessageFormat = "Type '{0}' contains the word 'and', which suggests in has multiple purposes.";
        private const string Description = "A class or interface should have a single purpose.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.ClassDesign;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category.DisplayName, DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private const string BlacklistWord = "and";

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(c => c.SkipEmptyName(AnalyzeType), SymbolKind.NamedType);
        }

        private void AnalyzeType(SymbolAnalysisContext context)
        {
            var type = (INamedTypeSymbol)context.Symbol;

            if (type.IsSynthesized())
            {
                return;
            }

            if (ContainsBlacklistedWord(type.Name))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, type.Locations[0], type.Name));
            }
        }

        private static bool ContainsBlacklistedWord([NotNull] string name)
        {
            return name.ContainsWordInTheMiddle(BlacklistWord);
        }
    }
}
