using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace PX.Analyzers.Vsix.CodeSnippets
{
	static class SnippetUtilities
	{
		internal const string LanguageServiceGuidStr = "7e697725-6e97-4405-9766-c8a774d3e503";
	}

	[ProvideLanguageCodeExpansion(
		SnippetUtilities.LanguageServiceGuidStr,
		"TestSnippets", //the language name
		0,              //the resource id of the language
		"TestSnippets", //the language ID used in the .snippet files
		@"C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\TestSnippets\Snippets\1033\TestSnippets.xml",
//the path of the index file
		SearchPaths = @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\TestSnippets\Snippets\1033\",
		ForceCreateDirs = @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\TestSnippets\Snippets\1033\")]
	internal class SnippetsRegistration
	{
		
	}
}
