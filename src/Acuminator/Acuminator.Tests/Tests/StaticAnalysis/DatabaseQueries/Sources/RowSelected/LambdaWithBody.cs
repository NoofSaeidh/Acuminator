﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects
{
	public class SOInvoiceEntry : ARInvoiceEntry
	{
		protected virtual void ARInvoice_CacheAttached(PXCache cache)
		{
			cache.Graph.RowSelected.AddHandler<ARInvoice>((sender, e) =>
			{
				if (!e.IsReadOnly)
				{
					PXDatabase.SelectTimeStamp();
				}
			});
		}
	}
}