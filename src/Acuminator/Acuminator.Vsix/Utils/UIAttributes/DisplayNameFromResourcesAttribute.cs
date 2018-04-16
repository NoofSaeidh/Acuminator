﻿using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Acuminator.Vsix;
using Acuminator.Vsix.Resources;
using Acuminator.Vsix.Utilities;


namespace Acuminator.Vsix
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	internal sealed class DisplayNameFromResourcesAttribute : DisplayNameAttribute
	{		
		public string ResourceKey { get; }

		public override string DisplayName => VSIXResource.ResourceManager.GetStringResourceSafe(ResourceKey);

		public DisplayNameFromResourcesAttribute(string resourceKey)
		{
			resourceKey.ThrowOnNullOrWhiteSpace(nameof(resourceKey));
			ResourceKey = resourceKey;
		}	
	}
}