using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.RowChangesInEventHandlers
{
	public partial class RowChangesInEventHandlersAnalyzer
	{
		/// <summary>
		/// Searches for <code>e.Row</code> usages
		/// </summary>
		private class EventArgsRowUsageWalker : ParameterUsageWalker
		{
			private static readonly string RowPropertyName = "Row";
			
			private readonly PXContext _pxContext;

			public EventArgsRowUsageWalker(IParameterSymbol parameter, SemanticModel semanticModel, PXContext pxContext)
				: base(parameter, semanticModel)
			{
				semanticModel.ThrowOnNull(nameof(semanticModel));
				pxContext.ThrowOnNull(nameof(pxContext));

				_pxContext = pxContext;
			}

			public override void Visit(SyntaxNode node)
			{
				if (!Success)
					base.Visit(node);
			}

			public override void VisitIdentifierName(IdentifierNameSyntax node)
			{
				base.VisitIdentifierName(node);

				if (!Success && node.Identifier.Text == RowPropertyName)
				{
					var propertySymbol = SemanticModel.GetSymbolInfo(node).Symbol as IPropertySymbol;
					var containingType = propertySymbol?.ContainingType?.OriginalDefinition;

					if (containingType != null && _pxContext.Events.EventTypeMap.ContainsKey(containingType))
					{
						Success = true;
					}
				}
			}
		}

	}
}
