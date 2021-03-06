﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
    public class StaticConstructorInfo : GraphNodeSymbolItem<ConstructorDeclarationSyntax, IMethodSymbol>
    {
        public StaticConstructorInfo(ConstructorDeclarationSyntax node, IMethodSymbol symbol)
            : base(node, symbol)
        {
        }
    }
}
