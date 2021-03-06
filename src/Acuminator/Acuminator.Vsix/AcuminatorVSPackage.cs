﻿using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using Acuminator.Utilities;
using Acuminator.Vsix.Formatter;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Classification;
using Acuminator.Vsix.GoToDeclaration;
using Acuminator.Vsix.ServiceLocation;
using CommonServiceLocator;
using System.Composition.Hosting;
using System.Composition.Hosting.Core;
using Acuminator.Vsix.Settings;
using Acuminator.Vsix.Logger;

using FirstChanceExceptionEventArgs = System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs;



namespace Acuminator.Vsix
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
	//[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndNotBuildingAndNotDebugging_string)] // Auto-load for dynamic menu enabling/disabling; this context seems to work for SSMS and VS
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[Guid(AcuminatorVSPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
                     Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
	[ProvideOptionPage(typeof(GeneralOptionsPage), SettingsCategoryName, GeneralOptionsPage.PageTitle,
					   categoryResourceID: 201, pageNameResourceID: 202, supportsAutomation: true, SupportsProfiles = true)]
	public sealed class AcuminatorVSPackage : Package
    {
		private const string SettingsCategoryName = "Acuminator";

		/// <summary>
		/// AcuminatorVSPackage GUID string.
		/// </summary>
		public const string PackageGuidString = "7e538ed0-0699-434f-acf0-3f6dbc9898ea";

		/// <summary>
		/// The acuminator default command set GUID string.
		/// </summary>
		public const string AcuminatorDefaultCommandSetGuidString = "3cd59430-1e8d-40af-b48d-9007624b3d77";

		[Import]
        internal IClassificationFormatMapService classificationFormatMapService = null;  //Set via MEF

        public IClassificationFormatMapService ClassificationFormatMapService => classificationFormatMapService;

        [Import]
        internal IClassificationTypeRegistryService classificationRegistry = null; // Set via MEF

        public IClassificationTypeRegistryService ClassificationRegistry => classificationRegistry;

        private const int INSTANCE_UNINITIALIZED = 0;
        private const int INSTANCE_INITIALIZED = 1;
        private static int instanceInitialized;

        public static AcuminatorVSPackage Instance { get; private set; }

        private object locker = new object();
        private GeneralOptionsPage generalOptionsPage = null;     
      
        public GeneralOptionsPage GeneralOptionsPage
        {
            get
            {
                if (generalOptionsPage == null)
                {
                    lock (locker)
                    {
                        if (generalOptionsPage == null)
                        {
                            generalOptionsPage = GetDialogPage(typeof(GeneralOptionsPage)) as GeneralOptionsPage;
                        }
                    }
                }

                return generalOptionsPage;
            }
        }

		internal AcuminatorLogger AcuminatorLogger
		{
			get;
			private set;
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="AcuminatorVSPackage"/> class.
        /// </summary>
        public AcuminatorVSPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        
            SetupSingleton(this);
        }
          
        private static void SetupSingleton(AcuminatorVSPackage package)
        {
            if (package == null)
                return;

            if (Interlocked.CompareExchange(ref instanceInitialized, INSTANCE_INITIALIZED, INSTANCE_UNINITIALIZED) == INSTANCE_UNINITIALIZED)
            {
                Instance = package;
            }
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            FormatBqlCommand.Initialize(this);
			GoToDeclarationOrHandlerCommand.Initialize(this);
            base.Initialize();

            IComponentModel componentModel = Package.GetGlobalService(typeof(SComponentModel)) as IComponentModel;

            if (componentModel == null)
                return;

			InitializeLogger();

			try
			{
				componentModel.DefaultCompositionService.SatisfyImportsOnce(this);

				var container = new CompositionContainer(CompositionOptions.Default, componentModel.DefaultExportProvider);
				container.ComposeExportedValue<CodeAnalysisSettings>(new CodeAnalysisSettingsFromOptionsPage(GeneralOptionsPage));

				// Service Locator
				IServiceLocator serviceLocator = new MefServiceLocator(container);

				if (ServiceLocator.IsLocationProviderSet)
					serviceLocator = new DelegatingServiceLocator(ServiceLocator.Current, serviceLocator);

				ServiceLocator.SetLocatorProvider(() => serviceLocator);
			}
			catch
			{
				// Exception will be logged in FCEL
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			AcuminatorLogger?.Dispose();
		}

	    private void InitializeLogger()
	    {
		    try
		    {
			    AcuminatorLogger = new AcuminatorLogger(this);
		    }
		    catch (Exception ex)
		    {
			    ActivityLog.TryLogError(AcuminatorLogger.PackageName,
				    $"An error occurred during the logger initialization ({ex.GetType().Name}, message: \"{ex.Message}\")");
		    }
	    }

		#region Package Settings         
		public bool ColoringEnabled => GeneralOptionsPage?.ColoringEnabled ?? true;


        public bool UseRegexColoring => GeneralOptionsPage?.UseRegexColoring ?? false;

        public bool UseBqlOutlining => GeneralOptionsPage?.UseBqlOutlining ?? true;

		public bool UseBqlDetailedOutlining => GeneralOptionsPage?.UseBqlDetailedOutlining ?? true;

        public bool PXGraphColoringEnabled => GeneralOptionsPage?.PXGraphColoringEnabled ?? true;
        
        public bool PXActionColoringEnabled => GeneralOptionsPage?.PXActionColoringEnabled ?? true;

        public bool ColorOnlyInsideBQL => GeneralOptionsPage?.ColorOnlyInsideBQL ?? false;
        #endregion
    }
}
