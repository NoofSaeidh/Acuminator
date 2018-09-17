using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FindSymbols;
using PX.SM;

namespace Acuminator.Analyzers.StaticAnalysis.RowChangesInEventHandlers
{
	public partial class RowChangesInEventHandlersAnalyzer : IEventHandlerAnalyzer
	{
		private static readonly ISet<EventType> AnalyzedEventTypes = new HashSet<EventType>()
		{
			EventType.FieldDefaulting,
			EventType.FieldVerifying,
			EventType.RowSelected,
		};

		public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
			ImmutableArray.Create(Descriptors.PX1047_RowChangesInEventHandlers);

		public void Analyze(SymbolAnalysisContext context, PXContext pxContext, EventType eventType)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (AnalyzedEventTypes.Contains(eventType))
			{
				var methodSymbol = (IMethodSymbol) context.Symbol;
				var methodSyntax = methodSymbol.GetSyntax(context.CancellationToken) as MethodDeclarationSyntax;

				if (methodSyntax != null)
				{
					var semanticModel = context.Compilation.GetSemanticModel(methodSyntax.SyntaxTree, true);
					
					// Find all variables that are declared and assigned with e.Row inside the analyzed method
					var variablesWalker = new VariablesWalker(methodSyntax, semanticModel, pxContext,
						node => ContainsEventArgsRow(node, semanticModel, pxContext),
						context.CancellationToken);
					methodSyntax.Accept(variablesWalker);

					// Perform analysis
					var diagnosticWalker = new DiagnosticWalker(context, semanticModel, pxContext, variablesWalker.Result,
						node => ContainsEventArgsRow(node, semanticModel, pxContext), eventType);
					methodSyntax.Accept(diagnosticWalker);
				}
			}
		}

		private static bool ContainsEventArgsRow(CSharpSyntaxNode node, SemanticModel semanticModel, PXContext pxContext)
		{
			if (node == null)
				return false;

			var walker = new EventArgsRowWalker(semanticModel, pxContext);
			node.Accept(walker);

			return walker.Success;
		}
	}
}
