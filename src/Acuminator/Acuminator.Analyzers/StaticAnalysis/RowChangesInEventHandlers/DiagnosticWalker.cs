using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.RowChangesInEventHandlers
{
	public partial class RowChangesInEventHandlersAnalyzer
	{
		private class DiagnosticWalker : NestedInvocationWalker
		{
			private static readonly ISet<string> MethodNames = new HashSet<string>(StringComparer.Ordinal)
			{
				"SetValue" ,
				"SetValueExt",
				"SetDefaultExt",
			};

			private readonly SymbolAnalysisContext _context;
			private readonly SemanticModel _semanticModel;
			private readonly PXContext _pxContext;
			private readonly Stack<ImmutableHashSet<ILocalSymbol>> _rowVariables = new Stack<ImmutableHashSet<ILocalSymbol>>();
			private readonly object[] _messageArgs;

			public DiagnosticWalker(SymbolAnalysisContext context, SemanticModel semanticModel, PXContext pxContext, 
				params object[] messageArgs)
				:base(context.Compilation, context.CancellationToken)
			{
				pxContext.ThrowOnNull(nameof (pxContext));

				_context = context;
				_semanticModel = semanticModel;
				_pxContext = pxContext;
				_messageArgs = messageArgs;
			}

			public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
			{
				var semanticModel = GetSemanticModel(node.SyntaxTree);

				// Find all variables that are declared and assigned with a DAC row from e.Row inside the analyzed method
				var variablesWalker = new VariablesWalker(node, semanticModel, _pxContext,
					ContainsEventArgsRow,
					_context.CancellationToken);
				node.Accept(variablesWalker);

				_rowVariables.Push(variablesWalker.Result);
				base.VisitMethodDeclaration(node);
				_rowVariables.Pop();
			}

			public override void VisitInvocationExpression(InvocationExpressionSyntax node)
			{
				_context.CancellationToken.ThrowIfCancellationRequested();

				if (_semanticModel.GetSymbolInfo(node).Symbol is IMethodSymbol methodSymbol)
				{
					int? argumentNbr = null;

					for (var i = 0; i < node.ArgumentList.Arguments.Count; i++)
					{
						var argumentSyntax = node.ArgumentList.Arguments[i];

						var rowVariables = _rowVariables.Peek();
						var walker = new VariableUsageWalker(rowVariables, _semanticModel);
						argumentSyntax.Accept(walker);
						
						if (walker.Success || ContainsEventArgsRow(argumentSyntax))
						{
							argumentNbr = i;
							break;
						}
					}
					
					if (argumentNbr != null)
					{
						if (IsMethodForbidden(methodSymbol))
						{
							ReportDiagnostic(_context.ReportDiagnostic, Descriptors.PX1047_RowChangesInEventHandlers, 
								node, _messageArgs);
						}
						else
						{
							base.VisitInvocationExpression(node);
						}
					}
				}
			}

			public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
			{
				if (node.Left != null)
				{
					bool found = ContainsEventArgsRow(node.Left);

					if (!found)
					{
						var rowVariables = _rowVariables.Peek();
						var varWalker = new VariableMemberAccessWalker(rowVariables, _semanticModel);
						node.Left.Accept(varWalker);
						found = varWalker.Success;
					}
					
					if (found)
					{
						ReportDiagnostic(_context.ReportDiagnostic, Descriptors.PX1047_RowChangesInEventHandlers, node, _messageArgs);
					}
				}
			}

			public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
			{
			}

			public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
			{
			}

			public override void VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
			{
			}


			private bool IsMethodForbidden(IMethodSymbol symbol)
			{
				return symbol.ContainingType?.OriginalDefinition != null
				       && symbol.ContainingType.OriginalDefinition.InheritsFromOrEquals(_pxContext.PXCacheType)
				       && MethodNames.Contains(symbol.Name);
			}

			private bool ContainsEventArgsRow(CSharpSyntaxNode node)
			{
				if (node == null)
					return false;

				var walker = new EventArgsRowWalker(GetSemanticModel(node.SyntaxTree), _pxContext);
				node.Accept(walker);

				return walker.Success;
			}
		}

	}
}
