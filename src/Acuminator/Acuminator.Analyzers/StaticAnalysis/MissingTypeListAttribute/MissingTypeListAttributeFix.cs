﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.MissingTypeListAttribute
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class MissingTypeListAttributeFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(Descriptors.PX1002_MissingTypeListAttributeAnalyzer.Id);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync().ConfigureAwait(false);
            var node = (PropertyDeclarationSyntax)root.FindNode(context.Span);

            context.RegisterCodeFix(
                CodeAction.Create(
                    Resources.PX1002Title,
                    c => InsertTypeAttribute(context.Document, node, c),
                    Resources.PX1002Title),
                context.Diagnostics);
        }

        private async Task<Document> InsertTypeAttribute(Document document, PropertyDeclarationSyntax propertyDeclaration, CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var pxContext = new PXContext(semanticModel.Compilation);

            var lists = new List<INamedTypeSymbol> {
                                    pxContext.AttributeTypes.PXIntListAttribute.Type,
                                    pxContext.AttributeTypes.PXStringListAttribute.Type};

            var property = semanticModel.GetDeclaredSymbol(propertyDeclaration);
            var attributeClasses = property.GetAttributes().
                    Select(a => a.AttributeClass);
            var listAttribute = attributeClasses.
                    FirstOrDefault(c => lists.Any(l => c.InheritsFromOrEquals(l, true)));

            AttributeSyntax attr = null;
            if (listAttribute.InheritsFromOrEquals(pxContext.AttributeTypes.PXIntListAttribute.Type))
            {
                attr = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(pxContext.FieldAttributes.PXIntAttribute.Name));
            }
            else if (listAttribute.InheritsFromOrEquals(pxContext.AttributeTypes.PXStringListAttribute.Type))
            {
                attr = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(pxContext.FieldAttributes.PXStringAttribute.Name));
            }
            var attributes = propertyDeclaration.AttributeLists.Add(
                   SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(attr)));
            var root = await document.GetSyntaxRootAsync();
            return document.WithSyntaxRoot(
                root.ReplaceNode(
                    propertyDeclaration,
                    propertyDeclaration.WithAttributeLists(attributes)));
        }
    }
}