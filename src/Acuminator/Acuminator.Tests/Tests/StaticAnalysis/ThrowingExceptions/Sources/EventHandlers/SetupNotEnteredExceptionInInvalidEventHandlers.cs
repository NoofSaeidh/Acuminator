﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects
{
	public class SOInvoiceEntry : PXGraph<SOInvoiceEntry, SOInvoice>
	{
		protected virtual void _(Events.FieldDefaulting<SOInvoice, SOInvoice.refNbr> e)
		{
			throw new PXSetupNotEnteredException("Setup is not entered");
		}

		protected virtual void _(Events.FieldVerifying<SOInvoice.refNbr> e)
		{
			throw new PXSetupNotEnteredException("Setup is not entered");
		}

		protected virtual void _(Events.RowSelecting<SOInvoice> e)
		{
			throw new PXSetupNotEnteredException("Setup is not entered");
		}

		protected virtual void _(Events.RowInserting<SOInvoice> e)
		{
			throw new PXSetupNotEnteredException("Setup is not entered");
		}

		protected virtual void _(Events.RowUpdating<SOInvoice> e)
		{
			throw new PXSetupNotEnteredException("Setup is not entered");
		}

		protected virtual void _(Events.RowDeleting<SOInvoice> e)
		{
			throw new PXSetupNotEnteredException("Setup is not entered");
		}

		protected virtual void _(Events.RowInserted<SOInvoice> e)
		{
			throw new PXSetupNotEnteredException("Setup is not entered");
		}

		protected virtual void _(Events.RowUpdated<SOInvoice> e)
		{
			throw new PXSetupNotEnteredException("Setup is not entered");
		}

		protected virtual void _(Events.RowDeleted<SOInvoice> e)
		{
			throw new PXSetupNotEnteredException("Setup is not entered");
		}

		protected virtual void _(Events.RowPersisting<SOInvoice> e)
		{
			throw new PXSetupNotEnteredException("Setup is not entered");
		}

		protected virtual void _(Events.RowPersisted<SOInvoice> e)
		{
			throw new PXSetupNotEnteredException("Setup is not entered");
		}
	}

	public class SOInvoice : IBqlTable
	{
		#region RefNbr
		[PXDBString(8, IsKey = true, InputMask = "")]
		public string RefNbr { get; set; }
		public abstract class refNbr : IBqlField { }
		#endregion	
	}
}