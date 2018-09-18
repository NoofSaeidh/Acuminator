﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
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
		/// Collects all variables that are declared inside the method, and assigned with <code>e.Row</code>
		/// </summary>
		private class VariablesWalker : CSharpSyntaxWalker
		{
			private readonly SemanticModel _semanticModel;
			private readonly PXContext _pxContext;
			private readonly Func<CSharpSyntaxNode, bool> _predicate;
			private CancellationToken _cancellationToken;
			private readonly ImmutableHashSet<ILocalSymbol> _variables;

			private readonly ISet<ILocalSymbol> _result = new HashSet<ILocalSymbol>();
			public ImmutableHashSet<ILocalSymbol> Result => _result.ToImmutableHashSet();

			public VariablesWalker(MethodDeclarationSyntax methodSyntax, SemanticModel semanticModel, PXContext pxContext,
				Func<CSharpSyntaxNode, bool> predicate, CancellationToken cancellationToken)
			{
				methodSyntax.ThrowOnNull(nameof (methodSyntax));
				semanticModel.ThrowOnNull(nameof (semanticModel));
				pxContext.ThrowOnNull(nameof (pxContext));

				_semanticModel = semanticModel;
				_pxContext = pxContext;
				_predicate = predicate;
				_cancellationToken = cancellationToken;

				if (methodSyntax.Body != null || methodSyntax.ExpressionBody?.Expression != null)
				{
					var dataFlow = methodSyntax.Body != null
						? semanticModel.AnalyzeDataFlow(methodSyntax.Body)
						: semanticModel.AnalyzeDataFlow(methodSyntax.ExpressionBody.Expression);

					if (dataFlow.Succeeded)
					{
						_variables = dataFlow.WrittenInside
							.Intersect(dataFlow.VariablesDeclared)
							.OfType<ILocalSymbol>()
							.ToImmutableHashSet();
					}
				}
				
			}

			public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
			{
				_cancellationToken.ThrowIfCancellationRequested();

				if (node.Left is IdentifierNameSyntax variableNode && node.Right != null)
				{
					var variableSymbol = _semanticModel.GetSymbolInfo(variableNode).Symbol as ILocalSymbol;

					if (variableSymbol != null && _variables.Contains(variableSymbol)
					    && _predicate(node.Right))
					{
						_result.Add(variableSymbol);
					}
				}
			}

			public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
			{
				foreach (var variableDeclarator in node.Variables
					.Where(v => v.Initializer?.Value != null))
				{
					var variableSymbol = _semanticModel.GetDeclaredSymbol(variableDeclarator) as ILocalSymbol;

					if (variableSymbol != null && _predicate(variableDeclarator.Initializer.Value))
						_result.Add(variableSymbol);
				}
			}
		}

	}
}