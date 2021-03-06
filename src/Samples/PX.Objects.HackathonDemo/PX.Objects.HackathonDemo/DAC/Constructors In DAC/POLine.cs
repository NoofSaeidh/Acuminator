﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class POLine : IBqlTable
	{
        #region Cons
        public POLine() : base()
        {
        }
        #endregion

        #region ConsParams
        public POLine(string orderType, string orderNbr, string status, DateTime? lineDate)
        {
            OrderType = orderType;
            OrderNbr = orderNbr;
            Status = status;
			LineDate = lineDate;
        }
        #endregion

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
		[PXString]
		public string Status { get; set; }
		#endregion

		#region LineDate
		public abstract class lineDate : IBqlField { }

		[PXDBDate]      
		[PXUIField(DisplayName = "OrderDate")]
		public DateTime? LineDate { get; set; }
        #endregion


        #region tstamp
        public abstract class Tstamp : IBqlField
		{
		}

		[PXDBTimestamp]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion
	}
}
