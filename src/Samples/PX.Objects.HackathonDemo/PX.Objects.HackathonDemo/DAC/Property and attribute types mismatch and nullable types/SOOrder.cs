﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class SOOrder : IBqlTable
	{
		#region OrderType
		public abstract class orderType : IBqlField { }

		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Order Type")]
		public string OrderType { get; set; }
		#endregion

		#region OrderNbr
		public abstract class orderNbr : IBqlField { }

		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Order Nbr.")]
		public string OrderNbr { get; set; }
		#endregion

		#region Status
		public abstract class status : IBqlField { }

		[PXStringList(new[] { "N", "O" }, new[] { "New", "Open" })]
		[PXUIField(DisplayName = "Status")]
		public string Status { get; set; }
		#endregion
		
		#region OrderDate
		public abstract class orderDate : IBqlField { }

		[PXDBInt]      
		[PXUIField(DisplayName = "OrderDate")]
		public DateTime? OrderDate { get; set; }
		#endregion

		#region OrderBal
		public abstract class orderBal : IBqlField { }

		[PXDBDecimal]
		[PXUIField(DisplayName = "Order Balance")]
		public decimal OrderBal { get; set; }
		#endregion

		#region tstamp
		public abstract class Tstamp : IBqlField
		{
		}

		[PXDBTimestamp]
		public virtual object tstamp
		{
			get;
			set;
		}
		#endregion
	}
}
