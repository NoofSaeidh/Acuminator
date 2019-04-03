﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Syntax;
using Acuminator.Vsix.Utilities;
using Acuminator.Vsix.ChangesClassification;



namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// A class for Code Map specific document changes classification. Contains optimizations for code map.
	/// </summary>
	internal class CodeMapDocChangesClassifier : DocumentChangesClassifier
	{
		public async Task<bool> ShouldRefreshCodeMapAsync(Document oldDocument, SyntaxNode oldRoot, Document newDocument,
														  CancellationToken cancellationToken = default)
		{
			ChangeLocation changeLocation = await GetChangesLocationAsync(oldDocument, oldRoot, newDocument, cancellationToken);
			return changeLocation == ChangeLocation.Class || 
				   changeLocation == ChangeLocation.Namespace;
		}

		protected override ChangeLocation GetChangesLocationImplAsync(Document oldDocument, SyntaxNode oldRoot, Document newDocument,
																	  IEnumerable<TextChange> textChanges, CancellationToken cancellationToken = default)
		{
			ChangeLocation accumulatedChangeLocation = ChangeLocation.None;

			foreach (TextChange change in textChanges)
			{
				ChangeLocation changeLocation = GetTextChangeLocation(change, oldRoot);

				//Early exit if we found a change which require the refresh of code map 
				if (changeLocation.ContainsLocation(ChangeLocation.Class) || changeLocation.ContainsLocation(ChangeLocation.Namespace))
					return changeLocation;

				accumulatedChangeLocation = accumulatedChangeLocation | changeLocation;
				cancellationToken.ThrowIfCancellationRequested();
			}

			return accumulatedChangeLocation;
		}
	}
}
