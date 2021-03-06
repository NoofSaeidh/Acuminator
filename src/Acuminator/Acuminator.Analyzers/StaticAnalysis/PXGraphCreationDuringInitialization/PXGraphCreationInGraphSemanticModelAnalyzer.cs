﻿using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Acuminator.Analyzers.StaticAnalysis.PXGraphCreationDuringInitialization
{
    public class PXGraphCreationInGraphSemanticModelAnalyzer : IPXGraphAnalyzer
    {
        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(
                Descriptors.PX1057_PXGraphCreationDuringInitialization,
                Descriptors.PX1084_GraphCreationInDataViewDelegate);

        public void Analyze(SymbolAnalysisContext context, PXContext pxContext, CodeAnalysisSettings settings, PXGraphSemanticModel pxGraph)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            PXGraphCreateInstanceWalker walker = new PXGraphCreateInstanceWalker(context, pxContext, Descriptors.PX1057_PXGraphCreationDuringInitialization);

            foreach(GraphInitializerInfo initializer in pxGraph.Initializers)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                walker.Visit(initializer.Node);
            }

            walker = new PXGraphCreateInstanceWalker(context, pxContext, Descriptors.PX1084_GraphCreationInDataViewDelegate);

            foreach(DataViewDelegateInfo del in pxGraph.ViewDelegates)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                walker.Visit(del.Node);
            }
        }

        private class PXGraphCreateInstanceWalker : NestedInvocationWalker
        {
            private readonly SymbolAnalysisContext _context;
            private readonly PXContext _pxContext;
            private readonly DiagnosticDescriptor _descriptor;

            public PXGraphCreateInstanceWalker(SymbolAnalysisContext context, PXContext pxContext, DiagnosticDescriptor descriptor)
                : base(context.Compilation, context.CancellationToken)
            {
                _context = context;
                _pxContext = pxContext;
                _descriptor = descriptor;
            }

            public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
            {
                _context.CancellationToken.ThrowIfCancellationRequested();

                IMethodSymbol symbol = GetSymbol<IMethodSymbol>(node);

                if (symbol != null && _pxContext.PXGraph.CreateInstance.Contains(symbol.ConstructedFrom))
                {
                    ReportDiagnostic(_context.ReportDiagnostic, _descriptor, node);
                }
                else
                {
                    base.VisitMemberAccessExpression(node);
                }
            }
        }
    }
}
