using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects
{
	public class SOInvoiceEntry : PXGraph<SOInvoiceEntry, SOInvoice>
	{
		protected virtual void _(Events.RowSelected<SOInvoice> e)
		{
			SOHelpers.InitializeRow(e);
		}

		protected virtual void SOInvoice_RefNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			SOHelpers.InitializeRow(e);
		}

		protected virtual void SOInvoice_RefNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOHelpers.InitializeRow(e);
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

	public static class SOHelpers
	{
		public static void InitializeRow(Events.RowSelected<SOInvoice> e)
		{
			e.Row.RefNbr = "<NEW>";
		}

		public static void InitializeRow(PXFieldDefaultingEventArgs e)
		{
			((SOInvoice) e.Row).RefNbr = "<NEW>";
		}

		public static void InitializeRow(PXFieldVerifyingEventArgs e)
		{
			var row = (SOInvoice) e.Row;
			row.RefNbr = "<NEW>";
		}
	}
}