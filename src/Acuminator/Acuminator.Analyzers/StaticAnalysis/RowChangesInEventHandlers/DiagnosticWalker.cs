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
			private readonly PXContext _pxContext;
			private readonly Stack<ImmutableHashSet<ILocalSymbol>> _rowVariables = new Stack<ImmutableHashSet<ILocalSymbol>>();
			private readonly object[] _messageArgs;

			//private readonly Stack<int?> _argNbr = new Stack<int?>();
			private readonly Stack<IParameterSymbol> _arg = new Stack<IParameterSymbol>();

			public DiagnosticWalker(SymbolAnalysisContext context, PXContext pxContext, 
				params object[] messageArgs)
				:base(context.Compilation, context.CancellationToken)
			{
				pxContext.ThrowOnNull(nameof (pxContext));

				_context = context;
				_pxContext = pxContext;
				_messageArgs = messageArgs;
			}

			public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
			{
				var semanticModel = GetSemanticModel(node.SyntaxTree);
				bool needsInitialization = _arg.Count == 0;

				if (needsInitialization)
				{
					var parameter = semanticModel.GetDeclaredSymbol(node)?.Parameters
						.FirstOrDefault(p => p.Type.OriginalDefinition != null 
						                     && _pxContext.Events.EventTypeMap.ContainsKey(p.Type.OriginalDefinition));

					if (parameter != null)
						_arg.Push(parameter);
				}

				// Find all variables that are declared and assigned with a DAC row from e.Row inside the analyzed method
				var variablesWalker = new VariablesWalker(node, semanticModel, _pxContext,
					ContainsRowParameter,
					_context.CancellationToken);
				node.Accept(variablesWalker);

				_rowVariables.Push(variablesWalker.Result);
				base.VisitMethodDeclaration(node);
				_rowVariables.Pop();

				if (needsInitialization)
					_arg.Pop();
			}

			public override void VisitInvocationExpression(InvocationExpressionSyntax node)
			{
				_context.CancellationToken.ThrowIfCancellationRequested();

				var semanticModel = GetSemanticModel(node.SyntaxTree);

				if (!(semanticModel.GetSymbolInfo(node).Symbol is IMethodSymbol methodSymbol)) 
					return;

				bool found = false;
				IParameterSymbol arg = null;

				for (var i = 0; i < node.ArgumentList.Arguments.Count; i++)
				{
					var argumentSyntax = node.ArgumentList.Arguments[i];

					var rowVariables = _rowVariables.Peek();
					var walker = new VariableUsageWalker(rowVariables, semanticModel);
					argumentSyntax.Accept(walker);
						
					if (walker.Success || ContainsRowParameter(argumentSyntax))
					{
						found = true;

						if (methodSymbol.Parameters.Length >= i)
							arg = methodSymbol.Parameters[i];
					}
				}
					
				if (found)
				{
					if (IsMethodForbidden(methodSymbol))
					{
						ReportDiagnostic(_context.ReportDiagnostic, Descriptors.PX1047_RowChangesInEventHandlers, 
							node, _messageArgs);
					}
					else if (arg != null)
					{
						_arg.Push(arg);
						base.VisitInvocationExpression(node);
						_arg.Pop();
					}
				}
			}

			public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
			{
				if (node.Left != null)
				{
					bool found = ContainsRowParameter(node.Left);

					if (!found)
					{
						var rowVariables = _rowVariables.Peek();
						var varWalker = new VariableMemberAccessWalker(rowVariables, GetSemanticModel(node.SyntaxTree));
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

			private bool ContainsRowParameter(CSharpSyntaxNode node)
			{
				if (node == null)
					return false;

				IParameterSymbol arg = _arg.Peek();

				ParameterUsageWalker walker;

				if (arg.Type?.OriginalDefinition != null 
				    && _pxContext.Events.EventTypeMap.ContainsKey(arg.Type.OriginalDefinition))
				{
					walker = new EventArgsRowUsageWalker(arg, GetSemanticModel(node.SyntaxTree), _pxContext);
				}
				else
				{
					walker = new ParameterUsageWalker(arg, GetSemanticModel(node.SyntaxTree));
				}

				node.Accept(walker);
				return walker.Success;
			}
		}
	}
}
