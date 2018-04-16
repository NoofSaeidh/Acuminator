using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using PX.Data;

namespace Acuminator.Analyzers
{
	public class PXContext
	{
		public PXContext(Compilation compilation)
		{
			Compilation = compilation;
			BQL = new BQLSymbols(Compilation);
		}

		public Compilation Compilation { get; }

		public BQLSymbols BQL { get; }

		public INamedTypeSymbol PXGraphType => Compilation.GetTypeByMetadataName(typeof(PXGraph).FullName);
        public INamedTypeSymbol PXProcessingBaseType => Compilation.GetTypeByMetadataName(typeof(PXProcessingBase<>).FullName);
        public INamedTypeSymbol PXGraphExtensionType => Compilation.GetTypeByMetadataName(typeof(PXGraphExtension).FullName);
		public INamedTypeSymbol PXCacheExtensionType => Compilation.GetTypeByMetadataName(typeof(PXCacheExtension).FullName);
		public INamedTypeSymbol PXMappedCacheExtensionType => Compilation.GetTypeByMetadataName(typeof(PXMappedCacheExtension).FullName);
		public INamedTypeSymbol PXViewType => Compilation.GetTypeByMetadataName(typeof(PXView).FullName);
        public INamedTypeSymbol PXSelectBaseType => Compilation.GetTypeByMetadataName(typeof(PXSelectBase).FullName);
        public INamedTypeSymbol PXActionType => Compilation.GetTypeByMetadataName(typeof(PXAction).FullName);
		public INamedTypeSymbol PXAdapterType => Compilation.GetTypeByMetadataName(typeof(PXAdapter).FullName);
		public INamedTypeSymbol IBqlTableType => Compilation.GetTypeByMetadataName(typeof(IBqlTable).FullName);
		public INamedTypeSymbol IBqlFieldType => Compilation.GetTypeByMetadataName(typeof(IBqlField).FullName);
		//public INamedTypeSymbol PXBaseListAttributeType => Compilation.GetTypeByMetadataName(typeof(PXBaseListAttribute).FullName);
		public INamedTypeSymbol PXStringListAttributeType => Compilation.GetTypeByMetadataName(typeof(PXStringListAttribute).FullName);
        public INamedTypeSymbol PXIntListAttributeType => Compilation.GetTypeByMetadataName(typeof(PXIntListAttribute).FullName);
        public INamedTypeSymbol IPXLocalizableListType => Compilation.GetTypeByMetadataName(typeof(IPXLocalizableList).FullName);
        public INamedTypeSymbol PXIntAttributeType => Compilation.GetTypeByMetadataName(typeof(PXIntAttribute).FullName);
        public INamedTypeSymbol PXShortAttributeType => Compilation.GetTypeByMetadataName(typeof(PXShortAttribute).FullName);
        public INamedTypeSymbol PXStringAttributeType => Compilation.GetTypeByMetadataName(typeof(PXStringAttribute).FullName);
        public INamedTypeSymbol PXByteAttributeType => Compilation.GetTypeByMetadataName(typeof(PXByteAttribute).FullName);
        public INamedTypeSymbol PXDecimalAttributeType => Compilation.GetTypeByMetadataName(typeof(PXDecimalAttribute).FullName);
        public INamedTypeSymbol PXDoubleAttributeType => Compilation.GetTypeByMetadataName(typeof(PXDoubleAttribute).FullName);
        public INamedTypeSymbol PXDBIntAttributeType => Compilation.GetTypeByMetadataName(typeof(PXDBIntAttribute).FullName);
        public INamedTypeSymbol PXDBShortAttributeType => Compilation.GetTypeByMetadataName(typeof(PXDBShortAttribute).FullName);
        public INamedTypeSymbol PXDBStringAttributeType => Compilation.GetTypeByMetadataName(typeof(PXDBStringAttribute).FullName);
        public INamedTypeSymbol PXDBByteAttributeType => Compilation.GetTypeByMetadataName(typeof(PXDBByteAttribute).FullName);
        public INamedTypeSymbol PXDBDecimalAttributeType => Compilation.GetTypeByMetadataName(typeof(PXDBDecimalAttribute).FullName);
        public INamedTypeSymbol PXDBDoubleAttributeType => Compilation.GetTypeByMetadataName(typeof(PXDBDoubleAttribute).FullName);
        public INamedTypeSymbol PXEventSubscriberAttributeType => Compilation.GetTypeByMetadataName(typeof(PXEventSubscriberAttribute).FullName);
        public INamedTypeSymbol PXFieldStateType => Compilation.GetTypeByMetadataName(typeof(PXFieldState).FullName);
        public INamedTypeSymbol PXAttributeFamilyType => Compilation.GetTypeByMetadataName(typeof(PXAttributeFamilyAttribute).FullName);


        #region BQL Types
        /// <summary>
        /// BQL Symbols are stored in separate file.
        /// </summary>
        public class BQLSymbols
		{
			private readonly Compilation compilation;

			public BQLSymbols(Compilation aCompilation)
			{
				compilation = aCompilation;
			}

			public INamedTypeSymbol BqlCommand => compilation.GetTypeByMetadataName(typeof(BqlCommand).FullName);

			public INamedTypeSymbol IBqlField => compilation.GetTypeByMetadataName(typeof(IBqlField).FullName);

			public INamedTypeSymbol IBqlParameter => compilation.GetTypeByMetadataName(typeof(IBqlParameter).FullName);

			public INamedTypeSymbol IBqlJoin => compilation.GetTypeByMetadataName(typeof(IBqlJoin).FullName);

			public INamedTypeSymbol IBqlComparison => compilation.GetTypeByMetadataName(typeof(IBqlComparison).FullName);

			public INamedTypeSymbol IBqlCreator => compilation.GetTypeByMetadataName(typeof(IBqlCreator).FullName);

			public INamedTypeSymbol PXSelect => compilation.GetTypeByMetadataName(typeof(PXSelect<>).FullName);

			public INamedTypeSymbol PXSelectJoin1 => compilation.GetTypeByMetadataName(typeof(PXSelectJoin<,>).FullName);

			public INamedTypeSymbol PXSelectJoin2 => compilation.GetTypeByMetadataName(typeof(PXSelectJoin<,,>).FullName);
			public INamedTypeSymbol PXSelectJoin3 => compilation.GetTypeByMetadataName(typeof(PXSelectJoin<,,,>).FullName);

			public INamedTypeSymbol OrderBy => compilation.GetTypeByMetadataName(typeof(OrderBy<>).FullName);

			public INamedTypeSymbol Asc => compilation.GetTypeByMetadataName(typeof(Asc<>).FullName);

			public INamedTypeSymbol AscWithContinuation => compilation.GetTypeByMetadataName(typeof(Asc<,>).FullName);

			public INamedTypeSymbol Desc => compilation.GetTypeByMetadataName(typeof(Desc<>).FullName);

			public INamedTypeSymbol DescWithContinuation => compilation.GetTypeByMetadataName(typeof(Desc<,>).FullName);
		}
		#endregion
	}
}