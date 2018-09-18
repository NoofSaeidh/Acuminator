using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.RowChangesInEventHandlers
{
	public partial class RowChangesInEventHandlersAnalyzer
	{
		/// <summary>
		/// Searches for usages of a local variable
		/// </summary>
		private class VariableUsageWalker : CSharpSyntaxWalker
		{
			private readonly ImmutableHashSet<ILocalSymbol> _variables;
			private readonly SemanticModel _semanticModel;

			public bool Success { get; private set; }

			public VariableUsageWalker(ImmutableHashSet<ILocalSymbol> variables, SemanticModel semanticModel)
			{
				_variables = variables;
				_semanticModel = semanticModel;
			}

			public override void Visit(SyntaxNode node)
			{
				if (!Success)
					base.Visit(node);
			}

			public override void VisitIdentifierName(IdentifierNameSyntax node)
			{
				if (IsVariable(node))
					Success = true;
			}

			public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
			{
			}

			public override void VisitAnonymousObjectCreationExpression(AnonymousObjectCreationExpressionSyntax node)
			{
			}

			private bool IsVariable(ExpressionSyntax node)
			{
				return node != null
				       && _semanticModel.GetSymbolInfo(node).Symbol is ILocalSymbol variable
				       && _variables.Contains(variable);
			}
		}

	}
}
