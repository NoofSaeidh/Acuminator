using Acuminator.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator.Analyzers.Analyzers.DAC
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DacPropertyModifiersAnalyzer : PXDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptors.PX1056_DacPropertiesShouldBeVirtual);

        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
        {
            compilationStartContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzeDacPropertyModifiers(syntaxContext, pxContext), SyntaxKind.ClassDeclaration);
        }

        private void AnalyzeDacPropertyModifiers(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
        {
            syntaxContext.CancellationToken.ThrowIfCancellationRequested();

            if (!(syntaxContext.Node is ClassDeclarationSyntax classDeclaration))
                return;

            INamedTypeSymbol typeSymbol = syntaxContext.SemanticModel.GetDeclaredSymbol(classDeclaration, syntaxContext.CancellationToken);
            if (typeSymbol == null || !typeSymbol.IsDacOrExtension(pxContext))
                return;

            IEnumerable<PropertyDeclarationSyntax> properties = classDeclaration.DescendantNodes()
                                                                .OfType<PropertyDeclarationSyntax>();

        }
    }
}
