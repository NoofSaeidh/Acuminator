﻿using System;
using System.Composition;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Editing;
using Acuminator.Utilities;
using PX.Data;

namespace Acuminator.Analyzers.FixProviders
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp)]
	public class DacExtensionDefaultAttributeFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1030_DefaultAttibuteToExisitingRecords.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			return Task.Run(() =>
			{
				var diagnostic = context.Diagnostics.FirstOrDefault(d => d.Id == Descriptors.PX1030_DefaultAttibuteToExisitingRecords.Id);

				if (diagnostic == null)
					return;

				string codeActionName = nameof(Resources.PX1030Fix).GetLocalized().ToString();
				CodeAction codeAction =
					CodeAction.Create(codeActionName,
									  cToken => ReplaceIncorrectDefaultAttribute(context.Document, context.Span, diagnostic.IsBoundField(),cToken),
									  equivalenceKey: codeActionName);

				context.RegisterCodeFix(codeAction, context.Diagnostics);
			}, context.CancellationToken);
		}

		private async Task<Document> ReplaceIncorrectDefaultAttribute(Document document, TextSpan span, bool isBoundField,CancellationToken cancellationToken)
		{
			SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			 if (!(root?.FindNode(span) is AttributeSyntax attributeNode))
				return document;
			if (!(attributeNode.Parent is AttributeListSyntax attributeList))
				return document;
			if (cancellationToken.IsCancellationRequested)
				return document;

			SyntaxGenerator generator = SyntaxGenerator.GetGenerator(document);
			var memberAccessExpression = generator.MemberAccessExpression(generator.IdentifierName("PXPersistingCheck"), generator.IdentifierName("Nothing"));

			var persistingAttributeArgument = generator.AttributeArgument("PersistingCheck", memberAccessExpression) as AttributeArgumentSyntax;

			var pxUnboundDefaultAttribute = generator.Attribute("PXUnboundDefault") as AttributeListSyntax;
			
			SyntaxNode modifiedRoot;
			if (isBoundField)
			{
				if (attributeNode.ArgumentList != null)
				{
					var persistingCheckArguments = from arguments in attributeNode.ArgumentList.Arguments
													where arguments.GetText().ToString().Contains("PersistingCheck")
													select arguments;
					//if attribute contains PersistingCheck
					if (persistingCheckArguments.Any())
					{
						AttributeArgumentSyntax argument = persistingCheckArguments.First();
						
						persistingAttributeArgument = argument.ReplaceNode(argument.Expression, memberAccessExpression);
						var newAttributeNode = attributeNode.ReplaceNode(argument, persistingAttributeArgument);
						var newAttributeList = attributeList.ReplaceNode(attributeNode, newAttributeNode);
						modifiedRoot = root.ReplaceNode(attributeList, newAttributeList);
					}
					else
					{
						//SyntaxNode newAttributeList = generator.AddAttributes(new AttributeListSyntax(), new SyntaxNode);
						var newAttributeList = generator.AddAttributeArguments(attributeNode, new SyntaxNode[] { persistingAttributeArgument }) as AttributeListSyntax;
						//var newAttributeList = attributeList.ReplaceNode(attributeNode, newAttributeNode);
						modifiedRoot = root.ReplaceNode(attributeNode, newAttributeList.Attributes[0]);
					}

/*					SyntaxNode newAttributeNode = generator.InsertAttributeArguments(attributeNode, 1, new SyntaxNode[] { persistingAttributeArgument });
					
					//var newAttribute = generator.InsertAttributeArguments(attributeNode, 1, new SyntaxNode[] { persistingAttributeArgument });
					modifiedRoot = root.ReplaceNode(attributeNode, newAttributeNode);*/
				}
				else
				{
					AttributeListSyntax newAttribute = generator.InsertAttributeArguments(attributeNode, 1, new SyntaxNode[] { persistingAttributeArgument }) as AttributeListSyntax;
					modifiedRoot = root.ReplaceNode(attributeNode, newAttribute.Attributes[0]);
				}
			
				
			}
			else
			{
				modifiedRoot = root.ReplaceNode(attributeNode, pxUnboundDefaultAttribute.Attributes[0]);
			}

			

			return document.WithSyntaxRoot(modifiedRoot);
		}
	}
}